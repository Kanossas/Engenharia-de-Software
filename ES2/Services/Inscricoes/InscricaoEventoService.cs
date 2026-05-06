using ES2.Data;
using ES2.DTOs;
using ES2.Models;
using Microsoft.EntityFrameworkCore;

namespace ES2.Services.Inscricoes;

public class InscricaoEventoService : IInscricaoEventoService
{
    private const string TipoStandard = "Standard";
    private const string TipoGold = "Gold";
    private const string TipoVip = "VIP";

    private readonly AppDbContext _context;
    private readonly IEnumerable<IRegraInscricaoEvento> _regras;
    private readonly IConfiguradorBilhetesService _configurador;

    public InscricaoEventoService(
        AppDbContext context,
        IEnumerable<IRegraInscricaoEvento> regras,
        IConfiguradorBilhetesService configurador)
    {
        _context = context;
        _regras = regras;
        _configurador = configurador;
    }

    public async Task<ResultadoOperacaoInscricao> InscreverAsync(int bilheteEventoId, string nomeUtilizador)
    {
        var contexto = await CriarContextoAsync(bilheteEventoId, nomeUtilizador);
        if (contexto == null)
            return ResultadoOperacaoInscricao.Falha("Nao foi possivel concluir a inscricao.");

        return await ProcessarInscricaoAsync(contexto, checkout: null);
    }

    public async Task<CheckoutBilheteDto?> ObterCheckoutAsync(int bilheteEventoId, string nomeUtilizador)
    {
        var utilizador = await _context.Utilizadores.FirstOrDefaultAsync(u => u.Nome == nomeUtilizador);
        if (utilizador == null)
            return null;

        var bilheteEvento = await _context.BilhetesEventos
            .Include(be => be.IdEventoNavigation)
            .Include(be => be.IdBilheteNavigation)
            .ThenInclude(b => b.IdTipoNavigation)
            .FirstOrDefaultAsync(be => be.IdBiEv == bilheteEventoId);

        if (bilheteEvento == null)
            return null;

        await _configurador.GarantirTiposBaseAsync();

        var tiposPagamento = await _context.TipoPagamentos
            .OrderBy(tp => tp.Nome)
            .Select(tp => new OpcaoPagamentoDto
            {
                IdTipoPagamento = tp.IdTipo,
                Nome = tp.Nome
            })
            .ToListAsync();

        return new CheckoutBilheteDto
        {
            IdBilheteEvento = bilheteEvento.IdBiEv,
            IdEvento = bilheteEvento.IdEvento,
            NomeEvento = bilheteEvento.IdEventoNavigation.Nome,
            DataEvento = bilheteEvento.IdEventoNavigation.Data,
            HoraEvento = bilheteEvento.IdEventoNavigation.HoraInicio,
            LocalEvento = bilheteEvento.IdEventoNavigation.Local,
            NomeBilhete = bilheteEvento.IdBilheteNavigation.Nome,
            TipoBilhete = bilheteEvento.IdBilheteNavigation.IdTipoNavigation?.Nome ?? TipoStandard,
            DescricaoAcesso = ObterDescricaoAcesso(bilheteEvento.IdBilheteNavigation.IdTipoNavigation?.Nome),
            Preco = Convert.ToDecimal(bilheteEvento.Preco),
            QuantidadeDisponivel = bilheteEvento.QuantidadeDisponivel,
            NomeComprador = utilizador.Nome,
            Email = utilizador.Email ?? string.Empty,
            Telemovel = utilizador.Telemovel ?? string.Empty,
            Morada = utilizador.Morada ?? string.Empty,
            TiposPagamento = tiposPagamento
        };
    }

    public async Task<ResultadoOperacaoInscricao> ComprarAsync(CheckoutBilheteDto dto, string nomeUtilizador)
    {
        var contexto = await CriarContextoAsync(dto.IdBilheteEvento, nomeUtilizador);
        if (contexto == null)
            return ResultadoOperacaoInscricao.Falha("Nao foi possivel identificar o utilizador ou o bilhete selecionado.");

        contexto.Utilizador.Email = dto.Email;
        contexto.Utilizador.Telemovel = dto.Telemovel;
        contexto.Utilizador.Morada = dto.Morada;

        return await ProcessarInscricaoAsync(contexto, dto);
    }

