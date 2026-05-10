using System.Data;
using ES2.Data;
using ES2.Models;
using ES2.Services.Inscricoes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ES2.Controllers.Api;

[ApiController]
[Route("api/eventos")]
public class EventosApiController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguradorBilhetesService _configuradorBilhetes;
    private readonly IInscricaoEventoService _inscricaoEventoService;

    public EventosApiController(
        AppDbContext context,
        IConfiguradorBilhetesService configuradorBilhetes,
        IInscricaoEventoService inscricaoEventoService)
    {
        _context = context;
        _configuradorBilhetes = configuradorBilhetes;
        _inscricaoEventoService = inscricaoEventoService;
    }

    private async Task<string?> TryGetEventoImageUrlColumnAsync(CancellationToken ct)
    {
        var conn = _context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
                          select column_name
                          from information_schema.columns
                          where table_schema ilike 'ES2'
                            and table_name ilike 'Evento'
                            and (
                              lower(column_name) in ('imageurl', 'image_url')
                              or lower(column_name) like '%image%url%'
                            )
                          limit 1;
                          """;

        var result = await cmd.ExecuteScalarAsync(ct);
        return result as string;
    }

    private async Task<Dictionary<int, string?>> LoadImageUrlsAsync(string? columnName, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(columnName))
            return new Dictionary<int, string?>();

        var conn = _context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync(ct);

        // Column name comes from information_schema. Still quote it, and double-quote escape to be safe.
        var safeCol = columnName.Replace("\"", "\"\"");

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"""
                           select "ID_Evento", "{safeCol}"
                           from "ES2"."Evento";
                           """;

        var map = new Dictionary<int, string?>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var id = reader.GetInt32(0);
            var url = reader.IsDBNull(1) ? null : reader.GetString(1);
            map[id] = url;
        }

        return map;
    }

    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken ct)
    {
        var imageCol = await TryGetEventoImageUrlColumnAsync(ct);
        var imageUrls = await LoadImageUrlsAsync(imageCol, ct);

        var eventos = await _context.Eventos
            .Include(e => e.IdCategoriaNavigation)
            .OrderBy(e => e.Data)
            .ThenBy(e => e.HoraInicio)
            .Select(e => new
            {
                id = e.IdEvento,
                nome = e.Nome,
                data = e.Data != null ? e.Data.Value.ToString("yyyy-MM-dd") : null,
                horaInicio = e.HoraInicio != null ? e.HoraInicio.Value.ToString("HH:mm") : null,
                local = e.Local,
                descricao = e.Descricao,
                capacidadeMax = e.CapMax,
                categoria = e.IdCategoriaNavigation != null ? new { id = e.IdCategoriaNavigation.IdCategoria, nome = e.IdCategoriaNavigation.Nome } : null,
                imageUrl = imageUrls.ContainsKey(e.IdEvento) ? imageUrls[e.IdEvento] : null
            })
            .ToListAsync(ct);

        return Ok(eventos);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obter(int id, CancellationToken ct)
    {
        var imageCol = await TryGetEventoImageUrlColumnAsync(ct);
        var imageUrls = await LoadImageUrlsAsync(imageCol, ct);

        var evento = await _context.Eventos
            .Include(e => e.IdCategoriaNavigation)
            .Where(e => e.IdEvento == id)
            .Select(e => new
            {
                id = e.IdEvento,
                nome = e.Nome,
                data = e.Data != null ? e.Data.Value.ToString("yyyy-MM-dd") : null,
                horaInicio = e.HoraInicio != null ? e.HoraInicio.Value.ToString("HH:mm") : null,
                local = e.Local,
                descricao = e.Descricao,
                capacidadeMax = e.CapMax,
                categoria = e.IdCategoriaNavigation != null ? new { id = e.IdCategoriaNavigation.IdCategoria, nome = e.IdCategoriaNavigation.Nome } : null,
                imageUrl = imageUrls.ContainsKey(e.IdEvento) ? imageUrls[e.IdEvento] : null
            })
            .FirstOrDefaultAsync(ct);

        return evento == null ? NotFound() : Ok(evento);
    }

    [HttpGet("{id:int}/bilhetes")]
    public async Task<IActionResult> ListarBilhetes(int id)
    {
        var existeEvento = await _context.Eventos.AnyAsync(e => e.IdEvento == id);
        if (!existeEvento)
            return NotFound();

        var ofertas = await _configuradorBilhetes.GarantirEObterOfertasAsync(id);
        var jaInscrito = (await _inscricaoEventoService.ObterEventosInscritosAsync(User.Identity?.Name)).Contains(id);
        var idBilheteAtivo = await _inscricaoEventoService.ObterBilheteAtivoDoEventoAsync(id, User.Identity?.Name);

        return Ok(new
        {
            jaInscrito,
            idBilheteAtivo,
            ofertas = ofertas.Select(o => new
            {
                idBilheteEvento = o.IdBilheteEvento,
                nomeBilhete = o.NomeBilhete,
                tipoBilhete = o.TipoBilhete,
                descricaoAcesso = o.DescricaoAcesso,
                classeIcone = o.ClasseIcone,
                preco = o.Preco,
                quantidadeDisponivel = o.QuantidadeDisponivel,
                esgotado = o.Esgotado
            })
        });
    }

    [HttpGet("{id:int}/atividades")]
    public async Task<IActionResult> ListarAtividades(int id, CancellationToken ct)
    {
        var nomeUtilizador = User.Identity?.Name;
        var utilizador = string.IsNullOrWhiteSpace(nomeUtilizador)
            ? null
            : await _context.Utilizadores.FirstOrDefaultAsync(u => u.Nome == nomeUtilizador, ct);

        var idsInscritos = new HashSet<int>();
        var acessoAutomaticoAtividades = false;

        if (utilizador != null)
        {
            acessoAutomaticoAtividades = await UtilizadorTemBilheteComAcessoAtividadesAsync(utilizador.IdUti, id, ct);

            if (acessoAutomaticoAtividades)
                await GarantirRegistoAtividadesAsync(utilizador.IdUti, id, ct);

            idsInscritos = (await _context.RegistoAtividades
                    .Where(r => r.IdUti == utilizador.IdUti &&
                                !r.IsCancelado &&
                                r.IdAtividadeNavigation.IdEvento == id)
                    .Select(r => r.IdAtividade)
                    .ToListAsync(ct))
                .ToHashSet();
        }

        var atividades = await _context.Atividades
            .Include(a => a.IdCategoriaNavigation)
            .Where(a => a.IdEvento == id)
            .OrderBy(a => a.Nome)
            .Select((a) => new
            {
                id = a.IdAtividade,
                nome = a.Nome,
                local = a.Local,
                capacidade = a.Capacidade,
                categoria = new { id = a.IdCategoria, nome = a.IdCategoriaNavigation.Nome },
                inscrito = idsInscritos.Contains(a.IdAtividade),
                acessoAutomatico = acessoAutomaticoAtividades
            })
            .ToListAsync(ct);

        return Ok(atividades);
    }

    [Authorize]
    [HttpPost("{id:int}/atividades/{atividadeId:int}/inscricao")]
    public async Task<IActionResult> InscreverAtividade(int id, int atividadeId, CancellationToken ct)
    {
        var resultado = await AlterarInscricaoAtividadeAsync(id, atividadeId, inscrever: true, ct);
        return resultado.Sucesso
            ? Ok(new { message = resultado.Mensagem })
            : BadRequest(new { message = resultado.Mensagem });
    }

    [Authorize]
    [HttpDelete("{id:int}/atividades/{atividadeId:int}/inscricao")]
    public async Task<IActionResult> CancelarInscricaoAtividade(int id, int atividadeId, CancellationToken ct)
    {
        var resultado = await AlterarInscricaoAtividadeAsync(id, atividadeId, inscrever: false, ct);
        return resultado.Sucesso
            ? Ok(new { message = resultado.Mensagem })
            : BadRequest(new { message = resultado.Mensagem });
    }

    public sealed class CriarAtividadeRequest
    {
        public string Nome { get; set; } = string.Empty;
        public string Local { get; set; } = string.Empty;
        public int? Capacidade { get; set; }
        public int? IdCategoria { get; set; }
        public string? NovaCategoriaNome { get; set; }
    }

    [Authorize]
    [HttpPost("{id:int}/atividades")]
    public async Task<IActionResult> CriarAtividade(int id, [FromBody] CriarAtividadeRequest req, CancellationToken ct)
    {
        var existeEvento = await _context.Eventos.AnyAsync(e => e.IdEvento == id, ct);
        if (!existeEvento)
            return NotFound(new { message = "O evento selecionado nao existe." });

        if (string.IsNullOrWhiteSpace(req.Nome))
            return BadRequest(new { message = "O nome da atividade e obrigatorio." });

        if (string.IsNullOrWhiteSpace(req.Local))
            return BadRequest(new { message = "O local da atividade e obrigatorio." });

        if (req.Capacidade is null or <= 0)
            return BadRequest(new { message = "A capacidade deve ser superior a 0." });

        var novaCategoriaNome = req.NovaCategoriaNome?.Trim();
        if (req.IdCategoria is null && string.IsNullOrWhiteSpace(novaCategoriaNome))
            return BadRequest(new { message = "A categoria e obrigatoria." });

        if (!string.IsNullOrWhiteSpace(novaCategoriaNome) && novaCategoriaNome.Length > 40)
            return BadRequest(new { message = "A nova categoria nao pode ter mais de 40 caracteres." });

        Categoria? categoria;
        if (!string.IsNullOrWhiteSpace(novaCategoriaNome))
        {
            categoria = await _context.Categorias
                .FirstOrDefaultAsync(c => c.Nome.ToLower() == novaCategoriaNome.ToLower(), ct)
                ?? new Categoria { Nome = novaCategoriaNome };

            if (categoria.IdCategoria == 0)
            {
                _context.Categorias.Add(categoria);
                await _context.SaveChangesAsync(ct);
            }
        }
        else
        {
            categoria = await _context.Categorias
                .FirstOrDefaultAsync(c => c.IdCategoria == req.IdCategoria!.Value, ct);

            if (categoria == null)
                return BadRequest(new { message = "A categoria selecionada nao existe." });
        }

        var temCategoriaEvento = await _context.CategoriaEventos
            .AnyAsync(c => c.IdEvento == id && c.IdCategoria == categoria.IdCategoria, ct);

        if (!temCategoriaEvento)
        {
            _context.CategoriaEventos.Add(new CategoriaEvento
            {
                IdEvento = id,
                IdCategoria = categoria.IdCategoria
            });
        }

        var atividade = new Atividade
        {
            IdEvento = id,
            Nome = req.Nome.Trim(),
            Local = req.Local.Trim(),
            Capacidade = req.Capacidade.Value,
            IdCategoria = categoria.IdCategoria
        };

        _context.Atividades.Add(atividade);
        await _context.SaveChangesAsync(ct);

        return Ok(new
        {
            id = atividade.IdAtividade,
            nome = atividade.Nome,
            local = atividade.Local,
            capacidade = atividade.Capacidade,
            categoria = new { id = categoria.IdCategoria, nome = categoria.Nome },
            inscrito = false,
            acessoAutomatico = false
        });
    }

    public sealed class CriarEventoRequest
    {
        public string Nome { get; set; } = string.Empty;
        public string? Data { get; set; } // yyyy-MM-dd
        public string? HoraInicio { get; set; } // HH:mm
        public string Local { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public int? Capacidade { get; set; }
        public decimal? Preco { get; set; }
        public int? QuantidadeStandard { get; set; }
        public int? QuantidadeGold { get; set; }
        public int? QuantidadeVip { get; set; }
        public int? IdCategoria { get; set; }
        public string? ImageUrl { get; set; }
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarEventoRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Nome))
            return BadRequest(new { message = "Nome e obrigatorio." });

        if (string.IsNullOrWhiteSpace(req.Data) || !DateOnly.TryParse(req.Data, out var data))
            return BadRequest(new { message = "Data invalida." });

        if (string.IsNullOrWhiteSpace(req.HoraInicio) || !TimeOnly.TryParse(req.HoraInicio, out var horaInicio))
            return BadRequest(new { message = "HoraInicio invalida." });

        if (string.IsNullOrWhiteSpace(req.Local))
            return BadRequest(new { message = "Local e obrigatorio." });

        if (string.IsNullOrWhiteSpace(req.Descricao))
            return BadRequest(new { message = "Descricao e obrigatoria." });

        if (req.Capacidade is null or <= 0)
            return BadRequest(new { message = "Capacidade invalida." });

        if (req.Preco is null or < 0)
            return BadRequest(new { message = "Preco invalido." });

        if (req.QuantidadeStandard is null or < 0 ||
            req.QuantidadeGold is null or < 0 ||
            req.QuantidadeVip is null or < 0)
            return BadRequest(new { message = "As quantidades de bilhetes sao obrigatorias e nao podem ser negativas." });

        if (req.QuantidadeStandard + req.QuantidadeGold + req.QuantidadeVip > req.Capacidade)
            return BadRequest(new { message = "A soma dos bilhetes Standard, Gold e VIP nao pode ultrapassar a capacidade maxima do evento." });

        if (req.QuantidadeStandard + req.QuantidadeGold + req.QuantidadeVip <= 0)
            return BadRequest(new { message = "Deves disponibilizar pelo menos um bilhete." });

        var evento = new Evento
        {
            Nome = req.Nome.Trim(),
            Data = data,
            HoraInicio = horaInicio,
            Local = req.Local.Trim(),
            Descricao = req.Descricao.Trim(),
            CapMax = req.Capacidade,
            IdCategoria = req.IdCategoria
        };

        await using var tx = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            _context.Eventos.Add(evento);
            await _context.SaveChangesAsync(ct);

            // Bilhete base que depois e expandido para Standard/Gold/VIP.
            var bilheteBase = new Bilhete { Nome = "Entrada Normal" };
            _context.Bilhetes.Add(bilheteBase);
            await _context.SaveChangesAsync(ct);

            var bilheteEvento = new BilhetesEvento
            {
                IdEvento = evento.IdEvento,
                IdBilhete = bilheteBase.IdBilhete,
                Preco = Convert.ToDouble(req.Preco.Value)
            };

            _context.BilhetesEventos.Add(bilheteEvento);
            await _context.SaveChangesAsync(ct);

            // Optional ImageUrl: only write if a matching column exists.
            if (!string.IsNullOrWhiteSpace(req.ImageUrl))
            {
                var imageCol = await TryGetEventoImageUrlColumnAsync(ct);
                if (!string.IsNullOrWhiteSpace(imageCol))
                {
                    var conn = _context.Database.GetDbConnection();
                    if (conn.State != ConnectionState.Open)
                        await conn.OpenAsync(ct);

                    var safeCol = imageCol.Replace("\"", "\"\"");
                    await using var cmd = conn.CreateCommand();
                    cmd.CommandText = $"""
                                       update "ES2"."Evento"
                                       set "{safeCol}" = @p_url
                                       where "ID_Evento" = @p_id;
                                       """;
                    var pUrl = cmd.CreateParameter();
                    pUrl.ParameterName = "p_url";
                    pUrl.Value = req.ImageUrl.Trim();
                    cmd.Parameters.Add(pUrl);

                    var pId = cmd.CreateParameter();
                    pId.ParameterName = "p_id";
                    pId.Value = evento.IdEvento;
                    cmd.Parameters.Add(pId);

                    await cmd.ExecuteNonQueryAsync(ct);
                }
            }

            await _configuradorBilhetes.ConfigurarBilhetesEventoAsync(
                evento.IdEvento,
                req.Preco.Value,
                req.QuantidadeStandard.Value,
                req.QuantidadeGold.Value,
                req.QuantidadeVip.Value);
            await tx.CommitAsync(ct);
            return Ok(new { id = evento.IdEvento });
        }
        catch
        {
            await tx.RollbackAsync(ct);
            return Problem("Nao foi possivel criar o evento.");
        }
    }

    private async Task<(bool Sucesso, string Mensagem)> AlterarInscricaoAtividadeAsync(
        int eventoId,
        int atividadeId,
        bool inscrever,
        CancellationToken ct)
    {
        var nomeUtilizador = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(nomeUtilizador))
            return (false, "Nao foi possivel identificar o utilizador autenticado.");

        var utilizador = await _context.Utilizadores
            .FirstOrDefaultAsync(u => u.Nome == nomeUtilizador, ct);

        if (utilizador == null)
            return (false, "Nao foi possivel identificar o utilizador autenticado.");

        if (await UtilizadorTemBilheteComAcessoAtividadesAsync(utilizador.IdUti, eventoId, ct))
            return (false, "As atividades ja estao incluidas automaticamente no teu bilhete Gold/VIP.");

        var atividade = await _context.Atividades
            .FirstOrDefaultAsync(a => a.IdAtividade == atividadeId && a.IdEvento == eventoId, ct);

        if (atividade == null)
            return (false, "A atividade selecionada nao existe.");

        var registo = await _context.RegistoAtividades
            .FirstOrDefaultAsync(r => r.IdUti == utilizador.IdUti && r.IdAtividade == atividadeId, ct);

        if (inscrever)
        {
            if (registo != null && !registo.IsCancelado)
                return (false, "Ja estas inscrito nesta atividade.");

            var inscritosAtivos = await _context.RegistoAtividades
                .CountAsync(r => r.IdAtividade == atividadeId && !r.IsCancelado, ct);

            if (inscritosAtivos >= atividade.Capacidade)
                return (false, "Esta atividade ja atingiu a capacidade maxima.");

            if (registo == null)
            {
                _context.RegistoAtividades.Add(new RegistoAtividade
                {
                    IdUti = utilizador.IdUti,
                    IdAtividade = atividadeId,
                    IsCancelado = false
                });
            }
            else
            {
                registo.IsCancelado = false;
            }

            await _context.SaveChangesAsync(ct);
            return (true, $"Inscricao na atividade '{atividade.Nome}' efetuada com sucesso.");
        }

        if (registo == null || registo.IsCancelado)
            return (false, "Nao tens uma inscricao ativa nesta atividade.");

        registo.IsCancelado = true;
        await _context.SaveChangesAsync(ct);

        return (true, "Inscricao na atividade cancelada com sucesso.");
    }

    private async Task<bool> UtilizadorTemBilheteComAcessoAtividadesAsync(int utilizadorId, int eventoId, CancellationToken ct)
    {
        var temEventoAtivo = await _context.RegistoEventos.AnyAsync(r =>
            r.IdUti == utilizadorId &&
            r.IdEvento == eventoId &&
            !r.IsCancelado, ct);

        if (!temEventoAtivo)
            return false;

        var tipoBilhete = await _context.BilheteUtils
            .Where(bu => bu.IdUtilizador == utilizadorId &&
                         bu.IdBiEvNavigation != null &&
                         bu.IdBiEvNavigation.IdEvento == eventoId)
            .OrderByDescending(bu => bu.IdBiUti)
            .Select(bu => bu.IdBiEvNavigation!.IdBilheteNavigation.IdTipoNavigation!.Nome)
            .FirstOrDefaultAsync(ct);

        return string.Equals(tipoBilhete, "Gold", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(tipoBilhete, "VIP", StringComparison.OrdinalIgnoreCase);
    }

    private async Task GarantirRegistoAtividadesAsync(int utilizadorId, int eventoId, CancellationToken ct)
    {
        var atividades = await _context.Atividades
            .Where(a => a.IdEvento == eventoId)
            .Select(a => a.IdAtividade)
            .ToListAsync(ct);

        foreach (var atividadeId in atividades)
        {
            var registo = await _context.RegistoAtividades
                .FirstOrDefaultAsync(r => r.IdUti == utilizadorId && r.IdAtividade == atividadeId, ct);

            if (registo == null)
            {
                _context.RegistoAtividades.Add(new RegistoAtividade
                {
                    IdUti = utilizadorId,
                    IdAtividade = atividadeId,
                    IsCancelado = false
                });
            }
            else
            {
                registo.IsCancelado = false;
            }
        }

        await _context.SaveChangesAsync(ct);
    }
}
