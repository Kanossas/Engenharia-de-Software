using System.ComponentModel.DataAnnotations;

namespace ES2.DTOs;

public class CriarEventoDto
{
    [Required(ErrorMessage = "O nome do evento é obrigatório.")]
    [StringLength(100, ErrorMessage = "O nome do evento não pode ter mais de 100 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "A data do evento é obrigatória.")]
    [DataType(DataType.Date)]
    public DateOnly? Data { get; set; }

    [Required(ErrorMessage = "A hora do evento é obrigatória.")]
    [DataType(DataType.Time)]
    public TimeOnly? HoraInicio { get; set; }

    [Required(ErrorMessage = "O local do evento é obrigatório.")]
    [StringLength(50, ErrorMessage = "O local não pode ter mais de 50 caracteres.")]
    public string Local { get; set; } = string.Empty;

    [Required(ErrorMessage = "A descrição do evento é obrigatória.")]
    [StringLength(1000, ErrorMessage = "A descrição não pode ter mais de 1000 caracteres.")]
    public string Descricao { get; set; } = string.Empty;

    [Required(ErrorMessage = "A capacidade máxima é obrigatória.")]
    [Range(1, int.MaxValue, ErrorMessage = "A capacidade máxima deve ser superior a 0.")]
    public int? Capacidade { get; set; }

    [Required(ErrorMessage = "O preço é obrigatório.")]
    [Range(0, 999999.99, ErrorMessage = "O preço deve ser igual ou superior a 0.")]
    public decimal? Preco { get; set; }
}
