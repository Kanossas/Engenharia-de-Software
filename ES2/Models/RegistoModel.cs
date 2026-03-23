using System.ComponentModel.DataAnnotations;


namespace ES2.Models;

public class RegistoModel
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    public string Nome { get; set; }

    [Required(ErrorMessage = "O email é obrigatório.")]
    [EmailAddress(ErrorMessage = "Email inválido.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "A password é obrigatória.")]
    [DataType(DataType.Password)]
    public string Password { get; set; }
    
    [Required(ErrorMessage = "Escreva o seu código postal.")]
    public string CodigoPostalInput { get; set; }
}