// Cenário 2: demonstra a diferença entre paginar no banco (OFFSET/FETCH no SQL)
// versus paginar na memória (Skip/Take em C# após materializar tudo)
using Microsoft.EntityFrameworkCore;
using QueryLab.Api.Infrastructure;
using QueryLab.Infra;

namespace QueryLab.Api.Endpoints;

public static class Cenario02PaginacaoEndpoints
{
    public static IEndpointRouteBuilder MapCenario02(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/cenario02").WithTags("Cenário 02 — Paginação");

        // IQueryable: OFFSET/FETCH gerado no SQL — só a página trafega
        group.MapGet("/iqueryable", async (IDbContextFactory<QueryLabDbContext> factory) =>
        {
            int page = 1, size = 20;

            // --- 1.: Context
            var ctx = QueryMetricsContext.Current!;
            await using var db = await factory.CreateDbContextAsync();

            // --- Projeção dentro do banco usando OFFSET no SQL > Só vai trafegar os dados filtrados
            var dados = await db.Pedidos.OrderBy(p => p.DataPedido).Skip((page - 1) * size).Take(size).ToListAsync();

            ctx.RegistrosTrafegados = dados.Count;

            var metrics = ctx.End("Cenáraio 01 - Paginação Backend (OFFSET)", "IQueryable", dados.Count);

            return Results.Ok(new QueryResult<int>(dados.Count, metrics));
        });

        // IEnumerable: ordena e pagina na memória após materializar tudo
        group.MapGet("/ienumerable", (int page = 1, int size = 20) => Results.Ok("not implemented"));

        return app;
    }
}
