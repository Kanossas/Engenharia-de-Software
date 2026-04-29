using ES2.Data;
using ES2.Models;
using ES2.Repositories.Interfaces;
using ES2.Services.Inscricoes;
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
    private readonly IInscricaoEventoService _inscricaoEventoService;

    public BilheteController(
        AppDbContext context,
        IBilhetesEventoRepository bilhetesEventoRepository,
        ITipoBilheteRepository tipoBilheteRepository,
        IInscricaoEventoService inscricaoEventoService)
    {
        _context = context;
        _bilhetesEventoRepository = bilhetesEventoRepository;
        _tipoBilheteRepository = tipoBilheteRepository;
        _inscricaoEventoService = inscricaoEventoService;
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
        ViewBag.EventosInscritos = await ObterEventosInscritosAsync();

        return View(bilhetes);
    }

    [HttpGet]
    public async Task<IActionResult> Pesquisar(string? nome, DateOnly? data, string? local, int? idTipo)
    {
        var bilhetes = await _bilhetesEventoRepository.GetFilteredWithDetailsAsync(nome, data, local, idTipo);
        ViewBag.EventosInscritos = await ObterEventosInscritosAsync();
        return PartialView("_ResultadosBilhetes", bilhetes);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Inscrever(int id, string? nome, DateOnly? data, string? local, int? idTipo)
    {
        var nomeUtilizador = User.FindFirstValue(ClaimTypes.Name);
        if (string.IsNullOrWhiteSpace(nomeUtilizador))
            return RedirectToAction("Index", "Login");

        var resultado = await _inscricaoEventoService.InscreverAsync(id, nomeUtilizador);
        TempData[resultado.Sucesso ? "Sucesso" : "Erro"] = resultado.Mensagem;

        return RedirectToAction(nameof(Index), new { nome, data, local, idTipo });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelarInscricao(int id)
    {
        var nomeUtilizador = User.FindFirstValue(ClaimTypes.Name);
        if (string.IsNullOrWhiteSpace(nomeUtilizador))
            return RedirectToAction("Index", "Login");

        var utilizador = await _context.Utilizadores
            .FirstOrDefaultAsync(u => u.Nome == nomeUtilizador);

        if (utilizador == null)
        {
            TempData["Erro"] = "Nao foi possivel identificar o utilizador autenticado.";
            return RedirectToAction(nameof(Index));
        }

        var bilheteEvento = await _context.BilhetesEventos
            .FirstOrDefaultAsync(be => be.IdBiEv == id);

        if (bilheteEvento == null)
            return NotFound();

        var registoEvento = await _context.RegistoEventos
            .FirstOrDefaultAsync(r => r.IdUti == utilizador.IdUti &&
                                       r.IdEvento == bilheteEvento.IdEvento &&
                                       !r.IsCancelado);

        if (registoEvento == null)
        {
            TempData["Erro"] = "Nao tens uma inscricao ativa neste evento.";
            return RedirectToAction(nameof(Index));
        }

        var bilheteUtil = await _context.BilheteUtils
            .FirstOrDefaultAsync(bu => bu.IdUtilizador == utilizador.IdUti && bu.IdBiEv == id);

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            registoEvento.IsCancelado = true;

            if (bilheteUtil != null)
                _context.BilheteUtils.Remove(bilheteUtil);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["Sucesso"] = "Inscricao no evento cancelada com sucesso.";
        }
        catch
        {
            await transaction.RollbackAsync();
            TempData["Erro"] = "Ocorreu um erro ao cancelar a inscricao.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<HashSet<int>> ObterEventosInscritosAsync()
    {
        var nomeUtilizador = User.FindFirstValue(ClaimTypes.Name);
        if (string.IsNullOrWhiteSpace(nomeUtilizador))
            return new HashSet<int>();

        var utilizador = await _context.Utilizadores
            .FirstOrDefaultAsync(u => u.Nome == nomeUtilizador);

        if (utilizador == null)
            return new HashSet<int>();

        var ids = await _context.RegistoEventos
            .Where(r => r.IdUti == utilizador.IdUti && !r.IsCancelado)
            .Select(r => r.IdEvento)
            .ToListAsync();

        return new HashSet<int>(ids);
    }
}
