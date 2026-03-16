using System;
using System.Collections.Generic;

namespace ES2.Models;

public partial class TipoUtilizador
{
    public int IdTpUti { get; set; }

    public string Nome { get; set; } = null!;

    public int NvPerm { get; set; }

    public virtual ICollection<Utilizador> Utilizadores { get; set; } = new List<Utilizador>();
}
