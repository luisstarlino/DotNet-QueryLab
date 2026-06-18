using System.Diagnostics;

namespace QueryLab.Api.Infrastructure;

public sealed class QueryMetricsContext
{
    private static readonly AsyncLocal<QueryMetricsContext?> _current = new();

    public static QueryMetricsContext? Current => _current.Value;

    private readonly Stopwatch _stopwatch;
    private readonly long _memoryBefore;

    public string SqlGerado { get; set; } = string.Empty;

    // Total de rows que vieram do banco (setado pelo código do endpoint)
    public int RegistrosTrafegados { get; set; }

    private QueryMetricsContext()
    {
        _memoryBefore = GC.GetTotalMemory(false);
        _stopwatch = Stopwatch.StartNew();
    }

    public static QueryMetricsContext Begin()
    {
        var ctx = new QueryMetricsContext();
        _current.Value = ctx;
        return ctx;
    }

    public QueryMetrics End(string cenario, string abordagem, int registrosRetornados)
    {
        _stopwatch.Stop();
        var memAfter = GC.GetTotalMemory(false);
        _current.Value = null;

        return new QueryMetrics(
            cenario,
            abordagem,
            SqlGerado,
            _stopwatch.ElapsedMilliseconds,
            RegistrosTrafegados,
            registrosRetornados,
            Math.Max(0, memAfter - _memoryBefore)
        );
    }

    public static void Clear()
    {
        _current.Value = null;
    }
}
