using System;
using System.Collections.Generic;

namespace ES2.Models;

public partial class TipoPagamento
{
    public int IdTipo { get; set; }

    public string Nome { get; set; } = null!;

    public virtual ICollection<Recibo> Recibos { get; set; } = new List<Recibo>();
}
