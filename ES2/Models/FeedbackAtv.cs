using System;
using System.Collections.Generic;

namespace ES2.Models;

public partial class FeedbackAtv
{
    public int IdFbati { get; set; }

    public int IdAtividade { get; set; }

    public int IdUti { get; set; }

    public string Descricao { get; set; } = null!;

    public virtual Atividade IdAtividadeNavigation { get; set; } = null!;

    public virtual Utilizador IdUtiNavigation { get; set; } = null!;
}
