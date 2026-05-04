using System.ComponentModel.DataAnnotations;

namespace ES2.DTOs;

public class CriarEventoDto : IValidatableObject
{
    public int? IdEvento { get; set; }

    [Required(ErrorMessage = "O nome do evento e obrigatorio.")]
    [StringLength(100, ErrorMessage = "O nome do evento nao pode ter mais de 100 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "A data do evento e obrigatoria.")]
    [DataType(DataType.Date)]
    public DateOnly? Data { get; set; }

    [Required(ErrorMessage = "A hora do evento e obrigatoria.")]
    [DataType(DataType.Time)]
    public TimeOnly? HoraInicio { get; set; }

    [Required(ErrorMessage = "O local do evento e obrigatorio.")]
    [StringLength(50, ErrorMessage = "O local nao pode ter mais de 50 caracteres.")]
    public string Local { get; set; } = string.Empty;

    [Required(ErrorMessage = "A descricao do evento e obrigatoria.")]
    [StringLength(1000, ErrorMessage = "A descricao nao pode ter mais de 1000 caracteres.")]
    public string Descricao { get; set; } = string.Empty;

    [Required(ErrorMessage = "A capacidade maxima e obrigatoria.")]
    [Range(1, int.MaxValue, ErrorMessage = "A capacidade maxima deve ser superior a 0.")]
    public int? Capacidade { get; set; }

    [Required(ErrorMessage = "O preco e obrigatorio.")]
    [Range(0, 999999.99, ErrorMessage = "O preco deve ser igual ou superior a 0.")]
    public decimal? Preco { get; set; }

    [Required(ErrorMessage = "A quantidade de bilhetes Standard e obrigatoria.")]
    [Range(0, int.MaxValue, ErrorMessage = "A quantidade de bilhetes Standard nao pode ser negativa.")]
    public int? QuantidadeStandard { get; set; }

    [Required(ErrorMessage = "A quantidade de bilhetes Gold e obrigatoria.")]
    [Range(0, int.MaxValue, ErrorMessage = "A quantidade de bilhetes Gold nao pode ser negativa.")]
    public int? QuantidadeGold { get; set; }

    [Required(ErrorMessage = "A quantidade de bilhetes VIP e obrigatoria.")]
    [Range(0, int.MaxValue, ErrorMessage = "A quantidade de bilhetes VIP nao pode ser negativa.")]
    public int? QuantidadeVip { get; set; }

    public int? IdCategoria { get; set; }

    [StringLength(100, ErrorMessage = "O nome da nova categoria nao pode ter mais de 100 caracteres.")]
    public string? NovaCategoria { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!Capacidade.HasValue)
            yield break;

        var totalBilhetes = (QuantidadeStandard ?? 0) + (QuantidadeGold ?? 0) + (QuantidadeVip ?? 0);

        if (totalBilhetes <= 0)
        {
            yield return new ValidationResult(
                "Deves disponibilizar pelo menos um bilhete entre Standard, Gold e VIP.",
                [nameof(QuantidadeStandard), nameof(QuantidadeGold), nameof(QuantidadeVip)]);
        }

        if (totalBilhetes > Capacidade.Value)
        {
            yield return new ValidationResult(
                "A soma dos bilhetes Standard, Gold e VIP nao pode ultrapassar a capacidade maxima do evento.",
                [nameof(QuantidadeStandard), nameof(QuantidadeGold), nameof(QuantidadeVip), nameof(Capacidade)]);
        }

        if (!IdCategoria.HasValue && string.IsNullOrWhiteSpace(NovaCategoria))
        {
            yield return new ValidationResult(
                "Escolhe uma categoria existente ou cria uma nova.",
                [nameof(IdCategoria), nameof(NovaCategoria)]);
        }
    }
}
