using ES2.Data;
using ES2.DTOs;
using ES2.Models;
using ES2.Services.Inscricoes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ES2.Controllers;

[Authorize]
public class EventoController : Controller
{
    private readonly AppDbContext _context;
    private readonly IInscricaoEventoService _inscricaoEventoService;

    public EventoController(AppDbContext context, IInscricaoEventoService inscricaoEventoService)
    {
        _context = context;
        _inscricaoEventoService = inscricaoEventoService;
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

    [HttpGet]
    public async Task<IActionResult> Criar()
    {
        await PrepararFormularioEventoAsync();
        return View(new CriarEventoDto());
    }

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

            var bilheteBase = new Bilhete
            {
                Nome = "Entrada Normal"
            };

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
            await _inscricaoEventoService.ConfigurarBilhetesEventoAsync(
                evento.IdEvento,
                dto.Preco!.Value,
                dto.QuantidadeStandard!.Value,
                dto.QuantidadeGold!.Value,
                dto.QuantidadeVip!.Value);
            await transaction.CommitAsync();

            TempData["Sucesso"] = "Evento criado com sucesso.";
            return RedirectToAction(nameof(Index));
        }
        catch
        {
            await transaction.RollbackAsync();
            ModelState.AddModelError(string.Empty, "Nao foi possivel criar o evento. Tenta novamente.");
            await PrepararFormularioEventoAsync();
            return View(dto);
        }
    }

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
        await _inscricaoEventoService.ConfigurarBilhetesEventoAsync(
            evento.IdEvento,
            dto.Preco!.Value,
            dto.QuantidadeStandard!.Value,
            dto.QuantidadeGold!.Value,
            dto.QuantidadeVip!.Value);

        TempData["Sucesso"] = "Evento editado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

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
            OfertasBilhete = await _inscricaoEventoService.GarantirEObterOfertasAsync(evento.IdEvento),
            JaInscrito = (await _inscricaoEventoService.ObterEventosInscritosAsync(User.Identity?.Name)).Contains(evento.IdEvento),
            IdBilheteAtivo = await _inscricaoEventoService.ObterBilheteAtivoDoEventoAsync(evento.IdEvento, User.Identity?.Name)
        };

        return View(dto);
    }

    private async Task<int> ObterOuCriarCategoriaAsync(string nomeCategoria)
    {
        var nomeNormalizado = nomeCategoria.Trim();
        var categoriaExistente = await _context.Categorias
            .FirstOrDefaultAsync(c => c.Nome.ToLower() == nomeNormalizado.ToLower());

        if (categoriaExistente != null)
            return categoriaExistente.IdCategoria;

        var novaCategoria = new Categoria
        {
            Nome = nomeNormalizado
        };

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
}
   
