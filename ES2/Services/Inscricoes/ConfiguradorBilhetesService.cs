using ES2.Data;
using ES2.DTOs;
using ES2.Models;
using Microsoft.EntityFrameworkCore;

namespace ES2.Services.Inscricoes;

public class ConfiguradorBilhetesService : IConfiguradorBilhetesService
{
    private const string TipoStandard = "Standard";
    private const string TipoGold = "Gold";
    private const string TipoVip = "VIP";

    private readonly AppDbContext _context;

    public ConfiguradorBilhetesService(AppDbContext context)
    {
        _context = context;
    }

    public async Task GarantirTiposBaseAsync()
    {
        await GarantirTipoBilheteAsync(TipoStandard);
        await GarantirTipoBilheteAsync(TipoGold);
        await GarantirTipoBilheteAsync(TipoVip);
        await GarantirTipoPagamentoAsync("Cartao Bancario");
        await GarantirTipoPagamentoAsync("MB Way");
        await GarantirTipoPagamentoAsync("PayPal");
    }

    public async Task<IReadOnlyCollection<OfertaBilheteEventoDto>> GarantirEObterOfertasAsync(int eventoId)
    {
        var evento = await _context.Eventos
            .Include(e => e.BilhetesEventos)
            .ThenInclude(be => be.IdBilheteNavigation)
            .ThenInclude(b => b.IdTipoNavigation)
            .FirstOrDefaultAsync(e => e.IdEvento == eventoId);

        if (evento == null)
            return Array.Empty<OfertaBilheteEventoDto>();

        await GarantirTiposBaseAsync();
        await GarantirBilhetesPadraoAsync(evento);

        return evento.BilhetesEventos
            .OrderBy(be => OrdemTipo(be.IdBilheteNavigation.IdTipoNavigation?.Nome))
            .Select(MapearOferta)
            .ToList();
    }

    public async Task ConfigurarBilhetesEventoAsync(int eventoId, decimal precoBase, int quantidadeStandard, int quantidadeGold, int quantidadeVip)
    {
        var evento = await _context.Eventos
            .Include(e => e.BilhetesEventos)
            .ThenInclude(be => be.IdBilheteNavigation)
            .ThenInclude(b => b.IdTipoNavigation)
            .FirstOrDefaultAsync(e => e.IdEvento == eventoId);

        if (evento == null)
            return;

        await GarantirTiposBaseAsync();
        await GarantirBilhetesPadraoAsync(
            evento,
            Convert.ToDouble(precoBase),
            quantidadeStandard,
            quantidadeGold,
            quantidadeVip,
            forcarConfiguracao: true);
    }

    private async Task GarantirTipoBilheteAsync(string nomeTipo)
    {
        var existe = await _context.TipoBilhetes.AnyAsync(t => t.Nome == nomeTipo);
        if (!existe)
        {
            _context.TipoBilhetes.Add(new TipoBilhete { Nome = nomeTipo });
            await _context.SaveChangesAsync();
        }
    }

    private async Task GarantirTipoPagamentoAsync(string nome)
    {
        var existe = await _context.TipoPagamentos.AnyAsync(tp => tp.Nome == nome);
        if (!existe)
        {
            _context.TipoPagamentos.Add(new TipoPagamento { Nome = nome });
            await _context.SaveChangesAsync();
        }
    }

    private async Task GarantirBilhetesPadraoAsync(
        Evento evento,
        double? precoBasePersonalizado = null,
        int? quantidadeStandardPersonalizada = null,
        int? quantidadeGoldPersonalizada = null,
        int? quantidadeVipPersonalizada = null,
        bool forcarConfiguracao = false)
    {
        var tipoStandard = await _context.TipoBilhetes.FirstAsync(t => t.Nome == TipoStandard);
        var tipoGold = await _context.TipoBilhetes.FirstAsync(t => t.Nome == TipoGold);
        var tipoVip = await _context.TipoBilhetes.FirstAsync(t => t.Nome == TipoVip);

        var distribuicao = quantidadeStandardPersonalizada.HasValue &&
                           quantidadeGoldPersonalizada.HasValue &&
                           quantidadeVipPersonalizada.HasValue
            ? (
                Standard: quantidadeStandardPersonalizada.Value,
                Gold: quantidadeGoldPersonalizada.Value,
                Vip: quantidadeVipPersonalizada.Value
            )
            : DistribuirCapacidade(evento.CapMax ?? 0);

        var precoBase = precoBasePersonalizado ?? (evento.BilhetesEventos.Any()
            ? evento.BilhetesEventos.OrderBy(be => be.IdBiEv).First().Preco
            : 0d);

        var precoGold = Math.Round(precoBase * 1.5, 2);
        var precoVip = Math.Round(precoBase * 2.2, 2);

        var standard = evento.BilhetesEventos.FirstOrDefault(be =>
            be.IdBilheteNavigation.IdTipoNavigation?.Nome == TipoStandard);

        if (standard == null)
            standard = evento.BilhetesEventos.OrderBy(be => be.IdBiEv).FirstOrDefault();

        if (standard == null)
        {
            standard = new BilhetesEvento
            {
                IdEvento = evento.IdEvento,
                Preco = precoBase,
                QuantidadeDisponivel = distribuicao.Standard
            };

            standard.IdBilheteNavigation = new Bilhete
            {
                Nome = "Bilhete Standard",
                IdTipo = tipoStandard.IdTipo
            };

            _context.BilhetesEventos.Add(standard);
            evento.BilhetesEventos.Add(standard);
        }
        else
        {
            standard.IdBilheteNavigation.Nome = "Bilhete Standard";
            standard.IdBilheteNavigation.IdTipo = tipoStandard.IdTipo;
            if (forcarConfiguracao || (standard.Preco <= 0 && precoBase > 0))
                standard.Preco = precoBase;
            if (forcarConfiguracao || DeveInicializarQuantidade(standard))
                standard.QuantidadeDisponivel = distribuicao.Standard;
        }

        await GarantirBilhetePorTipoAsync(evento, tipoGold, "Bilhete Gold", precoGold, distribuicao.Gold, forcarConfiguracao);
        await GarantirBilhetePorTipoAsync(evento, tipoVip, "Bilhete VIP", precoVip, distribuicao.Vip, forcarConfiguracao);

        await _context.SaveChangesAsync();
    }

