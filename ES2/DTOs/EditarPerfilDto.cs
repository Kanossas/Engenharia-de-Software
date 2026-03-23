using System.ComponentModel.DataAnnotations;

namespace ES2.DTOs;

public class EditarPerfilDto
{
    // [Required] significa que este campo é obrigatório
    // nome não pode ter mais de 100 caracteres
    [Required(ErrorMessage = "O nome é obrigatório")]
    [StringLength(100, ErrorMessage = "O nome não pode ter mais de 100 caracteres")]
    public string Nome { get; set; } = null!;

    // [EmailAddress] valida automaticamente se o formato do email está correto
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string? Email { get; set; }

    // O telemóvel é opcional (string? com o ?)
    [StringLength(20, ErrorMessage = "Telemóvel não pode ter mais de 20 caracteres")]
    public string? Telemovel { get; set; }
}