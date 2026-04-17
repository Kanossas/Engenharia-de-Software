using ES2.Data;
using ES2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace ES2.Controllers;

public class LoginController : Controller
{
    private readonly AppDbContext _context;

    public LoginController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Index(LoginModel model)
    {
        if (ModelState.IsValid)
        {
            // Procura o utilizador pelo email
            var user = await _context.Utilizadores
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            var hasher = new PasswordHasher<Utilizador>();
            if (user != null && hasher.VerifyHashedPassword(user, user.Password, model.Password) != PasswordVerificationResult.Failed)
            {
                // 1. Criamos os dados que o site vai "lembrar"
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Nome),
                    new Claim(ClaimTypes.Role, "Utilizador") 
                };

                var identity = new ClaimsIdentity(claims, "CookieAuth");
                var principal = new ClaimsPrincipal(identity);

                // 2. FORMA CORRETA: Usa apenas HttpContext.SignInAsync
                // Isto vai ativar o 'using' lá de cima e tirar o erro vermelho
                await HttpContext.SignInAsync("CookieAuth", principal);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Email ou Password incorretos.");
        }
        return View(model);
    }

    // Aproveita e adiciona já o Logout aqui em baixo!
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("CookieAuth");
        return RedirectToAction("Index", "Home");
    }
}