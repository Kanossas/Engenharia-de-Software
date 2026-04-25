namespace ES2.DTOs;

public class ParticipanteInscritoDto
{
    public int IdUtilizador { get; set; }
    public string Nome { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Telemovel { get; set; }
}
