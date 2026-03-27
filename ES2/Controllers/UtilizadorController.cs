using ES2.DTOs;
using ES2.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace ES2.Controllers;

public class UtilizadorController : Controller
{
    private readonly IUtilizadorRepository _utilizadorRepository;

    public UtilizadorController(IUtilizadorRepository utilizadorRepository)
    {
        _utilizadorRepository = utilizadorRepository;
    }

    // GET: /Utilizador/EditarPerfil/5
    // Abre a página com o formulário já preenchido com os dados atuais
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

    // POST: /Utilizador/EditarPerfil/5
    // Recebe os dados do formulário e guarda na base de dados
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

        // 1. Grava as alterações (Nome, Email e o Telemóvel que faltava)
        utilizador.Nome = dto.Nome;
        utilizador.Email = dto.Email;
    
        // Se o DTO trouxer um telemóvel, guarda-o diretamente (já é string)
        if (!string.IsNullOrEmpty(dto.Telemovel))
        {
            utilizador.Telemovel = dto.Telemovel;
        }

        await _utilizadorRepository.UpdateAsync(utilizador);

        // 2. Resolve o erro de Logout: Atualiza o Cookie com o novo Nome
        var claims = new List<Claim> { new Claim(ClaimTypes.Name, utilizador.Nome) };
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync("CookieAuth", new ClaimsPrincipal(claimsIdentity));

        TempData["Sucesso"] = "Perfil atualizado com sucesso!";
        return RedirectToAction("EditarPerfil", new { id });
    }
    
    // GET: /Utilizador/Detalhes
    // Esta função vai buscar os dados do utilizador logado para mostrar na página
    public async Task<IActionResult> Detalhes()
    {
        // 1. Obtém o nome do utilizador que está guardado no Cookie de autenticação
        var nomeLogado = User.Identity?.Name;

        if (string.IsNullOrEmpty(nomeLogado))
        {
            return RedirectToAction("Index", "Login");
        }

        // 2. Procura no repositório o utilizador que tem esse nome
        // (Assumindo que o teu GetAllAsync traz a lista de utilizadores)
        var utilizadores = await _utilizadorRepository.GetAllAsync();
        var utilizador = utilizadores.FirstOrDefault(u => u.Nome == nomeLogado);

        if (utilizador == null)
        {
            return NotFound();
        }

        // 3. Envia os dados para a View (Views/Utilizador/Detalhes.cshtml)
        return View(utilizador);
    }
    
    
}