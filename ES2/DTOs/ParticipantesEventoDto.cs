namespace ES2.DTOs;

public class ParticipantesEventoDto
{
    public int IdEvento { get; set; }
    public string NomeEvento { get; set; } = null!;
    public DateOnly? DataEvento { get; set; }
    public TimeOnly? HoraEvento { get; set; }
    public string? LocalEvento { get; set; }
    public List<ParticipanteInscritoDto> Participantes { get; set; } = new();
}
