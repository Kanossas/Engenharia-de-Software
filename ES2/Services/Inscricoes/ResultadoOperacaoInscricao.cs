namespace ES2.Services.Inscricoes;

public class ResultadoOperacaoInscricao
{
    public bool Sucesso { get; init; }

    public string Mensagem { get; init; } = string.Empty;

    public static ResultadoOperacaoInscricao Ok(string mensagem) => new()
    {
        Sucesso = true,
        Mensagem = mensagem
    };

    public static ResultadoOperacaoInscricao Falha(string mensagem) => new()
    {
        Sucesso = false,
        Mensagem = mensagem
    };
}
