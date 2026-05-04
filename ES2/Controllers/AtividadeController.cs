using ES2.DTOs;
using ES2.Data;
using ES2.Models;
using ES2.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ES2.Controllers;

public class AtividadeController : Controller
{
    // ISP: Separação de responsabilidades de leitura e escrita
    // Utilizadores comuns só precisam de leitura (IAtividadeReadRepository)
    // Admins precisam de leitura e escrita (IAtividadeWriteRepository)
    private readonly IAtividadeReadRepository _atividadeReadRepository;
    private readonly IAtividadeWriteRepository _atividadeWriteRepository;
    private readonly IEventoRepository _eventoRepository;
    private readonly ICategoriaRepository _categoriaRepository;
    private readonly AppDbContext _context;

    public AtividadeController(
        IAtividadeReadRepository atividadeReadRepository,
        IAtividadeWriteRepository atividadeWriteRepository,
        IEventoRepository eventoRepository,
        ICategoriaRepository categoriaRepository,
        AppDbContext context)
    {
        _atividadeReadRepository = atividadeReadRepository;
        _atividadeWriteRepository = atividadeWriteRepository;
        _eventoRepository = eventoRepository;
        _categoriaRepository = categoriaRepository;
        _context = context;
    }

    // GET: usa apenas IAtividadeReadRepository (leitura)
    public async Task<IActionResult> Editar(int id)
    {
        var atividade = await _atividadeReadRepository.GetByIdAsync(id);

        if (atividade == null)
            return NotFound();

        var dto = new EditarAtividadeDto
        {
            Nome = atividade.Nome,
            Local = atividade.Local,
            Capacidade = atividade.Capacidade,
            IdCategoria = atividade.IdCategoria
        };

        ViewBag.AtividadeId = id;
        ViewBag.EventoId = atividade.IdEvento;

        return View(dto);
    }

    // GET: usa apenas IAtividadeReadRepository (leitura)
    public async Task<IActionResult> Index(string? nome, string? local, int? capacidade, int? idCategoria,
        int? idEvento)
    {
        var atividades = await _atividadeReadRepository.GetAllAsync();

        if (!string.IsNullOrEmpty(nome))
            atividades = atividades.Where(a => a.Nome.Contains(nome, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(local))
            atividades = atividades.Where(a =>
                a.Local != null && a.Local.Contains(local, StringComparison.OrdinalIgnoreCase));

        if (capacidade.HasValue)
            atividades = atividades.Where(a => a.Capacidade >= capacidade.Value);

        if (idCategoria.HasValue)
            atividades = atividades.Where(a => a.IdCategoria == idCategoria.Value);

        if (idEvento.HasValue)
            atividades = atividades.Where(a => a.IdEvento == idEvento.Value);

        ViewBag.Categorias = await _categoriaRepository.GetAllAsync();
        ViewBag.Eventos = await _eventoRepository.GetAllAsync();

        ViewBag.FiltroNome = nome;
        ViewBag.FiltroLocal = local;
        ViewBag.FiltroCapacidade = capacidade;
        ViewBag.FiltroCategoria = idCategoria;
        ViewBag.FiltroEvento = idEvento;
        ViewBag.AtividadesInscritas = await ObterAtividadesInscritasAsync();

        return View(atividades);
    }

    // GET: usa apenas IAtividadeReadRepository (leitura)
    public async Task<IActionResult> Pesquisar(string? nome, string? local, int? capacidade, int? idCategoria,
        int? idEvento)
    {
        var atividades = await _atividadeReadRepository.GetAllAsync();

        if (!string.IsNullOrEmpty(nome))
            atividades = atividades.Where(a => a.Nome.Contains(nome, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(local))
            atividades = atividades.Where(a =>
                a.Local != null && a.Local.Contains(local, StringComparison.OrdinalIgnoreCase));

        if (capacidade.HasValue)
            atividades = atividades.Where(a => a.Capacidade >= capacidade.Value);

        if (idCategoria.HasValue)
            atividades = atividades.Where(a => a.IdCategoria == idCategoria.Value);

        if (idEvento.HasValue)
            atividades = atividades.Where(a => a.IdEvento == idEvento.Value);

        ViewBag.AtividadesInscritas = await ObterAtividadesInscritasAsync();
        return PartialView("_ResultadosAtividades", atividades);
    }

    // POST: usa IAtividadeWriteRepository (escrita) - só admin
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, EditarAtividadeDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.AtividadeId = id;
            return View(dto);
        }

        var atividade = await _atividadeReadRepository.GetByIdAsync(id);
        if (atividade == null)
            return NotFound();

        atividade.Nome = dto.Nome;
        atividade.Local = dto.Local;
        atividade.Capacidade = dto.Capacidade;
        atividade.IdCategoria = dto.IdCategoria;

        // ISP: operação de escrita usa IAtividadeWriteRepository
        await _atividadeWriteRepository.UpdateAsync(atividade);

        TempData["Sucesso"] = "Atividade atualizada com sucesso!";
        return RedirectToAction("Editar", new { id });
    }

