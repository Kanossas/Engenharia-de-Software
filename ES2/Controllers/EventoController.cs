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
    public async Task<IActionResult> Index()
    {
        var eventos = await _context.Eventos
            .OrderBy(e => e.Data)
            .ThenBy(e => e.HoraInicio)
            .ToListAsync();

        return View(eventos);
    }

    [HttpGet]
    public IActionResult Criar()
    {
        return View(new CriarEventoDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(CriarEventoDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var evento = new Evento
        {
            Nome = dto.Nome,
            Data = dto.Data,
            HoraInicio = dto.HoraInicio,
            Local = dto.Local,
            Descricao = dto.Descricao,
            CapMax = dto.Capacidade
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
            ModelState.AddModelError(string.Empty, "Não foi possível criar o evento. Tenta novamente.");
            return View(dto);
        }
    }
}
