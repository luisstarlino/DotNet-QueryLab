using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

namespace QueryLab.Api.Infrastructure;

// Intercepta comandos EF Core para capturar SQL gerado e associar ao contexto de métricas ativo
public sealed class SqlInterceptor : DbCommandInterceptor
{
    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        var ctx = QueryMetricsContext.Current;
        if (ctx is not null)
            ctx.SqlGerado = command.CommandText;

        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result,
        CancellationToken cancellationToken = default)
    {
        var ctx = QueryMetricsContext.Current;
        if (ctx is not null)
            ctx.SqlGerado = command.CommandText;

        return base.ScalarExecutingAsync(command, eventData, result, cancellationToken);
    }
}