    // GET: usa apenas IAtividadeReadRepository (leitura)
    public async Task<IActionResult> ConfirmarRemocao(int id)
    {
        var atividade = await _atividadeReadRepository.GetByIdAsync(id);

        if (atividade == null)
            return NotFound();

        return View(atividade);
    }

    [HttpGet]
    public async Task<IActionResult> Participantes(int id)
    {
        var atividade = await _context.Atividades
            .Include(a => a.IdEventoNavigation)
            .Include(a => a.RegistoAtividades.Where(r => !r.IsCancelado))
            .ThenInclude(r => r.IdUtiNavigation)
            .FirstOrDefaultAsync(a => a.IdAtividade == id);

        if (atividade == null)
            return NotFound();

        var dto = new ParticipantesAtividadeDto
        {
            IdAtividade = atividade.IdAtividade,
            NomeAtividade = atividade.Nome,
            IdEvento = atividade.IdEvento,
            NomeEvento = atividade.IdEventoNavigation.Nome,
            LocalAtividade = atividade.Local,
            Capacidade = atividade.Capacidade,
            Participantes = atividade.RegistoAtividades
                .Where(r => !r.IsCancelado)
                .OrderBy(r => r.IdUtiNavigation.Nome)
                .Select(r => new ParticipanteInscritoDto
                {
                    IdUtilizador = r.IdUtiNavigation.IdUti,
                    Nome = r.IdUtiNavigation.Nome,
                    Email = r.IdUtiNavigation.Email,
                    Telemovel = r.IdUtiNavigation.Telemovel
                })
                .ToList()
        };

        return View(dto);
    }

