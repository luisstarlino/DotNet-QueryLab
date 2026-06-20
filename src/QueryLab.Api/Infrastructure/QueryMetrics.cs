namespace QueryLab.Api.Infrastructure;

public record QueryMetrics(
    string Cenario,
    string Abordagem,
    string SqlGerado,
    long TempoMs,
    int RegistrosTrafegados,
    int RegistrosRetornados,
    long MemoriaAlocadaBytes,
    double MemoriaAlocadaMegaBytes
);
