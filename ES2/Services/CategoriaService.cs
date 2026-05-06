using ES2.Data;
using ES2.Models;
using ES2.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ES2.Services;

public class CategoriaService : ICategoriaService
{
    private readonly AppDbContext _context;

    public CategoriaService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> ObterOuCriarAsync(string nomeCategoria)
    {
        var nomeNormalizado = nomeCategoria.Trim();
        var categoriaExistente = await _context.Categorias
            .FirstOrDefaultAsync(c => c.Nome.ToLower() == nomeNormalizado.ToLower());

        if (categoriaExistente != null)
            return categoriaExistente.IdCategoria;

        var novaCategoria = new Categoria { Nome = nomeNormalizado };
        _context.Categorias.Add(novaCategoria);
        await _context.SaveChangesAsync();
        return novaCategoria.IdCategoria;
    }
}
