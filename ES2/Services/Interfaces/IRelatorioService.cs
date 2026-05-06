using ES2.DTOs;

namespace ES2.Services.Interfaces;

public interface IRelatorioService
{
    Task<RelatorioAdminDto> ObterRelatorioAdminAsync();
}
