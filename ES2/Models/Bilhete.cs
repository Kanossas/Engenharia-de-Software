using System;
using System.Collections.Generic;

namespace ES2.Models;

public partial class Bilhete
{
    public int IdBilhete { get; set; }

    public string Nome { get; set; } = null!;

    public int? IdTipo { get; set; }

    public virtual ICollection<BilhetesEvento> BilhetesEventos { get; set; } = new List<BilhetesEvento>();

    public virtual TipoBilhete? IdTipoNavigation { get; set; }
}
