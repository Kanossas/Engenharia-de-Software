using ES2.Models;

namespace ES2.Repositories.Interfaces;

public interface IUtilizadorRepository : IGenericRepository<Utilizador>
{
    Task<Utilizador?> GetByEmailAsync(string email);
    Task<IEnumerable<Utilizador>> GetParticipantesByEventoAsync(int eventoId);
    
    
    Task<bool> EmailJaExisteAsync(string email, int excludeId);
}