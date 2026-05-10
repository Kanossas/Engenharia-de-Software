using ES2.Models;

namespace ES2.Services.Interfaces;

public interface IAuthService
{
    Task<Utilizador?> AutenticarAsync(string email, string password);
}
