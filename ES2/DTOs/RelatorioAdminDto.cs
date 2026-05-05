namespace ES2.DTOs;

public class RelatorioAdminDto
{
    public int TotalEventos { get; set; }
    public int TotalUtilizadores { get; set; }
    public int TotalInscricoes { get; set; }
    public double ReceitaTotal { get; set; }
    public double MediaEventosPorMes { get; set; }
    public double MediaParticipantesPorEvento { get; set; }
    public List<EventosPorMesDto> EventosPorMes { get; set; } = new();
    public List<EventosPorCategoriaDto> EventosPorCategoria { get; set; } = new();
}

public class EventosPorMesDto
{
    public int Ano { get; set; }
    public int Mes { get; set; }
    public string NomeMes { get; set; } = string.Empty;
    public int TotalEventos { get; set; }
}

public class EventosPorCategoriaDto
{
    public string Categoria { get; set; } = string.Empty;
    public int TotalEventos { get; set; }
}
