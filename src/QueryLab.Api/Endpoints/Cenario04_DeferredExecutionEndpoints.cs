// Cenário 4: demonstra o perigo de retornar IQueryable como IEnumerable
// de um método que dispõe o DbContext — a query ainda não foi executada e o contexto já sumiu
namespace QueryLab.Api.Endpoints;

public static class Cenario04DeferredExecutionEndpoints
{
    public static IEndpointRouteBuilder MapCenario04(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/cenario04").WithTags("Cenário 04 — Deferred Execution");

        // Problema: retorna IQueryable como IEnumerable antes de executar — DbContext disposed ao enumerar
        group.MapGet("/problema", () => Results.Ok("not implemented"));

        // Solução: materializa com ToList() dentro do using antes de retornar
        group.MapGet("/solucao", () => Results.Ok("not implemented"));

        return app;
    }
}
