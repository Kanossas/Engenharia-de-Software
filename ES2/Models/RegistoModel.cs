using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;


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
    
    [Required(ErrorMessage = "O telemóvel é obrigatório.")]
    [RegularExpression(@"^\d{9}$", ErrorMessage = "O telemóvel deve ter exatamente 9 dígitos.")]
    [DataType(DataType.PhoneNumber)]
    public string Telemovel { get; set; }
    
    [Required(ErrorMessage = "Escreva o seu código postal.")]
    public string CodigoPostalInput { get; set; }
}