// Cenário 5: demonstra a diferença em SELECT — IQueryable projeta só as colunas necessárias,
// IEnumerable carrega a entidade completa e projeta depois em memória
namespace QueryLab.Api.Endpoints;

public static class Cenario05ProjecaoEndpoints
{
    public static IEndpointRouteBuilder MapCenario05(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/cenario05").WithTags("Cenário 05 — Projeção");

        // IQueryable: SELECT apenas Id, ValorTotal, DataPedido
        group.MapGet("/iqueryable", () => Results.Ok("not implemented"));

        // IEnumerable: SELECT * (todas as colunas) e projeta em memória depois
        group.MapGet("/ienumerable", () => Results.Ok("not implemented"));

        return app;
    }
}
