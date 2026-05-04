using ES2.Data;
using ES2.Models;
using Microsoft.EntityFrameworkCore;

namespace ES2.Services.Inscricoes;

public class InscricaoEventoService : IInscricaoEventoService
{
    private readonly AppDbContext _context;
    private readonly IEnumerable<IRegraInscricaoEvento> _regras;

    public InscricaoEventoService(AppDbContext context, IEnumerable<IRegraInscricaoEvento> regras)
    {
        _context = context;
        _regras = regras;
    }

    public async Task<ResultadoOperacaoInscricao> InscreverAsync(int bilheteEventoId, string nomeUtilizador)
    {
        var utilizador = await _context.Utilizadores
            .FirstOrDefaultAsync(u => u.Nome == nomeUtilizador);

        if (utilizador == null)
            return ResultadoOperacaoInscricao.Falha("Nao foi possivel identificar o utilizador autenticado.");

        var bilheteEvento = await _context.BilhetesEventos
            .Include(be => be.IdEventoNavigation)
            .Include(be => be.IdBilheteNavigation)
            .FirstOrDefaultAsync(be => be.IdBiEv == bilheteEventoId);

        if (bilheteEvento == null)
            return ResultadoOperacaoInscricao.Falha("O bilhete selecionado nao existe.");

        var contexto = new InscricaoEventoContexto
        {
            Utilizador = utilizador,
            BilheteEvento = bilheteEvento
        };

        foreach (var regra in _regras)
        {
            var erro = await regra.ValidarAsync(contexto);
            if (!string.IsNullOrWhiteSpace(erro))
                return ResultadoOperacaoInscricao.Falha(erro);
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

            return ResultadoOperacaoInscricao.Ok(
                $"Inscricao efetuada com sucesso para o evento {bilheteEvento.IdEventoNavigation.Nome}.");
        }
        catch
        {
            await transaction.RollbackAsync();
            return ResultadoOperacaoInscricao.Falha("Ocorreu um erro ao tentar concluir a inscricao.");
        }
    }
}
