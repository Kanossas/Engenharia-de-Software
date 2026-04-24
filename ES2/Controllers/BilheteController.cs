using ES2.Data;
using ES2.Models;
using ES2.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ES2.Controllers;

[Authorize]
public class BilheteController : Controller
{
    private readonly AppDbContext _context;
    private readonly IBilhetesEventoRepository _bilhetesEventoRepository;
    private readonly ITipoBilheteRepository _tipoBilheteRepository;

    public BilheteController(
        AppDbContext context,
        IBilhetesEventoRepository bilhetesEventoRepository,
        ITipoBilheteRepository tipoBilheteRepository)
    {
        _context = context;
        _bilhetesEventoRepository = bilhetesEventoRepository;
        _tipoBilheteRepository = tipoBilheteRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? nome, DateOnly? data, string? local, int? idTipo)
    {
        var bilhetes = await _bilhetesEventoRepository.GetFilteredWithDetailsAsync(nome, data, local, idTipo);

        ViewBag.TiposBilhete = (await _tipoBilheteRepository.GetAllAsync()).OrderBy(t => t.Nome);
        ViewBag.FiltroNome = nome;
        ViewBag.FiltroData = data?.ToString("yyyy-MM-dd");
        ViewBag.FiltroLocal = local;
        ViewBag.FiltroTipo = idTipo;

        return View(bilhetes);
    }

    [HttpGet]
    public async Task<IActionResult> Pesquisar(string? nome, DateOnly? data, string? local, int? idTipo)
    {
        var bilhetes = await _bilhetesEventoRepository.GetFilteredWithDetailsAsync(nome, data, local, idTipo);
        return PartialView("_ResultadosBilhetes", bilhetes);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Inscrever(int id, string? nome, DateOnly? data, string? local, int? idTipo)
    {
        var nomeUtilizador = User.FindFirstValue(ClaimTypes.Name);
        if (string.IsNullOrWhiteSpace(nomeUtilizador))
            return RedirectToAction("Index", "Login");

        var utilizador = await _context.Utilizadores
            .FirstOrDefaultAsync(u => u.Nome == nomeUtilizador);

        if (utilizador == null)
        {
            TempData["Erro"] = "Nao foi possivel identificar o utilizador autenticado.";
            return RedirectToAction(nameof(Index), new { nome, data, local, idTipo });
        }

        var bilheteEvento = await _context.BilhetesEventos
            .Include(be => be.IdEventoNavigation)
            .Include(be => be.IdBilheteNavigation)
            .FirstOrDefaultAsync(be => be.IdBiEv == id);

        if (bilheteEvento == null)
            return NotFound();

        var jaTemBilhete = await _context.BilheteUtils
            .AnyAsync(bu => bu.IdUtilizador == utilizador.IdUti && bu.IdBiEv == id);

        if (jaTemBilhete)
        {
            TempData["Erro"] = "Ja tens este bilhete associado a tua conta.";
            return RedirectToAction(nameof(Index), new { nome, data, local, idTipo });
        }

        var jaInscritoEvento = await _context.RegistoEventos
            .AnyAsync(r => r.IdUti == utilizador.IdUti &&
                           r.IdEvento == bilheteEvento.IdEvento &&
                           !r.IsCancelado);

        if (jaInscritoEvento)
        {
            TempData["Erro"] = "Ja estas inscrito neste evento.";
            return RedirectToAction(nameof(Index), new { nome, data, local, idTipo });
        }

        if (bilheteEvento.IdEventoNavigation.CapMax.HasValue)
        {
            var inscritosAtivos = await _context.RegistoEventos
                .CountAsync(r => r.IdEvento == bilheteEvento.IdEvento && !r.IsCancelado);

            if (inscritosAtivos >= bilheteEvento.IdEventoNavigation.CapMax.Value)
            {
                TempData["Erro"] = "Nao foi possivel concluir a inscricao porque o evento ja atingiu a lotacao maxima.";
                return RedirectToAction(nameof(Index), new { nome, data, local, idTipo });
            }
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            _context.BilheteUtils.Add(new BilheteUtil
            {
                IdBiEv = bilheteEvento.IdBiEv,
                IdUtilizador = utilizador.IdUti
            });

            var registoEvento = await _context.RegistoEventos
                .FirstOrDefaultAsync(r => r.IdUti == utilizador.IdUti && r.IdEvento == bilheteEvento.IdEvento);

            if (registoEvento == null)
            {
                _context.RegistoEventos.Add(new RegistoEvento
                {
                    IdUti = utilizador.IdUti,
                    IdEvento = bilheteEvento.IdEvento,
                    IsCancelado = false
                });
            }
            else
            {
                registoEvento.IsCancelado = false;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["Sucesso"] = $"Inscricao efetuada com sucesso para o evento {bilheteEvento.IdEventoNavigation.Nome}.";
        }
        catch
        {
            await transaction.RollbackAsync();
            TempData["Erro"] = "Ocorreu um erro ao tentar concluir a inscricao.";
        }

        return RedirectToAction(nameof(Index), new { nome, data, local, idTipo });
    }
}
