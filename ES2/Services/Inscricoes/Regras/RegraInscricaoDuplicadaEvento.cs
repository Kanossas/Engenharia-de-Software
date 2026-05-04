using ES2.Data;
using Microsoft.EntityFrameworkCore;

namespace ES2.Services.Inscricoes.Regras;

public class RegraInscricaoDuplicadaEvento : IRegraInscricaoEvento
{
    private readonly AppDbContext _context;

    public RegraInscricaoDuplicadaEvento(AppDbContext context)
    {
        _context = context;
    }

    public async Task<string?> ValidarAsync(InscricaoEventoContexto contexto)
    {
        var jaInscritoEvento = await _context.RegistoEventos
            .AnyAsync(r => r.IdUti == contexto.Utilizador.IdUti &&
                           r.IdEvento == contexto.BilheteEvento.IdEvento &&
                           !r.IsCancelado);

        return jaInscritoEvento ? "Ja estas inscrito neste evento." : null;
    }
}
