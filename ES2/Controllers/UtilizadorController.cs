using ES2.DTOs;
using ES2.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

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

        if (!string.IsNullOrEmpty(dto.Email) &&
            await _utilizadorRepository.EmailJaExisteAsync(dto.Email, id))
        {
            ModelState.AddModelError("Email", "Este email já está em uso por outro utilizador.");
            ViewBag.UtilizadorId = id;
            return View(dto);
        }

        var utilizador = await _utilizadorRepository.GetByIdAsync(id);
        if (utilizador == null)
            return NotFound();

        utilizador.Nome = dto.Nome;
        utilizador.Email = dto.Email;

        await _utilizadorRepository.UpdateAsync(utilizador);

        TempData["Sucesso"] = "Perfil atualizado com sucesso!";
        return RedirectToAction("EditarPerfil", new { id });
    }
}