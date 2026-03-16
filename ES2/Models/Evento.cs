using System;
using System.Collections.Generic;

namespace ES2.Models;

public partial class Evento
{
    public int IdEvento { get; set; }

    public string Nome { get; set; } = null!;

    public DateOnly? Data { get; set; }

    public string? Local { get; set; }

    public string? Descricao { get; set; }

    public int? CapMax { get; set; }

    public int? IdCategoria { get; set; }

    public TimeOnly? HoraInicio { get; set; }

    public TimeOnly? HoraFim { get; set; }

    public virtual ICollection<Atividade> Atividades { get; set; } = new List<Atividade>();

    public virtual ICollection<BilhetesEvento> BilhetesEventos { get; set; } = new List<BilhetesEvento>();

    public virtual ICollection<CategoriaEvento> CategoriaEventos { get; set; } = new List<CategoriaEvento>();

    public virtual ICollection<FeedbackEvnt> FeedbackEvnts { get; set; } = new List<FeedbackEvnt>();

    public virtual Categoria? IdCategoriaNavigation { get; set; }

    public virtual ICollection<RegistoEvento> RegistoEventos { get; set; } = new List<RegistoEvento>();
}
