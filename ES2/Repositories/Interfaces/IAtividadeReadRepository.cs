using ES2.Models;

namespace ES2.Repositories.Interfaces;

public interface IAtividadeReadRepository
{
    Task<IEnumerable<Atividade>> GetAllAsync();
    Task<Atividade?> GetByIdAsync(int id);
    Task<IEnumerable<Atividade>> GetByEventoAsync(int eventoId);
    Task<IEnumerable<Utilizador>> GetParticipantesByAtividadeAsync(int atividadeId);
}