using ES2.DTOs;
using ES2.Models;
using ES2.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace ES2.Controllers;

[Authorize]
public class UtilizadorController : Controller
{
    private readonly IUtilizadorRepository _utilizadorRepository;

    public UtilizadorController(IUtilizadorRepository utilizadorRepository)
    {
        _utilizadorRepository = utilizadorRepository;
    }

    [HttpGet]
    public async Task<IActionResult> EditarPerfil(int id)
    {
        var utilizador = await _utilizadorRepository.GetByIdAsync(id);

        if (utilizador == null)
            return NotFound();

        var dto = new EditarPerfilDto
        {
            Nome = utilizador.Nome,
            Email = utilizador.Email,
            Telemovel = utilizador.Telemovel != null ? utilizador.Telemovel.ToString() : null
        };

        ViewBag.UtilizadorId = id;
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarPerfil(int id, EditarPerfilDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.UtilizadorId = id;
            return View(dto);
        }

        var utilizador = await _utilizadorRepository.GetByIdAsync(id);
        if (utilizador == null) return NotFound();

        utilizador.Nome = dto.Nome;
        utilizador.Email = dto.Email;

        if (!string.IsNullOrEmpty(dto.Telemovel))
            utilizador.Telemovel = dto.Telemovel;

        await _utilizadorRepository.UpdateAsync(utilizador);

        // Atualiza o cookie mantendo o Role e o Id corretos
        var role = utilizador.TipoUti switch
        {
            1 => "Admin",
            3 => "Organizador",
            _ => "Utilizador"
        };

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, utilizador.Nome),
            new Claim(ClaimTypes.Email, utilizador.Email ?? ""),
            new Claim(ClaimTypes.NameIdentifier, utilizador.IdUti.ToString()),
            new Claim(ClaimTypes.Role, role)
        };

        var identity = new ClaimsIdentity(claims, "CookieAuth");
        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync("CookieAuth", principal);

        TempData["Sucesso"] = "Perfil atualizado com sucesso!";
        return RedirectToAction("EditarPerfil", new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Detalhes()
    {
        var nomeLogado = User.Identity?.Name;

        if (string.IsNullOrEmpty(nomeLogado))
            return RedirectToAction("Index", "Login");

        var utilizadores = await _utilizadorRepository.GetAllAsync();
        var utilizador = utilizadores.FirstOrDefault(u => u.Nome == nomeLogado);

        if (utilizador == null)
            return NotFound();

        return View(utilizador);
    }
}