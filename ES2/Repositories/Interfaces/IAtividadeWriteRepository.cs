using ES2.Models;

namespace ES2.Repositories.Interfaces;

public interface IAtividadeWriteRepository
{
    Task AddAsync(Atividade entity);
    Task UpdateAsync(Atividade entity);
    Task DeleteAsync(int id);
}