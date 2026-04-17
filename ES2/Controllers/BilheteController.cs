using ES2.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ES2.Controllers;

[Authorize]
public class BilheteController : Controller
{
    private readonly IBilhetesEventoRepository _bilhetesEventoRepository;
    private readonly ITipoBilheteRepository _tipoBilheteRepository;

    public BilheteController(
        IBilhetesEventoRepository bilhetesEventoRepository,
        ITipoBilheteRepository tipoBilheteRepository)
    {
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
}
