using System;
using System.Collections.Generic;

namespace ES2.Models;

public partial class RegistoAtividade
{
    public int IdRegAt { get; set; }

    public int IdUti { get; set; }

    public int IdAtividade { get; set; }

    public bool IsCancelado { get; set; }

    public virtual Atividade IdAtividadeNavigation { get; set; } = null!;

    public virtual Utilizador IdUtiNavigation { get; set; } = null!;
}
