using System;
using System.Collections.Generic;

namespace ES2.Models;

public partial class CategoriaEvento
{
    public int IdCatEve { get; set; }

    public int IdCategoria { get; set; }

    public int IdEvento { get; set; }

    public virtual Categoria IdCategoriaNavigation { get; set; } = null!;

    public virtual Evento IdEventoNavigation { get; set; } = null!;
}
