namespace QueryLab.Api.Infrastructure;

// Garante que cada request HTTP inicia e limpa o contexto de métricas de query
public sealed class DiagnosticMiddleware
{
    private readonly RequestDelegate _next;

    public DiagnosticMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        QueryMetricsContext.Begin();
        try
        {
            await _next(context);
        }
        finally
        {
            QueryMetricsContext.Clear();
        }
    }
}
