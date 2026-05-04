using System.ComponentModel.DataAnnotations;

namespace ES2.DTOs;

public class CheckoutBilheteDto
{
    public int IdBilheteEvento { get; set; }

    public int IdEvento { get; set; }

    public string NomeEvento { get; set; } = string.Empty;

    public DateOnly? DataEvento { get; set; }

    public TimeOnly? HoraEvento { get; set; }

    public string? LocalEvento { get; set; }

    public string NomeBilhete { get; set; } = string.Empty;

    public string TipoBilhete { get; set; } = string.Empty;

    public string DescricaoAcesso { get; set; } = string.Empty;

    public decimal Preco { get; set; }

    public int QuantidadeDisponivel { get; set; }

    [Required(ErrorMessage = "O nome e obrigatorio.")]
    public string NomeComprador { get; set; } = string.Empty;

    [Required(ErrorMessage = "O email e obrigatorio.")]
    [EmailAddress(ErrorMessage = "Indica um email valido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "O telemovel e obrigatorio.")]
    [RegularExpression(@"^\d{9}$", ErrorMessage = "O telemovel deve ter 9 digitos.")]
    public string Telemovel { get; set; } = string.Empty;

    [Required(ErrorMessage = "A morada e obrigatoria.")]
    [StringLength(150, ErrorMessage = "A morada nao pode ter mais de 150 caracteres.")]
    public string Morada { get; set; } = string.Empty;

    [Required(ErrorMessage = "Escolhe um metodo de pagamento.")]
    public int? IdTipoPagamento { get; set; }

    public IReadOnlyCollection<OpcaoPagamentoDto> TiposPagamento { get; set; } = Array.Empty<OpcaoPagamentoDto>();
}