    public async Task<ResultadoOperacaoInscricao> CancelarAsync(int bilheteEventoId, string nomeUtilizador)
    {
        var utilizador = await _context.Utilizadores.FirstOrDefaultAsync(u => u.Nome == nomeUtilizador);
        if (utilizador == null)
            return ResultadoOperacaoInscricao.Falha("Nao foi possivel identificar o utilizador autenticado.");

        var bilheteEvento = await _context.BilhetesEventos
            .Include(be => be.IdEventoNavigation)
            .ThenInclude(e => e.Atividades)
            .FirstOrDefaultAsync(be => be.IdBiEv == bilheteEventoId);

        if (bilheteEvento == null)
            return ResultadoOperacaoInscricao.Falha("O bilhete selecionado nao existe.");

        var registoEvento = await _context.RegistoEventos
            .FirstOrDefaultAsync(r => r.IdUti == utilizador.IdUti &&
                                      r.IdEvento == bilheteEvento.IdEvento &&
                                      !r.IsCancelado);

        if (registoEvento == null)
            return ResultadoOperacaoInscricao.Falha("Nao tens uma inscricao ativa neste evento.");

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            registoEvento.IsCancelado = true;
            bilheteEvento.QuantidadeDisponivel += 1;

            var idsAtividades = bilheteEvento.IdEventoNavigation.Atividades.Select(a => a.IdAtividade).ToList();
            if (idsAtividades.Count > 0)
            {
                var registosAtividade = await _context.RegistoAtividades
                    .Where(r => r.IdUti == utilizador.IdUti &&
                                idsAtividades.Contains(r.IdAtividade) &&
                                !r.IsCancelado)
                    .ToListAsync();

                foreach (var registoAtividade in registosAtividade)
                    registoAtividade.IsCancelado = true;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return ResultadoOperacaoInscricao.Ok("Inscricao no evento cancelada com sucesso.");
        }
        catch
        {
            await transaction.RollbackAsync();
            return ResultadoOperacaoInscricao.Falha("Ocorreu um erro ao cancelar a inscricao.");
        }
    }

    public async Task<HashSet<int>> ObterEventosInscritosAsync(string? nomeUtilizador)
    {
        if (string.IsNullOrWhiteSpace(nomeUtilizador))
            return new HashSet<int>();

        var utilizador = await _context.Utilizadores
            .FirstOrDefaultAsync(u => u.Nome == nomeUtilizador);

        if (utilizador == null)
            return new HashSet<int>();

        var ids = await _context.RegistoEventos
            .Where(r => r.IdUti == utilizador.IdUti && !r.IsCancelado)
            .Select(r => r.IdEvento)
            .ToListAsync();

        return new HashSet<int>(ids);
    }

