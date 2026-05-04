using ES2.Data;
using ES2.Models;
using ES2.Repositories;
using ES2.Repositories.Interfaces;
using ES2.Services.Inscricoes;
using ES2.Services.Inscricoes.Regras;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configura o sistema de autenticação por Cookies
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", config =>
    {
        config.Cookie.Name = "UserLoginCookie";
        config.LoginPath = "/Login/Index";
        config.AccessDeniedPath = "/Home/Index"; 
    });

// Add services to the container.
builder.Services.AddControllersWithViews();

// Registar o DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositórios gerais
builder.Services.AddScoped<IUtilizadorRepository, UtilizadorRepository>();
builder.Services.AddScoped<IEventoRepository, EventoRepository>();
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>(); 
builder.Services.AddScoped<IBilhetesEventoRepository, BilhetesEventoRepository>();
builder.Services.AddScoped<ITipoBilheteRepository, TipoBilheteRepository>();
builder.Services.AddScoped<IInscricaoEventoService, InscricaoEventoService>();
builder.Services.AddScoped<IRegraInscricaoEvento, RegraBilheteDuplicado>();
builder.Services.AddScoped<IRegraInscricaoEvento, RegraInscricaoDuplicadaEvento>();
builder.Services.AddScoped<IRegraInscricaoEvento, RegraCapacidadeEvento>();

// ISP: Registo das interfaces de leitura e escrita de atividades separadamente
builder.Services.AddScoped<IAtividadeReadRepository, AtividadeRepository>();
builder.Services.AddScoped<IAtividadeWriteRepository, AtividadeRepository>();
builder.Services.AddScoped<IAtividadeRepository, AtividadeRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();
app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    bool existeAdmin = context.Utilizadores.Any(u => u.TipoUti == 1);

    if (!existeAdmin)
    {
        var hasher = new PasswordHasher<Utilizador>();
        var admin = new Utilizador
        {
            Nome = "Admin",
            Email = "admin@es2.com",
            TipoUti = 1
        };

        admin.Password = hasher.HashPassword(admin, "admin");

        context.Utilizadores.Add(admin);
        context.SaveChanges();
    }
}

app.Run();