using ES2.Data;
using ES2.Models;
using ES2.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace ES2.Services;

public class RegistoService : IRegistoService
{
    private readonly AppDbContext _context;

    public RegistoService(AppDbContext context)
    {
        _context = context;
    }

    public async Task RegistarAsync(RegistoModel model)
    {
        var cpExistente = _context.CodigoPostals
            .FirstOrDefault(c => c.CodPostal == model.CodigoPostalInput);

        int idCodPostalFinal;

        if (cpExistente != null)
        {
            idCodPostalFinal = cpExistente.IdCodPostal;
        }
        else
        {
            var novoCp = new CodigoPostal { CodPostal = model.CodigoPostalInput };
            _context.Add(novoCp);
            await _context.SaveChangesAsync();
            idCodPostalFinal = novoCp.IdCodPostal;
        }

        var hasher = new PasswordHasher<Utilizador>();
        var hashedPassword = hasher.HashPassword(null, model.Password);

        var novoUtilizador = new Utilizador
        {
            Nome = model.Nome,
            Email = model.Email,
            Password = hashedPassword,
            Telemovel = model.Telemovel,
            TipoUti = 2,
            IdCodPostal = idCodPostalFinal
        };

        _context.Add(novoUtilizador);
        await _context.SaveChangesAsync();
    }
}
