using Microsoft.EntityFrameworkCore;
using QueryLab.Api.Infrastructure;

namespace QueryLab.Tests.Comparativos;

[Collection("Database")]
public sealed class SqlInterceptorTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public SqlInterceptorTests(DatabaseFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task InterceptorCapturesSqlOnQueryExecution()
    {
        var interceptor = new SqlInterceptor();

        var options = new DbContextOptionsBuilder<QueryLab.Infra.QueryLabDbContext>()
            .UseNpgsql(_fixture.ConnectionString)
            .AddInterceptors(interceptor)
            .Options;

        var ctx = QueryMetricsContext.Begin();

        await using (var db = new QueryLab.Infra.QueryLabDbContext(options))
        {
            _ = await db.Clientes.Take(1).ToListAsync();
        }

        Assert.Contains("SELECT", ctx.SqlGerado, StringComparison.OrdinalIgnoreCase);
        QueryMetricsContext.Clear();
    }
}
