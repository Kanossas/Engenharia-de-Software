using ES2.Models;
using ES2.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ES2.Controllers;

public class RegistoController : Controller
{
    private readonly IRegistoService _registoService;

    public RegistoController(IRegistoService registoService)
    {
        _registoService = registoService;
    }

    [HttpGet]
    public IActionResult Registo()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Registo(RegistoModel model)
    {
        if (ModelState.IsValid)
        {
            await _registoService.RegistarAsync(model);
            return RedirectToAction("Index", "Home");
        }
        return View(model);
    }
}
