using ES2.Data;
using ES2.DTOs;
using ES2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ES2.Controllers;

[Authorize]
public class EventoController : Controller
{
    private readonly AppDbContext _context;

    public EventoController(AppDbContext context)
    {
        _context = context;
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

        return PartialView("_TabelaEventos", eventos);
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
                : 0m
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

        var bilheteEvento = evento.BilhetesEventos.OrderBy(b => b.IdBiEv).FirstOrDefault();
        if (bilheteEvento != null)
            bilheteEvento.Preco = Convert.ToDouble(dto.Preco!.Value);

        await _context.SaveChangesAsync();

        TempData["Sucesso"] = "Evento editado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<int> ObterOuCriarCategoriaAsync(string nomeCategoria)
    {
        var categoriaExistente = await _context.Categorias
            .FirstOrDefaultAsync(c => c.Nome.ToLower() == nomeCategoria.Trim().ToLower());

        if (categoriaExistente != null)
            return categoriaExistente.IdCategoria;

        var novaCategoria = new Categoria { Nome = nomeCategoria.Trim() };
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
}
