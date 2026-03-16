using System;
using System.Collections.Generic;

namespace ES2.Models;

public partial class BilhetesEvento
{
    public int IdBiEv { get; set; }

    public int IdBilhete { get; set; }

    public int IdEvento { get; set; }

    public double Preco { get; set; }

    public virtual ICollection<BilheteUtil> BilheteUtils { get; set; } = new List<BilheteUtil>();

    public virtual Bilhete IdBilheteNavigation { get; set; } = null!;

    public virtual Evento IdEventoNavigation { get; set; } = null!;
}
