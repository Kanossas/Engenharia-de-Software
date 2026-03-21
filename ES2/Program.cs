using ES2.Data;
using ES2.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Registar o DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
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