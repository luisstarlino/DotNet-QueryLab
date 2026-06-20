// Cenário 1: demonstra a diferença entre filtrar no banco (IQueryable/WHERE no SQL)
// versus filtrar na memória (IEnumerable/WHERE em C#) — mostrando quantas rows trafegaram
using Microsoft.EntityFrameworkCore;
using QueryLab.Api.Infrastructure;
using QueryLab.Infra;

namespace QueryLab.Api.Endpoints;

public static class Cenario01FiltragemEndpoints
{
    public static IEndpointRouteBuilder MapCenario01(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/cenario01").WithTags("Cenário 01 — Filtragem");

        // IQueryable: WHERE traduzido para SQL — apenas as rows filtradas chegam da rede
        group.MapGet("/iqueryable", async (IDbContextFactory<QueryLabDbContext> factory) =>
        {
            // --- 1.: Context
            var ctx = QueryMetricsContext.Current!;
            await using var db = await factory.CreateDbContextAsync();

            // DataPedido é timestamptz no Postgres; Npgsql exige DateTime UTC como parâmetro
            var umMesAtras = DateTime.UtcNow.AddMonths(-1);

            // --- WHERE DENTRO do SQL: só vai trafegar as rows filtradas
            var dados = await db.Pedidos
            .Where(p => p.ValorTotal > 500 && p.DataPedido >= umMesAtras)
            .ToListAsync();

            ctx.RegistrosTrafegados = dados.Count;
            var metrics = ctx.End("Cenário 01 — Filtragem", "IQueryable", dados.Count);

            return Results.Ok(new QueryResult<int>(dados.Count, metrics));
        });

        // IEnumerable: AsEnumerable() materializa tudo antes do WHERE — 1M rows trafegam
        group.MapGet("/ienumerable", () => async (IDbContextFactory<QueryLabDbContext> factory) =>
        {
            // --- 1.: Context
            var ctx = QueryMetricsContext.Current!;
            await using var db = await factory.CreateDbContextAsync();

            // DataPedido é timestamptz no Postgres; Npgsql exige DateTime UTC como parâmetro
            var umMesAtras = DateTime.UtcNow.AddMonths(-1);

            // --- PROJETA TUDO PRIMEIRO
            var dados = db.Pedidos
            .Where(p => p.ValorTotal > 500 && p.DataPedido >= umMesAtras)
            .AsEnumerable()
            .ToList();

            ctx.RegistrosTrafegados = dados.Count;
            var metrics = ctx.End("Cenário 02 — Filtragem", "IEnumerable", dados.Count);

            return Results.Ok(new QueryResult<int>(dados.Count, metrics));
        });

        return app;
    }
}
