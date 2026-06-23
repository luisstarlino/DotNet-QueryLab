using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore;
using QueryLab.Infra;

namespace QueryLab.Benchmarks.Benchmarks;

// Compara paginação IQueryable (OFFSET/FETCH no SQL) vs IEnumerable (Skip/Take na memória)
[MemoryDiagnoser]
public class PaginacaoBenchmark
{
    private QueryLabDbContext _db = null!;
    private const int Page = 5;
    private const int Size = 20;

    [GlobalSetup]
    public void Setup()
    {
        var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL") ?? "Host=localhost;Database=querylab;Username=postgres;Password=postgres";
        var options = new DbContextOptionsBuilder<QueryLabDbContext>().UseNpgsql(connectionString).Options;
        _db = new QueryLabDbContext(options);
    }

    [Benchmark]
    public async Task<int> IQueryable_PaginacaoNoBanco()
    {
        var result = await _db.Pedidos
            .OrderBy(p => p.DataPedido)
            .Skip((Page - 1) * Size)
            .Take(Size)
            .ToListAsync();
        return result.Count;
    }

    [Benchmark]
    public int IEnumerable_PaginacaoNaMemoria()
    {
        var result = _db.Pedidos
            .OrderBy(p => p.DataPedido)
            .AsEnumerable()
            .Skip((Page - 1) * Size)
            .Take(Size)
            .ToList();
        return result.Count;
    }

    [GlobalCleanup]
    public void Cleanup() => _db.Dispose();
}
