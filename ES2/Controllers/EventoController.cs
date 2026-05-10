using System.Data;
using ES2.Data;
using ES2.DTOs;
using ES2.Models;
using ES2.Services.Inscricoes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ES2.Controllers;

[Authorize] // qualquer utilizador autenticado pode ver eventos
public class EventoController : Controller
{
    private readonly AppDbContext _context;
    private readonly IInscricaoEventoService _inscricaoEventoService;
    private readonly IConfiguradorBilhetesService _configuradorBilhetesService;

    public EventoController(
        AppDbContext context,
        IInscricaoEventoService inscricaoEventoService,
        IConfiguradorBilhetesService configuradorBilhetesService)
    {
        _context = context;
        _inscricaoEventoService = inscricaoEventoService;
        _configuradorBilhetesService = configuradorBilhetesService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? nome, DateOnly? data, string? local, int? idCategoria)
    {
        var query = _context.Eventos.Include(e => e.IdCategoriaNavigation).AsQueryable();

        if (!string.IsNullOrWhiteSpace(nome))
            query = query.Where(e => EF.Functions.ILike(e.Nome, $"%{nome}%"));

        if (data.HasValue)
            query = query.Where(e => e.Data == data.Value);

        if (!string.IsNullOrWhiteSpace(local))
            query = query.Where(e => e.Local != null && EF.Functions.ILike(e.Local, $"%{local}%"));

        if (idCategoria.HasValue)
            query = query.Where(e => e.IdCategoria == idCategoria.Value);

        var eventos = await query
            .OrderBy(e => e.Data)
            .ThenBy(e => e.HoraInicio)
            .ToListAsync();

        ViewBag.Categorias = await _context.Categorias.OrderBy(c => c.Nome).ToListAsync();
        ViewBag.FiltroNome = nome;
        ViewBag.FiltroData = data?.ToString("yyyy-MM-dd");
        ViewBag.FiltroLocal = local;
        ViewBag.FiltroCategoria = idCategoria;

        return View(eventos);
    }

    [HttpGet]
    public async Task<IActionResult> Pesquisar(string? nome, DateOnly? data, string? local, int? idCategoria)
    {
        var query = _context.Eventos.Include(e => e.IdCategoriaNavigation).AsQueryable();

        if (!string.IsNullOrWhiteSpace(nome))
            query = query.Where(e => EF.Functions.ILike(e.Nome, $"%{nome}%"));

        if (data.HasValue)
            query = query.Where(e => e.Data == data.Value);

        if (!string.IsNullOrWhiteSpace(local))
            query = query.Where(e => e.Local != null && EF.Functions.ILike(e.Local, $"%{local}%"));

        if (idCategoria.HasValue)
            query = query.Where(e => e.IdCategoria == idCategoria.Value);

        var eventos = await query
            .OrderBy(e => e.Data)
            .ThenBy(e => e.HoraInicio)
            .ToListAsync();

        return PartialView("_ResultadosEventos", eventos);
    }

    // Só Admin e Organizador podem criar eventos
    [Authorize(Roles = "Admin,Organizador")]
    [HttpGet]
    public async Task<IActionResult> Criar()
    {
        await PrepararFormularioEventoAsync();
        return View(new CriarEventoDto());
    }

    [Authorize(Roles = "Admin,Organizador")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(CriarEventoDto dto)
    {
        if (!ModelState.IsValid)
        {
            await PrepararFormularioEventoAsync();
            return View(dto);
        }

        if (!string.IsNullOrWhiteSpace(dto.NovaCategoria))
            dto.IdCategoria = await ObterOuCriarCategoriaAsync(dto.NovaCategoria);

        var evento = new Evento
        {
            Nome = dto.Nome,
            Data = dto.Data,
            HoraInicio = dto.HoraInicio,
            Local = dto.Local,
            Descricao = dto.Descricao,
            CapMax = dto.Capacidade,
            IdCategoria = dto.IdCategoria
        };

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            _context.Eventos.Add(evento);
            await _context.SaveChangesAsync();
            await GuardarImageUrlSeExistirAsync(evento.IdEvento, dto.ImageUrl);
            await GarantirCategoriaEventoAsync(evento.IdEvento, dto.IdCategoria);

            var bilheteBase = new Bilhete { Nome = "Entrada Normal" };
            _context.Bilhetes.Add(bilheteBase);
            await _context.SaveChangesAsync();

            var bilheteEvento = new BilhetesEvento
            {
                IdEvento = evento.IdEvento,
                IdBilhete = bilheteBase.IdBilhete,
                Preco = Convert.ToDouble(dto.Preco!.Value)
            };

            _context.BilhetesEventos.Add(bilheteEvento);
            await _context.SaveChangesAsync();
            await _configuradorBilhetesService.ConfigurarBilhetesEventoAsync(
                evento.IdEvento,
                dto.Preco!.Value,
                dto.QuantidadeStandard!.Value,
                dto.QuantidadeGold!.Value,
                dto.QuantidadeVip!.Value);
            await transaction.CommitAsync();

            TempData["Sucesso"] = "Evento criado com sucesso.";
            return RedirectToAction("Index", "Home", new { page = "events" });
        }
        catch
        {
            await transaction.RollbackAsync();
            ModelState.AddModelError(string.Empty, "Nao foi possivel criar o evento. Tenta novamente.");
            await PrepararFormularioEventoAsync();
            return View(dto);
        }
    }