    private async Task GarantirBilhetePorTipoAsync(
        Evento evento,
        TipoBilhete tipoBilhete,
        string nomeBilhete,
        double preco,
        int quantidade,
        bool forcarConfiguracao = false)
    {
        var bilheteEvento = evento.BilhetesEventos.FirstOrDefault(be =>
            be.IdBilheteNavigation.IdTipoNavigation?.Nome == tipoBilhete.Nome);

        if (bilheteEvento == null)
        {
            bilheteEvento = new BilhetesEvento
            {
                IdEvento = evento.IdEvento,
                Preco = preco,
                QuantidadeDisponivel = quantidade
            };

            bilheteEvento.IdBilheteNavigation = new Bilhete
            {
                Nome = nomeBilhete,
                IdTipo = tipoBilhete.IdTipo
            };

            _context.BilhetesEventos.Add(bilheteEvento);
            evento.BilhetesEventos.Add(bilheteEvento);
            return;
        }

        bilheteEvento.IdBilheteNavigation.Nome = nomeBilhete;
        bilheteEvento.IdBilheteNavigation.IdTipo = tipoBilhete.IdTipo;

        if (forcarConfiguracao || (bilheteEvento.Preco <= 0 && preco > 0))
            bilheteEvento.Preco = preco;

        if (forcarConfiguracao || DeveInicializarQuantidade(bilheteEvento))
            bilheteEvento.QuantidadeDisponivel = quantidade;
    }

    private bool DeveInicializarQuantidade(BilhetesEvento bilheteEvento)
    {
        if (bilheteEvento.QuantidadeDisponivel > 0)
            return false;

        return !_context.BilheteUtils.Any(bu => bu.IdBiEv == bilheteEvento.IdBiEv);
    }

    private static (int Standard, int Gold, int Vip) DistribuirCapacidade(int capacidade)
    {
        if (capacidade <= 0)
            return (0, 0, 0);

        if (capacidade == 1)
            return (1, 0, 0);

        if (capacidade == 2)
            return (1, 1, 0);

        var vip = Math.Max(1, (int)Math.Round(capacidade * 0.15, MidpointRounding.AwayFromZero));
        var gold = Math.Max(1, (int)Math.Round(capacidade * 0.25, MidpointRounding.AwayFromZero));

        if (vip + gold >= capacidade)
        {
            vip = 1;
            gold = Math.Min(1, capacidade - vip);
        }

        var standard = Math.Max(0, capacidade - vip - gold);
        if (standard == 0)
        {
            standard = 1;
            if (gold > vip && gold > 0)
                gold -= 1;
            else if (vip > 0)
                vip -= 1;
        }

        return (standard, Math.Max(0, gold), Math.Max(0, vip));
    }

    private static OfertaBilheteEventoDto MapearOferta(BilhetesEvento bilheteEvento)
    {
        var tipo = bilheteEvento.IdBilheteNavigation.IdTipoNavigation?.Nome ?? TipoStandard;

        return new OfertaBilheteEventoDto
        {
            IdBilheteEvento = bilheteEvento.IdBiEv,
            NomeBilhete = bilheteEvento.IdBilheteNavigation.Nome,
            TipoBilhete = tipo,
            DescricaoAcesso = ObterDescricaoAcesso(tipo),
            ClasseIcone = ObterClasseIcone(tipo),
            Preco = Convert.ToDecimal(bilheteEvento.Preco),
            QuantidadeDisponivel = bilheteEvento.QuantidadeDisponivel
        };
    }

    private static string ObterDescricaoAcesso(string? tipoBilhete) =>
        tipoBilhete switch
        {
            TipoGold => "Entrada no evento e acesso automatico a todas as atividades do programa.",
            TipoVip => "Entrada no evento, acesso a todas as atividades e zonas com acesso restrito.",
            _ => "Entrada no evento com acesso standard ao recinto principal."
        };

    private static string ObterClasseIcone(string? tipoBilhete) =>
        tipoBilhete switch
        {
            TipoGold => "bi bi-stars",
            TipoVip => "bi bi-gem",
            _ => "bi bi-bar-chart-steps"
        };

    private static int OrdemTipo(string? tipoBilhete) =>
        tipoBilhete switch
        {
            TipoStandard => 0,
            TipoGold => 1,
            TipoVip => 2,
            _ => 99
        };
}
