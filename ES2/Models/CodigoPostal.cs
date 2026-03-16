using System;
using System.Collections.Generic;

namespace ES2.Models;

public partial class CodigoPostal
{
    public int IdCodPostal { get; set; }

    public string CodPostal { get; set; } = null!;

    public virtual ICollection<Utilizador> Utilizadores { get; set; } = new List<Utilizador>();
}