    // Só Admin e Organizador podem editar eventos
    [Authorize(Roles = "Admin,Organizador")]
    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var evento = await _context.Eventos
            .Include(e => e.BilhetesEventos)
            .ThenInclude(be => be.IdBilheteNavigation)
            .ThenInclude(b => b.IdTipoNavigation)
            .FirstOrDefaultAsync(e => e.IdEvento == id);

        if (evento == null)
            return NotFound();

        var dto = new CriarEventoDto
        {
            IdEvento = evento.IdEvento,
            Nome = evento.Nome,
            Data = evento.Data,
            HoraInicio = evento.HoraInicio,
            Local = evento.Local ?? string.Empty,
            Descricao = evento.Descricao ?? string.Empty,
            Capacidade = evento.CapMax,
            IdCategoria = evento.IdCategoria,
            Preco = evento.BilhetesEventos.Any()
                ? Convert.ToDecimal(evento.BilhetesEventos.OrderBy(b => b.IdBiEv).First().Preco)
                : 0m,
            QuantidadeStandard = evento.BilhetesEventos
                .FirstOrDefault(b => b.IdBilheteNavigation.IdTipoNavigation != null &&
                                     b.IdBilheteNavigation.IdTipoNavigation.Nome == "Standard")
                ?.QuantidadeDisponivel ?? 0,
            QuantidadeGold = evento.BilhetesEventos
                .FirstOrDefault(b => b.IdBilheteNavigation.IdTipoNavigation != null &&
                                     b.IdBilheteNavigation.IdTipoNavigation.Nome == "Gold")
                ?.QuantidadeDisponivel ?? 0,
            QuantidadeVip = evento.BilhetesEventos
                .FirstOrDefault(b => b.IdBilheteNavigation.IdTipoNavigation != null &&
                                     b.IdBilheteNavigation.IdTipoNavigation.Nome == "VIP")
                ?.QuantidadeDisponivel ?? 0
        };

