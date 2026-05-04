using ES2.Data;
using Microsoft.EntityFrameworkCore;

namespace ES2.Services.Inscricoes.Regras;

public class RegraCapacidadeEvento : IRegraInscricaoEvento
{
    private readonly AppDbContext _context;

    public RegraCapacidadeEvento(AppDbContext context)
    {
        _context = context;
    }

    public async Task<string?> ValidarAsync(InscricaoEventoContexto contexto)
    {
        var capacidadeMaxima = contexto.BilheteEvento.IdEventoNavigation.CapMax;
        if (!capacidadeMaxima.HasValue)
            return null;

        var inscritosAtivos = await _context.RegistoEventos
            .CountAsync(r => r.IdEvento == contexto.BilheteEvento.IdEvento && !r.IsCancelado);

        return inscritosAtivos >= capacidadeMaxima.Value
            ? "Nao foi possivel concluir a inscricao porque o evento ja atingiu a lotacao maxima."
            : null;
    }
}
