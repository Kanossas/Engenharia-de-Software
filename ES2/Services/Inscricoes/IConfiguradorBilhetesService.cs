using ES2.DTOs;

namespace ES2.Services.Inscricoes;

public interface IConfiguradorBilhetesService
{
    Task GarantirTiposBaseAsync();
    Task<IReadOnlyCollection<OfertaBilheteEventoDto>> GarantirEObterOfertasAsync(int eventoId);
    Task ConfigurarBilhetesEventoAsync(int eventoId, decimal precoBase, int quantidadeStandard, int quantidadeGold, int quantidadeVip);
}
