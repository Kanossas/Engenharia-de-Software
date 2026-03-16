using System;
using System.Collections.Generic;

namespace ES2.Models;

public partial class Categoria
{
    public int IdCategoria { get; set; }

    public string Nome { get; set; } = null!;

    public virtual ICollection<Atividade> Atividades { get; set; } = new List<Atividade>();

    public virtual ICollection<CategoriaEvento> CategoriaEventos { get; set; } = new List<CategoriaEvento>();

    public virtual ICollection<Evento> Eventos { get; set; } = new List<Evento>();
}
