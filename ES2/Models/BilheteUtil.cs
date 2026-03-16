using System;
using System.Collections.Generic;

namespace ES2.Models;

public partial class BilheteUtil
{
    public int IdBiUti { get; set; }

    public int? IdBiEv { get; set; }

    public int? IdUtilizador { get; set; }

    public virtual BilhetesEvento? IdBiEvNavigation { get; set; }

    public virtual Utilizador? IdUtilizadorNavigation { get; set; }

    public virtual ICollection<Recibo> Recibos { get; set; } = new List<Recibo>();
}
