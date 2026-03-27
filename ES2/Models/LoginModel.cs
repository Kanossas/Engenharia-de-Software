using System.ComponentModel.DataAnnotations;

namespace ES2.Models;

public class LoginModel
{
    [Required(ErrorMessage = "O email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string Email { get; set; }

    [Required(ErrorMessage = "A password é obrigatória")]
    [DataType(DataType.Password)]
    public string Password { get; set; }
}