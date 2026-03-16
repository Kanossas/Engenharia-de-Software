using System;
using System.Collections;
using System.Collections.Generic;

namespace ES2.Models;

public partial class Utilizador
{
    public int IdUti { get; set; }

    public string Nome { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int TipoUti { get; set; }

    public string? Email { get; set; }

    public BitArray? Telemovel { get; set; }

    public string? Morada { get; set; }

    public int? IdCodPostal { get; set; }

    public virtual ICollection<BilheteUtil> BilheteUtils { get; set; } = new List<BilheteUtil>();

    public virtual ICollection<FeedbackAtv> FeedbackAtvs { get; set; } = new List<FeedbackAtv>();

    public virtual ICollection<FeedbackEvnt> FeedbackEvnts { get; set; } = new List<FeedbackEvnt>();

    public virtual CodigoPostal? IdCodPostalNavigation { get; set; }

    public virtual ICollection<Mensagem> MensagenIdEnviadorNavigations { get; set; } = new List<Mensagem>();

    public virtual ICollection<Mensagem> MensagenIdRecetorNavigations { get; set; } = new List<Mensagem>();

    public virtual ICollection<Recibo> Recibos { get; set; } = new List<Recibo>();

    public virtual ICollection<RegistoAtividade> RegistoAtividades { get; set; } = new List<RegistoAtividade>();

    public virtual ICollection<RegistoEvento> RegistoEventos { get; set; } = new List<RegistoEvento>();

    public virtual TipoUtilizador TipoUtiNavigation { get; set; } = null!;
}