        await PrepararFormularioEventoAsync(true, id);
        return View("Criar", dto);
    }

    [Authorize(Roles = "Admin,Organizador")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, CriarEventoDto dto)
    {
        if (!ModelState.IsValid)
        {
            await PrepararFormularioEventoAsync(true, id);
            return View("Criar", dto);
        }

        var evento = await _context.Eventos
            .Include(e => e.BilhetesEventos)
            .FirstOrDefaultAsync(e => e.IdEvento == id);

        if (evento == null)
            return NotFound();

        if (!string.IsNullOrWhiteSpace(dto.NovaCategoria))
            dto.IdCategoria = await ObterOuCriarCategoriaAsync(dto.NovaCategoria);

        evento.Nome = dto.Nome;
        evento.Data = dto.Data;
        evento.HoraInicio = dto.HoraInicio;
        evento.Local = dto.Local;
        evento.Descricao = dto.Descricao;
        evento.CapMax = dto.Capacidade;
        evento.IdCategoria = dto.IdCategoria;
        await _context.SaveChangesAsync();
        await GuardarImageUrlSeExistirAsync(evento.IdEvento, dto.ImageUrl);
        await GarantirCategoriaEventoAsync(evento.IdEvento, dto.IdCategoria);
        await _configuradorBilhetesService.ConfigurarBilhetesEventoAsync(
            evento.IdEvento,
            dto.Preco!.Value,
            dto.QuantidadeStandard!.Value,
            dto.QuantidadeGold!.Value,
            dto.QuantidadeVip!.Value);

        TempData["Sucesso"] = "Evento editado com sucesso.";
        return RedirectToAction("Index", "Home", new { page = "events" });
    }

    // Só Admin e Organizador podem ver participantes
    [Authorize(Roles = "Admin,Organizador")]
    [HttpGet]
    public async Task<IActionResult> Participantes(int id)
    {
        var evento = await _context.Eventos
            .Include(e => e.RegistoEventos.Where(r => !r.IsCancelado))
            .ThenInclude(r => r.IdUtiNavigation)
            .FirstOrDefaultAsync(e => e.IdEvento == id);

        if (evento == null)
            return NotFound();

        var dto = new ParticipantesEventoDto
        {
            IdEvento = evento.IdEvento,
            NomeEvento = evento.Nome,
            DataEvento = evento.Data,
            HoraEvento = evento.HoraInicio,
            LocalEvento = evento.Local,
            Participantes = evento.RegistoEventos
                .Where(r => !r.IsCancelado)
                .OrderBy(r => r.IdUtiNavigation.Nome)
                .Select(r => new ParticipanteInscritoDto
                {
                    IdUtilizador = r.IdUtiNavigation.IdUti,
                    Nome = r.IdUtiNavigation.Nome,
                    Email = r.IdUtiNavigation.Email,
                    Telemovel = r.IdUtiNavigation.Telemovel
                })
                .ToList()
        };

        return View(dto);
    }

    // Todos os utilizadores autenticados podem ver detalhes
    [HttpGet]
    public async Task<IActionResult> Detalhes(int id)
    {
        var evento = await _context.Eventos
            .Include(e => e.Atividades)
            .ThenInclude(a => a.IdCategoriaNavigation)
            .Include(e => e.IdCategoriaNavigation)
            .FirstOrDefaultAsync(e => e.IdEvento == id);

        if (evento == null)
            return NotFound();

        var dto = new EventoDetalhesCompraDto
        {
            Evento = evento,
            OfertasBilhete = await _configuradorBilhetesService.GarantirEObterOfertasAsync(evento.IdEvento),
            JaInscrito = (await _inscricaoEventoService.ObterEventosInscritosAsync(User.Identity?.Name)).Contains(evento.IdEvento),
            IdBilheteAtivo = await _inscricaoEventoService.ObterBilheteAtivoDoEventoAsync(evento.IdEvento, User.Identity?.Name)
        };

        return View(dto);
    }

    // Só Admin e Organizador podem gerir eventos
    [Authorize(Roles = "Admin,Organizador")]
    [HttpGet]
    public async Task<IActionResult> Gerir()
    {
        var eventos = await _context.Eventos
            .Include(e => e.IdCategoriaNavigation)
            .Include(e => e.Atividades)
            .OrderBy(e => e.Data)
            .ToListAsync();

        return View(eventos);
    }

    private async Task<int> ObterOuCriarCategoriaAsync(string nomeCategoria)
    {
        var nomeNormalizado = nomeCategoria.Trim();
        var categoriaExistente = await _context.Categorias
            .FirstOrDefaultAsync(c => c.Nome.ToLower() == nomeNormalizado.ToLower());

        if (categoriaExistente != null)
            return categoriaExistente.IdCategoria;

        var novaCategoria = new Categoria { Nome = nomeNormalizado };
        _context.Categorias.Add(novaCategoria);
        await _context.SaveChangesAsync();
        return novaCategoria.IdCategoria;
    }

    private async Task PrepararFormularioEventoAsync(bool emEdicao = false, int? idEvento = null)
    {
        ViewBag.Categorias = await _context.Categorias.OrderBy(c => c.Nome).ToListAsync();
        ViewBag.EmEdicao = emEdicao;
        ViewBag.FormAction = emEdicao ? nameof(Editar) : nameof(Criar);
        ViewBag.EventoId = idEvento;
        ViewBag.TituloFormulario = emEdicao ? "Editar Evento" : "Criar Evento";
        ViewBag.TextoBotaoSubmeter = emEdicao ? "Guardar Alteracoes" : "Criar Evento";
    }

    private async Task GarantirCategoriaEventoAsync(int idEvento, int? idCategoria)
    {
        if (!idCategoria.HasValue)
            return;

        var existe = await _context.CategoriaEventos
            .AnyAsync(c => c.IdEvento == idEvento && c.IdCategoria == idCategoria.Value);

        if (existe)
            return;

        _context.CategoriaEventos.Add(new CategoriaEvento
        {
            IdEvento = idEvento,
            IdCategoria = idCategoria.Value
        });
        await _context.SaveChangesAsync();
    }

    private async Task GuardarImageUrlSeExistirAsync(int idEvento, string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            return;

        var coluna = await TryGetEventoImageUrlColumnAsync();
        if (string.IsNullOrWhiteSpace(coluna))
            return;

        var conn = _context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync();

        var safeCol = coluna.Replace("\"", "\"\"");
        await using var cmd = conn.CreateCommand();
        if (_context.Database.CurrentTransaction != null)
            cmd.Transaction = _context.Database.CurrentTransaction.GetDbTransaction();
        cmd.CommandText = $"""
                           update "ES2"."Evento"
                           set "{safeCol}" = @p_url
                           where "ID_Evento" = @p_id;
                           """;

        var pUrl = cmd.CreateParameter();
        pUrl.ParameterName = "p_url";
        pUrl.Value = imageUrl.Trim();
        cmd.Parameters.Add(pUrl);

        var pId = cmd.CreateParameter();
        pId.ParameterName = "p_id";
        pId.Value = idEvento;
        cmd.Parameters.Add(pId);

        await cmd.ExecuteNonQueryAsync();
    }

    private async Task<string?> TryGetEventoImageUrlColumnAsync()
    {
        var conn = _context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        if (_context.Database.CurrentTransaction != null)
            cmd.Transaction = _context.Database.CurrentTransaction.GetDbTransaction();
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

        var result = await cmd.ExecuteScalarAsync();
        return result as string;
    }
}
