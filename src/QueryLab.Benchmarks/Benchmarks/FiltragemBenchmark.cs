using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore;
using QueryLab.Infra;

namespace QueryLab.Benchmarks.Benchmarks;

// Compara filtragem IQueryable (WHERE no SQL) vs IEnumerable (WHERE na memória)
[MemoryDiagnoser]
public class FiltragemBenchmark
{
    private QueryLabDbContext _db = null!;

    [GlobalSetup]
    public void Setup()
    {
        var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
            ?? "Host=localhost;Database=querylab;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<QueryLabDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        _db = new QueryLabDbContext(options);
    }

    [Benchmark]
    public async Task<int> IQueryable_FiltragemNoBanco()
    {
        var umMesAtras = DateTime.UtcNow.AddMonths(-1);
        var result = await _db.Pedidos
            .Where(p => p.ValorTotal > 500 && p.DataPedido >= umMesAtras)
            .ToListAsync();
        return result.Count;
    }

    [Benchmark]
    public int IEnumerable_FiltragemNaMemoria()
    {
        var umMesAtras = DateTime.UtcNow.AddMonths(-1);
        var result = _db.Pedidos
            .AsEnumerable()
            .Where(p => p.ValorTotal > 500 && p.DataPedido >= umMesAtras)
            .ToList();
        return result.Count;
    }

    [GlobalCleanup]
    public void Cleanup() => _db.Dispose();
}
