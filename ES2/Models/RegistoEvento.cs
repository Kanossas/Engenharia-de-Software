using System;
using System.Collections.Generic;

namespace ES2.Models;

public partial class RegistoEvento
{
    public int IdRegEv { get; set; }

    public int IdUti { get; set; }

    public int IdEvento { get; set; }

    public bool IsCancelado { get; set; }

    public virtual Evento IdEventoNavigation { get; set; } = null!;

    public virtual Utilizador IdUtiNavigation { get; set; } = null!;
}
