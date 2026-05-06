using ES2.Models;

namespace ES2.Services.Interfaces;

public interface IRegistoService
{
    Task RegistarAsync(RegistoModel model);
}
