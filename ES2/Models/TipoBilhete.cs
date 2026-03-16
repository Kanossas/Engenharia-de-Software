using System;
using System.Collections.Generic;

namespace ES2.Models;

public partial class TipoBilhete
{
    public int IdTipo { get; set; }

    public string? Nome { get; set; }

    public virtual ICollection<Bilhete> Bilhetes { get; set; } = new List<Bilhete>();
}
