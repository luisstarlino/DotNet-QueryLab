using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore;
using QueryLab.Infra;

namespace QueryLab.Benchmarks.Benchmarks;

// Compara projeção IQueryable (SELECT só colunas necessárias) vs IEnumerable (SELECT *)
[MemoryDiagnoser]
public class ProjecaoBenchmark
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
    public async Task<int> IQueryable_ProjecaoParcial()
    {
        var result = await _db.Pedidos
            .Where(p => p.ValorTotal > 500)
            .Select(p => new { p.Id, p.ValorTotal, p.DataPedido })
            .ToListAsync();
        return result.Count;
    }

    [Benchmark]
    public int IEnumerable_ProjecaoCompleta()
    {
        var result = _db.Pedidos
            .Where(p => p.ValorTotal > 500)
            .AsEnumerable()
            .Select(p => new { p.Id, p.ValorTotal, p.DataPedido })
            .ToList();
        return result.Count;
    }

    [GlobalCleanup]
    public void Cleanup() => _db.Dispose();
}
