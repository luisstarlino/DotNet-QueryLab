// Cenário 3: demonstra o problema de múltipla enumeração em IQueryable —
// cada .Count(), .First(), .ToList() gera uma query separada ao banco
namespace QueryLab.Api.Endpoints;

public static class Cenario03MultiplaEnumeracaoEndpoints
{
    public static IEndpointRouteBuilder MapCenario03(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/cenario03").WithTags("Cenário 03 — Múltipla Enumeração");

        // Problema: 3 operações sobre IQueryable = 3 roundtrips ao banco
        group.MapGet("/problema", () => Results.Ok("not implemented"));

        // Solução: materializa uma vez com ToListAsync(), todas as operações seguintes em memória
        group.MapGet("/solucao", () => Results.Ok("not implemented"));

        return app;
    }
}
