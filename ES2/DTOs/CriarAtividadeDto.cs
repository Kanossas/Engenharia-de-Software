using System.ComponentModel.DataAnnotations;

namespace ES2.DTOs;

public class CriarAtividadeDto
{
    public int IdEvento { get; set; }

    [Required(ErrorMessage = "O nome é obrigatório")]
    [StringLength(100, ErrorMessage = "O nome não pode ter mais de 100 caracteres")]
    public string Nome { get; set; } = null!;

    [Required(ErrorMessage = "O local é obrigatório")]
    [StringLength(100, ErrorMessage = "O local não pode ter mais de 100 caracteres")]
    public string Local { get; set; } = null!;

    [Required(ErrorMessage = "A capacidade é obrigatória")]
    [Range(1, 100000, ErrorMessage = "A capacidade tem de ser maior que 0")]
    public int Capacidade { get; set; }

    [Required(ErrorMessage = "A categoria é obrigatória")]
    public int IdCategoria { get; set; }
}