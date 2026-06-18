using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace QueryLab.Infra;

// Fábrica usada pelo 'dotnet ef' em design-time (migrations), sem precisar de DI completo
public sealed class QueryLabDbContextFactory : IDesignTimeDbContextFactory<QueryLabDbContext>
{
    public QueryLabDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
            ?? "Host=localhost;Database=querylab;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<QueryLabDbContext>()
            .UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsAssembly(typeof(QueryLabDbContext).Assembly.FullName))
            .Options;

        return new QueryLabDbContext(options);
    }
}
