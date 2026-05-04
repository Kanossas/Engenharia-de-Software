namespace ES2.Services.Inscricoes;

public interface IRegraInscricaoEvento
{
    Task<string?> ValidarAsync(InscricaoEventoContexto contexto);
}
