using ES2.Data;
using Microsoft.EntityFrameworkCore;

namespace ES2.Services.Inscricoes.Regras;

public class RegraBilheteDuplicado : IRegraInscricaoEvento
{
    private readonly AppDbContext _context;

    public RegraBilheteDuplicado(AppDbContext context)
    {
        _context = context;
    }

    public async Task<string?> ValidarAsync(InscricaoEventoContexto contexto)
    {
        var jaTemBilhete = await _context.BilheteUtils
            .AnyAsync(bu => bu.IdUtilizador == contexto.Utilizador.IdUti &&
                            bu.IdBiEv == contexto.BilheteEvento.IdBiEv &&
                            _context.RegistoEventos.Any(r =>
                                r.IdUti == contexto.Utilizador.IdUti &&
                                r.IdEvento == contexto.BilheteEvento.IdEvento &&
                                !r.IsCancelado));

        return jaTemBilhete ? "Ja tens este bilhete associado a tua conta." : null;
    }
}
