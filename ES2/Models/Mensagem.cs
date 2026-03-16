using System;
using System.Collections.Generic;

namespace ES2.Models;

public partial class Mensagem
{
    public int IdEnvio { get; set; }

    public int IdRecetor { get; set; }

    public int IdEnviador { get; set; }

    public string Conteudo { get; set; } = null!;

    public virtual Utilizador IdEnviadorNavigation { get; set; } = null!;

    public virtual Utilizador IdRecetorNavigation { get; set; } = null!;
}
