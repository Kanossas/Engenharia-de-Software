using ES2.Data;
using ES2.Models;
using ES2.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ES2.Services;

// Responsabilidade única: lógica de negócio relacionada com inscrições em bilhetes/eventos.
// O controller trata de HTTP; este serviço trata das regras de negócio.
public class BilheteService : IBilheteService
{
    private readonly AppDbContext _context;

    public BilheteService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Sucesso, string Mensagem)> InscreverAsync(int idBiEv, string nomeUtilizador)
    {
        var utilizador = await _context.Utilizadores
            .FirstOrDefaultAsync(u => u.Nome == nomeUtilizador);

        if (utilizador == null)
            return (false, "Nao foi possivel identificar o utilizador autenticado.");

        var bilheteEvento = await _context.BilhetesEventos
            .Include(be => be.IdEventoNavigation)
            .Include(be => be.IdBilheteNavigation)
            .FirstOrDefaultAsync(be => be.IdBiEv == idBiEv);

        if (bilheteEvento == null)
            return (false, "Bilhete/Evento nao encontrado.");

        var jaTemBilhete = await _context.BilheteUtils
            .AnyAsync(bu => bu.IdUtilizador == utilizador.IdUti && bu.IdBiEv == idBiEv);

        if (jaTemBilhete)
            return (false, "Ja tens este bilhete associado a tua conta.");

        var jaInscritoEvento = await _context.RegistoEventos
            .AnyAsync(r => r.IdUti == utilizador.IdUti &&
                           r.IdEvento == bilheteEvento.IdEvento &&
                           !r.IsCancelado);

        if (jaInscritoEvento)
            return (false, "Ja estas inscrito neste evento.");

        if (bilheteEvento.IdEventoNavigation.CapMax.HasValue)
        {
            var inscritosAtivos = await _context.RegistoEventos
                .CountAsync(r => r.IdEvento == bilheteEvento.IdEvento && !r.IsCancelado);

            if (inscritosAtivos >= bilheteEvento.IdEventoNavigation.CapMax.Value)
                return (false, "Nao foi possivel concluir a inscricao porque o evento ja atingiu a lotacao maxima.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            _context.BilheteUtils.Add(new BilheteUtil
            {
                IdBiEv = bilheteEvento.IdBiEv,
                IdUtilizador = utilizador.IdUti
            });

            var registoEvento = await _context.RegistoEventos
                .FirstOrDefaultAsync(r => r.IdUti == utilizador.IdUti && r.IdEvento == bilheteEvento.IdEvento);

            if (registoEvento == null)
            {
                _context.RegistoEventos.Add(new RegistoEvento
                {
                    IdUti = utilizador.IdUti,
                    IdEvento = bilheteEvento.IdEvento,
                    IsCancelado = false
                });
            }
            else
            {
                registoEvento.IsCancelado = false;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (true, $"Inscricao efetuada com sucesso para o evento {bilheteEvento.IdEventoNavigation.Nome}.");
        }
        catch
        {
            await transaction.RollbackAsync();
            return (false, "Ocorreu um erro ao tentar concluir a inscricao.");
        }
    }

    public async Task<(bool Sucesso, string Mensagem)> CancelarInscricaoAsync(int idBiEv, string nomeUtilizador)
    {
        var utilizador = await _context.Utilizadores
            .FirstOrDefaultAsync(u => u.Nome == nomeUtilizador);

        if (utilizador == null)
            return (false, "Nao foi possivel identificar o utilizador autenticado.");

        var bilheteEvento = await _context.BilhetesEventos
            .FirstOrDefaultAsync(be => be.IdBiEv == idBiEv);

        if (bilheteEvento == null)
            return (false, "Bilhete/Evento nao encontrado.");

        var registoEvento = await _context.RegistoEventos
            .FirstOrDefaultAsync(r => r.IdUti == utilizador.IdUti &&
                                       r.IdEvento == bilheteEvento.IdEvento &&
                                       !r.IsCancelado);

        if (registoEvento == null)
            return (false, "Nao tens uma inscricao ativa neste evento.");

        var bilheteUtil = await _context.BilheteUtils
            .FirstOrDefaultAsync(bu => bu.IdUtilizador == utilizador.IdUti && bu.IdBiEv == idBiEv);

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            registoEvento.IsCancelado = true;

            if (bilheteUtil != null)
                _context.BilheteUtils.Remove(bilheteUtil);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (true, "Inscricao no evento cancelada com sucesso.");
        }
        catch
        {
            await transaction.RollbackAsync();
            return (false, "Ocorreu um erro ao cancelar a inscricao.");
        }
    }

    public async Task<HashSet<int>> ObterEventosInscritosAsync(string nomeUtilizador)
    {
        var utilizador = await _context.Utilizadores
            .FirstOrDefaultAsync(u => u.Nome == nomeUtilizador);

        if (utilizador == null)
            return new HashSet<int>();

        var ids = await _context.RegistoEventos
            .Where(r => r.IdUti == utilizador.IdUti && !r.IsCancelado)
            .Select(r => r.IdEvento)
            .ToListAsync();

        return new HashSet<int>(ids);
    }
}
