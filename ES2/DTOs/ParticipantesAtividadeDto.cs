namespace ES2.DTOs;

public class ParticipantesAtividadeDto
{
    public int IdAtividade { get; set; }
    public string NomeAtividade { get; set; } = null!;
    public int IdEvento { get; set; }
    public string NomeEvento { get; set; } = null!;
    public string? LocalAtividade { get; set; }
    public int? Capacidade { get; set; }
    public List<ParticipanteInscritoDto> Participantes { get; set; } = new();
}
