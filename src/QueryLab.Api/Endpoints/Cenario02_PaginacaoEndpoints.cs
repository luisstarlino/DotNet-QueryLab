// Cenário 2: demonstra a diferença entre paginar no banco (OFFSET/FETCH no SQL)
// versus paginar na memória (Skip/Take em C# após materializar tudo)
namespace QueryLab.Api.Endpoints;

public static class Cenario02PaginacaoEndpoints
{
    public static IEndpointRouteBuilder MapCenario02(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/cenario02").WithTags("Cenário 02 — Paginação");

        // IQueryable: OFFSET/FETCH gerado no SQL — só a página trafega
        group.MapGet("/iqueryable", (int page = 1, int size = 20) => Results.Ok("not implemented"));

        // IEnumerable: ordena e pagina na memória após materializar tudo
        group.MapGet("/ienumerable", (int page = 1, int size = 20) => Results.Ok("not implemented"));

        return app;
    }
}
