using ES2.Models;

namespace ES2.Services.Inscricoes;

public class InscricaoEventoContexto
{
    public Utilizador Utilizador { get; set; } = null!;

    public BilhetesEvento BilheteEvento { get; set; } = null!;
}
