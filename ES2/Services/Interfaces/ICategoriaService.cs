namespace ES2.Services.Interfaces;

public interface ICategoriaService
{
    Task<int> ObterOuCriarAsync(string nomeCategoria);
}
