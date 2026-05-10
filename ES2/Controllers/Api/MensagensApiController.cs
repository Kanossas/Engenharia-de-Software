using ES2.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ES2.Controllers.Api;

[ApiController]
[Route("api/mensagens")]
[Authorize]
public class MensagensApiController : ControllerBase
{
    private readonly AppDbContext _context;

    public MensagensApiController(AppDbContext context)
    {
        _context = context;
    }
    private async Task<int?> TryGetCurrentUserIdAsync(CancellationToken ct)
    {
        var nome = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(nome))
            return null;

        return await _context.Utilizadores
            .Where(u => u.Nome == nome)
            .Select(u => (int?)u.IdUti)
            .FirstOrDefaultAsync(ct);
    }
//ISTO é a ver se da para mandar lalalaa
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] int take = 15, CancellationToken ct = default)
    {
        var utilizadorId = await TryGetCurrentUserIdAsync(ct);
        if (utilizadorId is null)
            return Unauthorized();

        take = Math.Clamp(take, 1, 50);

        var itens = await _context.Mensagens
            .Where(m => m.IdRecetor == utilizadorId.Value)
            .Select(m => new
            {
                id = m.IdEnvio,
                conteudo = m.Conteudo,
                idEnviador = m.IdEnviador
            })
            .ToListAsync(ct);

        return Ok(new
        {
            count = itens.Count,
            items = itens
        });
    }

    [HttpPost("{id:int}/dismiss")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Dismiss(int id, CancellationToken ct)
    {
        var utilizadorId = await TryGetCurrentUserIdAsync(ct);
        if (utilizadorId is null)
            return Unauthorized();

        var mensagem = await _context.Mensagens
            .FirstOrDefaultAsync(m => m.IdEnvio == id && m.IdRecetor == utilizadorId.Value, ct);

        if (mensagem == null)
            return NotFound();

        _context.Mensagens.Remove(mensagem);
        await _context.SaveChangesAsync(ct);
        return Ok(new { ok = true });
    }
}

