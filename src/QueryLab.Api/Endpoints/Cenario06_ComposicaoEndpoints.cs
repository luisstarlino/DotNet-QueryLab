// Cenário 6: demonstra composição dinâmica de queries — IQueryable compõe WHEREs opcionais
// e gera um único SQL; List carrega tudo e filtra na memória
namespace QueryLab.Api.Endpoints;

public static class Cenario06ComposicaoEndpoints
{
    public static IEndpointRouteBuilder MapCenario06(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/cenario06").WithTags("Cenário 06 — Composição Dinâmica");

        // IQueryable: cada filtro opcional adiciona um WHERE ao SQL — uma query eficiente
        group.MapGet("/iqueryable", (decimal? valorMin, int? categoriaId, DateTime? dataInicio) =>
            Results.Ok("not implemented"));

        // List: carrega tudo com Includes e filtra em memória — custo enorme
        group.MapGet("/list", (decimal? valorMin, int? categoriaId, DateTime? dataInicio) =>
            Results.Ok("not implemented"));

        return app;
    }
}