    public async Task<int?> ObterBilheteAtivoDoEventoAsync(int eventoId, string? nomeUtilizador)
    {
        if (string.IsNullOrWhiteSpace(nomeUtilizador))
            return null;

        var utilizador = await _context.Utilizadores.FirstOrDefaultAsync(u => u.Nome == nomeUtilizador);
        if (utilizador == null)
            return null;

        var temEventoAtivo = await _context.RegistoEventos.AnyAsync(r =>
            r.IdUti == utilizador.IdUti &&
            r.IdEvento == eventoId &&
            !r.IsCancelado);

        if (!temEventoAtivo)
            return null;

        return await _context.BilheteUtils
            .Where(bu => bu.IdUtilizador == utilizador.IdUti &&
                         bu.IdBiEvNavigation != null &&
                         bu.IdBiEvNavigation.IdEvento == eventoId)
            .OrderByDescending(bu => bu.IdBiUti)
            .Select(bu => bu.IdBiEv)
            .FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyCollection<HistoricoCompraDto>> ObterHistoricoAsync(string nomeUtilizador)
    {
        var utilizador = await _context.Utilizadores.FirstOrDefaultAsync(u => u.Nome == nomeUtilizador);
        if (utilizador == null)
            return Array.Empty<HistoricoCompraDto>();

        return await _context.Recibos
            .Where(r => r.IdUtilizador == utilizador.IdUti)
            .Include(r => r.IdTipoPagNavigation)
            .Include(r => r.IdBiUtiNavigation)
            .ThenInclude(bu => bu.IdBiEvNavigation)
            .ThenInclude(be => be.IdEventoNavigation)
            .Include(r => r.IdBiUtiNavigation)
            .ThenInclude(bu => bu.IdBiEvNavigation)
            .ThenInclude(be => be.IdBilheteNavigation)
            .ThenInclude(b => b.IdTipoNavigation)
            .OrderByDescending(r => r.Data)
            .ThenByDescending(r => r.IdRecibo)
            .Select(r => new HistoricoCompraDto
            {
                IdRecibo = r.IdRecibo,
                NomeEvento = r.IdBiUtiNavigation.IdBiEvNavigation!.IdEventoNavigation.Nome,
                NomeBilhete = r.IdBiUtiNavigation.IdBiEvNavigation.IdBilheteNavigation.Nome,
                TipoBilhete = r.IdBiUtiNavigation.IdBiEvNavigation.IdBilheteNavigation.IdTipoNavigation!.Nome ?? TipoStandard,
                DescricaoAcesso = ObterDescricaoAcesso(r.IdBiUtiNavigation.IdBiEvNavigation.IdBilheteNavigation.IdTipoNavigation!.Nome),
                ValorPago = Convert.ToDecimal(r.ValorPago),
                DataCompra = r.Data,
                MetodoPagamento = r.IdTipoPagNavigation!.Nome
            })
            .ToListAsync();
    }

    private async Task<InscricaoEventoContexto?> CriarContextoAsync(int bilheteEventoId, string nomeUtilizador)
    {
        var utilizador = await _context.Utilizadores
            .FirstOrDefaultAsync(u => u.Nome == nomeUtilizador);

        if (utilizador == null)
            return null;

        var bilheteEvento = await _context.BilhetesEventos
            .Include(be => be.IdEventoNavigation)
            .ThenInclude(e => e.Atividades)
            .Include(be => be.IdBilheteNavigation)
            .ThenInclude(b => b.IdTipoNavigation)
            .FirstOrDefaultAsync(be => be.IdBiEv == bilheteEventoId);

        if (bilheteEvento == null)
            return null;

        return new InscricaoEventoContexto
        {
            Utilizador = utilizador,
            BilheteEvento = bilheteEvento
        };
    }

    private async Task<ResultadoOperacaoInscricao> ProcessarInscricaoAsync(InscricaoEventoContexto contexto, CheckoutBilheteDto? checkout)
    {
        foreach (var regra in _regras)
        {
            var erro = await regra.ValidarAsync(contexto);
            if (!string.IsNullOrWhiteSpace(erro))
                return ResultadoOperacaoInscricao.Falha(erro);
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            contexto.BilheteEvento.QuantidadeDisponivel -= 1;

            var registoEvento = await _context.RegistoEventos
                .FirstOrDefaultAsync(r => r.IdUti == contexto.Utilizador.IdUti &&
                                          r.IdEvento == contexto.BilheteEvento.IdEvento);

            if (registoEvento == null)
            {
                _context.RegistoEventos.Add(new RegistoEvento
                {
                    IdUti = contexto.Utilizador.IdUti,
                    IdEvento = contexto.BilheteEvento.IdEvento,
                    IsCancelado = false
                });
            }
            else
            {
                registoEvento.IsCancelado = false;
            }

            var bilheteUtil = new BilheteUtil
            {
                IdBiEv = contexto.BilheteEvento.IdBiEv,
                IdUtilizador = contexto.Utilizador.IdUti
            };

            _context.BilheteUtils.Add(bilheteUtil);

            if (DaAcessoAtividades(contexto.BilheteEvento.IdBilheteNavigation.IdTipoNavigation?.Nome))
                await GarantirRegistoAtividadesAsync(contexto);

            await _context.SaveChangesAsync();

            if (checkout?.IdTipoPagamento is int idTipoPagamento)
            {
                _context.Recibos.Add(new Recibo
                {
                    IdUtilizador = contexto.Utilizador.IdUti,
                    IdBiUti = bilheteUtil.IdBiUti,
                    ValorPago = contexto.BilheteEvento.Preco,
                    Data = DateOnly.FromDateTime(DateTime.Today),
                    IdTipoPag = idTipoPagamento
                });

                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();

            var mensagem = checkout == null
                ? $"Inscricao efetuada com sucesso para o evento {contexto.BilheteEvento.IdEventoNavigation.Nome}."
                : $"Compra concluida com sucesso. O teu bilhete para {contexto.BilheteEvento.IdEventoNavigation.Nome} ja esta no historico de compras.";

            return ResultadoOperacaoInscricao.Ok(mensagem);
        }
        catch
        {
            await transaction.RollbackAsync();
            return ResultadoOperacaoInscricao.Falha("Ocorreu um erro ao tentar concluir a operacao.");
        }
    }

    private async Task GarantirRegistoAtividadesAsync(InscricaoEventoContexto contexto)
    {
        foreach (var atividade in contexto.BilheteEvento.IdEventoNavigation.Atividades)
        {
            var registoAtividade = await _context.RegistoAtividades
                .FirstOrDefaultAsync(r => r.IdUti == contexto.Utilizador.IdUti && r.IdAtividade == atividade.IdAtividade);

            if (registoAtividade == null)
            {
                _context.RegistoAtividades.Add(new RegistoAtividade
                {
                    IdUti = contexto.Utilizador.IdUti,
                    IdAtividade = atividade.IdAtividade,
                    IsCancelado = false
                });
            }
            else
            {
                registoAtividade.IsCancelado = false;
            }
        }
    }

    private static bool DaAcessoAtividades(string? tipoBilhete) =>
        string.Equals(tipoBilhete, TipoGold, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(tipoBilhete, TipoVip, StringComparison.OrdinalIgnoreCase);

    private static string ObterDescricaoAcesso(string? tipoBilhete) =>
        tipoBilhete switch
        {
            TipoGold => "Entrada no evento e acesso automatico a todas as atividades do programa.",
            TipoVip => "Entrada no evento, acesso a todas as atividades e zonas com acesso restrito.",
            _ => "Entrada no evento com acesso standard ao recinto principal."
        };
}
