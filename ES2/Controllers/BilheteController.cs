using ES2.DTOs;
using ES2.Models;
using ES2.Repositories.Interfaces;
using ES2.Services.Inscricoes;
using ES2.Data;
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
    private readonly IConfiguradorBilhetesService _configuradorBilhetes;

    public BilheteController(
        AppDbContext context,
        IBilhetesEventoRepository bilhetesEventoRepository,
        ITipoBilheteRepository tipoBilheteRepository,
        IInscricaoEventoService inscricaoEventoService,
        IConfiguradorBilhetesService configuradorBilhetes)
    {
        _context = context;
        _bilhetesEventoRepository = bilhetesEventoRepository;
        _tipoBilheteRepository = tipoBilheteRepository;
        _inscricaoEventoService = inscricaoEventoService;
        _configuradorBilhetes = configuradorBilhetes;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? nome, DateOnly? data, string? local, int? idTipo)
    {
        await GarantirBilhetesDosEventosAsync();
        var bilhetes = await _bilhetesEventoRepository.GetFilteredWithDetailsAsync(nome, data, local, idTipo);

        ViewBag.TiposBilhete = (await _tipoBilheteRepository.GetAllAsync()).OrderBy(t => t.Nome);
        ViewBag.FiltroNome = nome;
        ViewBag.FiltroData = data?.ToString("yyyy-MM-dd");
        ViewBag.FiltroLocal = local;
        ViewBag.FiltroTipo = idTipo;
        ViewBag.EventosInscritos = await _inscricaoEventoService.ObterEventosInscritosAsync(User.Identity?.Name);

        return View(bilhetes);
    }

    [HttpGet]
    public async Task<IActionResult> Pesquisar(string? nome, DateOnly? data, string? local, int? idTipo)
    {
        await GarantirBilhetesDosEventosAsync();
        var bilhetes = await _bilhetesEventoRepository.GetFilteredWithDetailsAsync(nome, data, local, idTipo);
        ViewBag.EventosInscritos = await _inscricaoEventoService.ObterEventosInscritosAsync(User.Identity?.Name);
        return PartialView("_ResultadosBilhetes", bilhetes);
    }

    [HttpGet]
    public async Task<IActionResult> Checkout(int id)
    {
        var nomeUtilizador = User.FindFirstValue(ClaimTypes.Name);
        if (string.IsNullOrWhiteSpace(nomeUtilizador))
            return RedirectToAction("Index", "Login");

        var dto = await _inscricaoEventoService.ObterCheckoutAsync(id, nomeUtilizador);
        if (dto == null)
            return NotFound();

        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CheckoutBilheteDto dto)
    {
        var nomeUtilizador = User.FindFirstValue(ClaimTypes.Name);
        if (string.IsNullOrWhiteSpace(nomeUtilizador))
            return RedirectToAction("Index", "Login");

        if (!ModelState.IsValid)
        {
            var checkout = await _inscricaoEventoService.ObterCheckoutAsync(dto.IdBilheteEvento, nomeUtilizador);
            if (checkout == null)
                return NotFound();

            dto.NomeEvento = checkout.NomeEvento;
            dto.DataEvento = checkout.DataEvento;
            dto.HoraEvento = checkout.HoraEvento;
            dto.LocalEvento = checkout.LocalEvento;
            dto.NomeBilhete = checkout.NomeBilhete;
            dto.TipoBilhete = checkout.TipoBilhete;
            dto.DescricaoAcesso = checkout.DescricaoAcesso;
            dto.Preco = checkout.Preco;
            dto.QuantidadeDisponivel = checkout.QuantidadeDisponivel;
            dto.TiposPagamento = checkout.TiposPagamento;

            return View(dto);
        }

        var resultado = await _inscricaoEventoService.ComprarAsync(dto, nomeUtilizador);
        TempData[resultado.Sucesso ? "Sucesso" : "Erro"] = resultado.Mensagem;

        if (!resultado.Sucesso)
        {
            var checkout = await _inscricaoEventoService.ObterCheckoutAsync(dto.IdBilheteEvento, nomeUtilizador);
            if (checkout != null)
                dto.TiposPagamento = checkout.TiposPagamento;

            return View(dto);
        }

        return RedirectToAction(nameof(HistoricoCompras));
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

        var resultado = await _inscricaoEventoService.CancelarAsync(id, nomeUtilizador);
        TempData[resultado.Sucesso ? "Sucesso" : "Erro"] = resultado.Mensagem;

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> HistoricoCompras()
    {
        var nomeUtilizador = User.FindFirstValue(ClaimTypes.Name);
        if (string.IsNullOrWhiteSpace(nomeUtilizador))
            return RedirectToAction("Index", "Login");

        var historico = await _inscricaoEventoService.ObterHistoricoAsync(nomeUtilizador);
        return View(historico);
    }

    private async Task GarantirBilhetesDosEventosAsync()
    {
        var idsEventos = await _context.Eventos
            .Select(e => e.IdEvento)
            .ToListAsync();

        foreach (var idEvento in idsEventos)
            await _configuradorBilhetes.GarantirEObterOfertasAsync(idEvento);
    }
}
