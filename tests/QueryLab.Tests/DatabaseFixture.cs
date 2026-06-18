using Microsoft.EntityFrameworkCore;
using QueryLab.Infra;
using Testcontainers.PostgreSql;

namespace QueryLab.Tests;

public sealed class DatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16")
        .WithDatabase("querylab_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public QueryLabDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<QueryLabDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;
        return new QueryLabDbContext(options);
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await using var db = CreateDbContext();
        await db.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}
