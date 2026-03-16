using System;
using System.Collections.Generic;

namespace ES2.Models;

public partial class Recibo
{
    public int IdRecibo { get; set; }

    public int IdUtilizador { get; set; }

    public int IdBiUti { get; set; }

    public double ValorPago { get; set; }

    public DateOnly Data { get; set; }

    public int? IdTipoPag { get; set; }

    public virtual BilheteUtil IdBiUtiNavigation { get; set; } = null!;

    public virtual TipoPagamento? IdTipoPagNavigation { get; set; }

    public virtual Utilizador IdUtilizadorNavigation { get; set; } = null!;
}
