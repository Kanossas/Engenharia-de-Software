namespace ES2.Services.Inscricoes;

public interface IInscricaoEventoService
{
    Task<ResultadoOperacaoInscricao> InscreverAsync(int bilheteEventoId, string nomeUtilizador);
}
