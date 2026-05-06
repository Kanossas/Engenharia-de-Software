using ES2.Data;
using ES2.Models;
using ES2.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ES2.Services;

public class AutenticacaoService : IAutenticacaoService
{
    private readonly AppDbContext _context;

    public AutenticacaoService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Utilizador?> AutenticarAsync(string email, string password)
    {
        var user = await _context.Utilizadores.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
            return null;

        var hasher = new PasswordHasher<Utilizador>();
        var resultado = hasher.VerifyHashedPassword(user, user.Password, password);
        return resultado != PasswordVerificationResult.Failed ? user : null;
    }
}
