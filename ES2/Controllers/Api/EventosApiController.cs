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
    private readonly IInscricaoEventoService _inscricaoEventoService;

    public EventosApiController(AppDbContext context, IInscricaoEventoService inscricaoEventoService)
    {
        _context = context;
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

    [HttpGet("{id:int}/atividades")]
    public async Task<IActionResult> ListarAtividades(int id, CancellationToken ct)
    {
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
                categoria = new { id = a.IdCategoria, nome = a.IdCategoriaNavigation.Nome }
            })
            .ToListAsync(ct);

        return Ok(atividades);
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

            await _inscricaoEventoService.ConfigurarBilhetesEventoAsync(
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
}

