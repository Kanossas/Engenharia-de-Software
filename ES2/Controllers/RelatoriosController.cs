using ES2.Data;
using ES2.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ES2.Controllers;

[Authorize(Roles = "Admin")]
public class RelatoriosController : Controller
{
    private readonly AppDbContext _context;

    public RelatoriosController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var eventos = await _context.Eventos
            .Include(e => e.IdCategoriaNavigation)
            .Include(e => e.RegistoEventos)
            .ToListAsync();

        var totalUtilizadores = await _context.Utilizadores.CountAsync();
        var receitaTotal = await _context.Recibos.SumAsync(r => (double?)r.ValorPago) ?? 0;

        var eventosPorMes = eventos
            .Where(e => e.Data.HasValue)
            .GroupBy(e => new { e.Data!.Value.Year, e.Data.Value.Month })
            .Select(g => new EventosPorMesDto
            {
                Ano = g.Key.Year,
                Mes = g.Key.Month,
                NomeMes = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                TotalEventos = g.Count()
            })
            .OrderBy(e => e.Ano).ThenBy(e => e.Mes)
            .ToList();

        var mediaEventosPorMes = eventosPorMes.Any()
            ? Math.Round(eventosPorMes.Average(e => e.TotalEventos), 2)
            : 0;

        var eventosPorCategoria = eventos
            .GroupBy(e => e.IdCategoriaNavigation?.Nome ?? "Sem Categoria")
            .Select(g => new EventosPorCategoriaDto
            {
                Categoria = g.Key,
                TotalEventos = g.Count()
            })
            .OrderByDescending(e => e.TotalEventos)
            .ToList();

        var totalInscricoes = eventos.Sum(e => e.RegistoEventos.Count(r => !r.IsCancelado));
        var mediaParticipantes = eventos.Any()
            ? Math.Round((double)totalInscricoes / eventos.Count, 2)
            : 0;

        var dto = new RelatorioAdminDto
        {
            TotalEventos = eventos.Count,
            TotalUtilizadores = totalUtilizadores,
            TotalInscricoes = totalInscricoes,
            ReceitaTotal = receitaTotal,
            MediaEventosPorMes = mediaEventosPorMes,
            MediaParticipantesPorEvento = mediaParticipantes,
            EventosPorMes = eventosPorMes,
            EventosPorCategoria = eventosPorCategoria
        };

        return View(dto);
    }
}
