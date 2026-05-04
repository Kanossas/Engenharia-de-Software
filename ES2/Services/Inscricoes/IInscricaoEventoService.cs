using ES2.DTOs;

namespace ES2.Services.Inscricoes;

public interface IInscricaoEventoService
{
    Task<ResultadoOperacaoInscricao> InscreverAsync(int bilheteEventoId, string nomeUtilizador);

    Task<IReadOnlyCollection<OfertaBilheteEventoDto>> GarantirEObterOfertasAsync(int eventoId);

    Task ConfigurarBilhetesEventoAsync(int eventoId, decimal precoBase, int quantidadeStandard, int quantidadeGold, int quantidadeVip);

    Task<CheckoutBilheteDto?> ObterCheckoutAsync(int bilheteEventoId, string nomeUtilizador);

    Task<ResultadoOperacaoInscricao> ComprarAsync(CheckoutBilheteDto dto, string nomeUtilizador);

    Task<ResultadoOperacaoInscricao> CancelarAsync(int bilheteEventoId, string nomeUtilizador);

    Task<HashSet<int>> ObterEventosInscritosAsync(string? nomeUtilizador);

    Task<int?> ObterBilheteAtivoDoEventoAsync(int eventoId, string? nomeUtilizador);

    Task<IReadOnlyCollection<HistoricoCompraDto>> ObterHistoricoAsync(string nomeUtilizador);
}
