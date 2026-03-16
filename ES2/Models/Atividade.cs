using System;
using System.Collections.Generic;

namespace ES2.Models;

public partial class Atividade
{
    public int IdAtividade { get; set; }

    public int IdEvento { get; set; }

    public string Nome { get; set; } = null!;

    public string Local { get; set; } = null!;

    public int Capacidade { get; set; }

    public int IdCategoria { get; set; }

    public virtual ICollection<FeedbackAtv> FeedbackAtvs { get; set; } = new List<FeedbackAtv>();

    public virtual Categoria IdCategoriaNavigation { get; set; } = null!;

    public virtual Evento IdEventoNavigation { get; set; } = null!;

    public virtual ICollection<RegistoAtividade> RegistoAtividades { get; set; } = new List<RegistoAtividade>();
}
