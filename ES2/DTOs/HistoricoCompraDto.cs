namespace ES2.DTOs;

public class HistoricoCompraDto
{
    public int IdRecibo { get; set; }

    public string NomeEvento { get; set; } = string.Empty;

    public string NomeBilhete { get; set; } = string.Empty;

    public string TipoBilhete { get; set; } = string.Empty;

    public string DescricaoAcesso { get; set; } = string.Empty;

    public decimal ValorPago { get; set; }

    public DateOnly DataCompra { get; set; }

    public string MetodoPagamento { get; set; } = string.Empty;
}
