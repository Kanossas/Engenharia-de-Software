using ES2.Data;
using ES2.Models;
using ES2.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ES2.Repositories;

public class AtividadeRepository : IAtividadeRepository, IAtividadeReadRepository, IAtividadeWriteRepository
{
    private readonly AppDbContext _context;

    public AtividadeRepository(AppDbContext context) => _context = context;

    public async Task<IEnumerable<Atividade>> GetAllAsync() =>
        await _context.Atividades
            .Include(a => a.IdEventoNavigation)
            .Include(a => a.IdCategoriaNavigation)
            .ToListAsync();

    public async Task<Atividade?> GetByIdAsync(int id) =>
        await _context.Atividades.FindAsync(id);

    public async Task AddAsync(Atividade atividade)
    {
        await _context.Atividades.AddAsync(atividade);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Atividade atividade)
    {
        _context.Atividades.Update(atividade);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var atividade = await GetByIdAsync(id);
        if (atividade != null)
        {
            _context.Atividades.Remove(atividade);
            await _context.SaveChangesAsync();
        }
    }
    
    

    public async Task<IEnumerable<Atividade>> GetByEventoAsync(int eventoId) =>
        await _context.Atividades
            .Where(a => a.IdEvento == eventoId)
            .ToListAsync();

    public async Task<IEnumerable<Utilizador>> GetParticipantesByAtividadeAsync(int atividadeId) =>
        await _context.RegistoAtividades
            .Where(r => r.IdAtividade == atividadeId && !r.IsCancelado)
            .Select(r => r.IdUtiNavigation)
            .ToListAsync();
    
    
}