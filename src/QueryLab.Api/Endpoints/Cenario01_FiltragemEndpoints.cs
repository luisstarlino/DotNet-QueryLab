// Cenário 1: demonstra a diferença entre filtrar no banco (IQueryable/WHERE no SQL)
// versus filtrar na memória (IEnumerable/WHERE em C#) — mostrando quantas rows trafegaram
namespace QueryLab.Api.Endpoints;

public static class Cenario01FiltragemEndpoints
{
    public static IEndpointRouteBuilder MapCenario01(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/cenario01").WithTags("Cenário 01 — Filtragem");

        // IQueryable: WHERE traduzido para SQL — apenas as rows filtradas chegam da rede
        group.MapGet("/iqueryable", () => Results.Ok("not implemented"));

        // IEnumerable: AsEnumerable() materializa tudo antes do WHERE — 1M rows trafegam
        group.MapGet("/ienumerable", () => Results.Ok("not implemented"));

        return app;
    }
}
