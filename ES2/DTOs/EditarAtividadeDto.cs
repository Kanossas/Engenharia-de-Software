using System.ComponentModel.DataAnnotations;

namespace ES2.DTOs;

public class EditarAtividadeDto
{
    // Nome da atividade - obrigatório
    [Required(ErrorMessage = "O nome é obrigatório")]
    [StringLength(100, ErrorMessage = "O nome não pode ter mais de 100 caracteres")]
    public string Nome { get; set; } = null!;

    // Local onde decorre a atividade - obrigatório
    [Required(ErrorMessage = "O local é obrigatório")]
    [StringLength(200, ErrorMessage = "O local não pode ter mais de 200 caracteres")]
    public string Local { get; set; } = null!;

    // Número máximo de pessoas na atividade
    [Required(ErrorMessage = "A capacidade é obrigatória")]
    [Range(1, 100000, ErrorMessage = "A capacidade tem de ser maior que 0")]
    public int Capacidade { get; set; }

    // Categoria da atividade
    [Required(ErrorMessage = "A categoria é obrigatória")]
    public int IdCategoria { get; set; }
}