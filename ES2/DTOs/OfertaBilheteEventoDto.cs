namespace ES2.DTOs;

public class OfertaBilheteEventoDto
{
    public int IdBilheteEvento { get; set; }

    public string NomeBilhete { get; set; } = string.Empty;

    public string TipoBilhete { get; set; } = string.Empty;

    public string DescricaoAcesso { get; set; } = string.Empty;

    public string ClasseIcone { get; set; } = "bi bi-ticket-perforated";

    public decimal Preco { get; set; }

    public int QuantidadeDisponivel { get; set; }

    public bool Esgotado => QuantidadeDisponivel <= 0;
}