    // POST: usa IAtividadeWriteRepository (escrita) - só admin
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remover(int id)
    {
        var atividade = await _atividadeReadRepository.GetByIdAsync(id);
        if (atividade == null)
            return NotFound();

        var eventoId = atividade.IdEvento;

        // ISP: operação de escrita usa IAtividadeWriteRepository
        await _atividadeWriteRepository.DeleteAsync(id);

        TempData["Sucesso"] = "Atividade removida com sucesso!";
        return RedirectToAction("Index", "Evento", new { id = eventoId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Inscrever(int id)
    {
        var nomeUtilizador = User.FindFirstValue(ClaimTypes.Name);
        if (string.IsNullOrWhiteSpace(nomeUtilizador))
            return RedirectToAction("Index", "Login");

        var utilizador = await _context.Utilizadores
            .FirstOrDefaultAsync(u => u.Nome == nomeUtilizador);

        if (utilizador == null)
        {
            TempData["Erro"] = "Nao foi possivel identificar o utilizador autenticado.";
            return RedirectToAction(nameof(Index));
        }

        var atividade = await _context.Atividades
            .FirstOrDefaultAsync(a => a.IdAtividade == id);

        if (atividade == null)
            return NotFound();

        var jaInscrito = await _context.RegistoAtividades
            .AnyAsync(r => r.IdUti == utilizador.IdUti && r.IdAtividade == id && !r.IsCancelado);

        if (jaInscrito)
        {
            TempData["Erro"] = "Ja estas inscrito nesta atividade.";
            return RedirectToAction(nameof(Index));
        }

        var inscritosAtivos = await _context.RegistoAtividades
            .CountAsync(r => r.IdAtividade == id && !r.IsCancelado);

        if (inscritosAtivos >= atividade.Capacidade)
        {
            TempData["Erro"] = "Esta atividade ja atingiu a capacidade maxima.";
            return RedirectToAction(nameof(Index));
        }

        var registoExistente = await _context.RegistoAtividades
            .FirstOrDefaultAsync(r => r.IdUti == utilizador.IdUti && r.IdAtividade == id);

        if (registoExistente == null)
        {
            _context.RegistoAtividades.Add(new RegistoAtividade
            {
                IdUti = utilizador.IdUti,
                IdAtividade = id,
                IsCancelado = false
            });
        }
        else
        {
            registoExistente.IsCancelado = false;
        }

        await _context.SaveChangesAsync();

        TempData["Sucesso"] = $"Inscricao na atividade '{atividade.Nome}' efetuada com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> CancelarInscricao(int id)
    {
        var nomeUtilizador = User.FindFirstValue(ClaimTypes.Name);
        if (string.IsNullOrWhiteSpace(nomeUtilizador))
            return RedirectToAction("Index", "Login");

        var utilizador = await _context.Utilizadores
            .FirstOrDefaultAsync(u => u.Nome == nomeUtilizador);

        if (utilizador == null)
        {
            TempData["Erro"] = "Nao foi possivel identificar o utilizador autenticado.";
            return RedirectToAction(nameof(Index));
        }

        var registo = await _context.RegistoAtividades
            .FirstOrDefaultAsync(r => r.IdUti == utilizador.IdUti && r.IdAtividade == id && !r.IsCancelado);

        if (registo == null)
        {
            TempData["Erro"] = "Nao tens uma inscricao ativa nesta atividade.";
            return RedirectToAction(nameof(Index));
        }

        registo.IsCancelado = true;
        await _context.SaveChangesAsync();

        TempData["Sucesso"] = "Inscricao na atividade cancelada com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<HashSet<int>> ObterAtividadesInscritasAsync()
    {
        var nomeUtilizador = User.FindFirstValue(ClaimTypes.Name);
        if (string.IsNullOrWhiteSpace(nomeUtilizador))
            return new HashSet<int>();

        var utilizador = await _context.Utilizadores
            .FirstOrDefaultAsync(u => u.Nome == nomeUtilizador);

        if (utilizador == null)
            return new HashSet<int>();

        var ids = await _context.RegistoAtividades
            .Where(r => r.IdUti == utilizador.IdUti && !r.IsCancelado)
            .Select(r => r.IdAtividade)
            .ToListAsync();

        return new HashSet<int>(ids);
    }

    // GET: usa apenas IAtividadeReadRepository (leitura)
    [HttpGet]
    public async Task<IActionResult> Criar(int idEvento)
    {
        var evento = await _eventoRepository.GetByIdAsync(idEvento);
        if (evento == null) return NotFound();

        ViewBag.EventoNome = evento.Nome;
        ViewBag.EventoId = idEvento;
        ViewBag.Categorias = await _categoriaRepository.GetAllAsync();

        var dto = new CriarAtividadeDto { IdEvento = idEvento };
        return View("CriarAtividade", dto);
    }

    // POST: usa IAtividadeWriteRepository (escrita) - só admin
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(CriarAtividadeDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categorias = await _categoriaRepository.GetAllAsync();
            ViewBag.EventoId = dto.IdEvento;
            return View("CriarAtividade", dto);
        }

        var atividade = new Atividade
        {
            IdEvento = dto.IdEvento,
            Nome = dto.Nome,
            Local = dto.Local,
            Capacidade = dto.Capacidade,
            IdCategoria = dto.IdCategoria
        };

        // ISP: operação de escrita usa IAtividadeWriteRepository
        await _atividadeWriteRepository.AddAsync(atividade);

        TempData["Sucesso"] = "Atividade criada com sucesso!";
        return RedirectToAction("Detalhes", "Evento", new { id = dto.IdEvento });
    }
}