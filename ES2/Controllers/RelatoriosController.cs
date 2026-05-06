using ES2.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ES2.Controllers;

[Authorize(Roles = "Admin")]
public class RelatoriosController : Controller
{
    private readonly IRelatorioService _relatorioService;

    public RelatoriosController(IRelatorioService relatorioService)
    {
        _relatorioService = relatorioService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var dto = await _relatorioService.ObterRelatorioAdminAsync();
        return View(dto);
    }
}
