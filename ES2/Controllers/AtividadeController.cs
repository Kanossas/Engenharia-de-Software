using ES2.DTOs;
using ES2.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ES2.Controllers;

public class AtividadeController : Controller
{
    private readonly IAtividadeRepository _atividadeRepository;

    // O repositório é injetado automaticamente pelo .NET
    public AtividadeController(IAtividadeRepository atividadeRepository)
    {
        _atividadeRepository = atividadeRepository;
    }

    // GET: /Atividade/Editar/5
    // Abre o formulário de edição já preenchido com os dados atuais
    public async Task<IActionResult> Editar(int id)
    {
        var atividade = await _atividadeRepository.GetByIdAsync(id);

        if (atividade == null)
            return NotFound();

        // Preenche o DTO com os dados atuais para mostrar no formulário
        var dto = new EditarAtividadeDto
        {
            Nome = atividade.Nome,
            Local = atividade.Local,
            Capacidade = atividade.Capacidade,
            IdCategoria = atividade.IdCategoria
        };

        // Guarda o ID e o ID do evento na ViewBag para usar na View
        ViewBag.AtividadeId = id;
        ViewBag.EventoId = atividade.IdEvento;

        return View(dto);
    }

    // POST: /Atividade/Editar/5
    // Recebe os dados do formulário e guarda na base de dados
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, EditarAtividadeDto dto)
    {
        // Verifica se os dados são válidos
        if (!ModelState.IsValid)
        {
            ViewBag.AtividadeId = id;
            return View(dto);
        }

        var atividade = await _atividadeRepository.GetByIdAsync(id);
        if (atividade == null)
            return NotFound();

        // Atualiza os campos com os novos valores do formulário
        atividade.Nome = dto.Nome;
        atividade.Local = dto.Local;
        atividade.Capacidade = dto.Capacidade;
        atividade.IdCategoria = dto.IdCategoria;

        // Guarda na base de dados
        await _atividadeRepository.UpdateAsync(atividade);

        TempData["Sucesso"] = "Atividade atualizada com sucesso!";
        // Redireciona para a página do evento a que pertence esta atividade
        return RedirectToAction("Editar", new { id });
    }

    // GET: /Atividade/ConfirmarRemocao/5
    // Mostra uma página de confirmação antes de apagar
    public async Task<IActionResult> ConfirmarRemocao(int id)
    {
        var atividade = await _atividadeRepository.GetByIdAsync(id);

        if (atividade == null)
            return NotFound();

        // Envia a atividade completa para a View mostrar os detalhes
        return View(atividade);
    }

    // POST: /Atividade/Remover/5
    // Apaga a atividade depois da confirmação
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remover(int id)
    {
        var atividade = await _atividadeRepository.GetByIdAsync(id);
        if (atividade == null)
            return NotFound();

        // Guarda o ID do evento antes de apagar para redirecionar depois
        var eventoId = atividade.IdEvento;

        await _atividadeRepository.DeleteAsync(id);

        TempData["Sucesso"] = "Atividade removida com sucesso!";
        // Redireciona para a lista de atividades do evento
        return RedirectToAction("Index", "Evento", new { id = eventoId });
    }
}