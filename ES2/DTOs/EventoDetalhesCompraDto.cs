using ES2.Models;

namespace ES2.DTOs;

public class EventoDetalhesCompraDto
{
    public Evento Evento { get; set; } = null!;

    public IReadOnlyCollection<OfertaBilheteEventoDto> OfertasBilhete { get; set; } = Array.Empty<OfertaBilheteEventoDto>();

    public bool JaInscrito { get; set; }

    public int? IdBilheteAtivo { get; set; }
}
