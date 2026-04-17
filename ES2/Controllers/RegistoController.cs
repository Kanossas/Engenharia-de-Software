using ES2.Models;
using ES2.Data;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.AspNetCore.Identity; // Added for PasswordHasher

namespace ES2.Controllers;

public class RegistoController : Controller
{
    private readonly AppDbContext _context;
    
    public RegistoController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Registo()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Registo(RegistoModel model)
    {
        if (ModelState.IsValid)
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
                var novoCp = new CodigoPostal 
                {
                    CodPostal = model.CodigoPostalInput
                };

                _context.Add(novoCp);
                await _context.SaveChangesAsync(); 
                idCodPostalFinal = novoCp.IdCodPostal;
            }

            var hasher = new PasswordHasher<Utilizador>(); // Initialize PasswordHasher
            var hashedPassword = hasher.HashPassword(null, model.Password); // Hash the password

            var NovoUtilizador = new Utilizador
            {
                Nome = model.Nome,
                Email = model.Email,
                Password = hashedPassword, // Store the hashed password
                Telemovel = model.Telemovel,
                TipoUti=2,
                IdCodPostal = idCodPostalFinal
            };
            
            _context.Add(NovoUtilizador);
            await _context.SaveChangesAsync();
            
            return RedirectToAction("Index", "Home");
        }
        return View(model);
    }
}