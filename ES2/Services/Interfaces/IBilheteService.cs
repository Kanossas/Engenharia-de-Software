namespace ES2.Services.Interfaces;

public interface IBilheteService
{
    Task<(bool Sucesso, string Mensagem)> InscreverAsync(int idBiEv, string nomeUtilizador);
    Task<(bool Sucesso, string Mensagem)> CancelarInscricaoAsync(int idBiEv, string nomeUtilizador);
    Task<HashSet<int>> ObterEventosInscritosAsync(string nomeUtilizador);
}
