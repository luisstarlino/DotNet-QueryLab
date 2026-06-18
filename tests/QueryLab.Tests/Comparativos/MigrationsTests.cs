using Npgsql;
using QueryLab.Tests;

namespace QueryLab.Tests.Comparativos;

[Collection("Database")]
public sealed class MigrationsTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public MigrationsTests(DatabaseFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task AllExpectedTablesExistAfterMigration()
    {
        var expectedTables = new[] { "Clientes", "Produtos", "Pedidos", "ItensPedido" };

        await using var conn = new NpgsqlConnection(_fixture.ConnectionString);
        await conn.OpenAsync();

        foreach (var table in expectedTables)
        {
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT to_regclass('public.\"{table}\"')";
            var result = await cmd.ExecuteScalarAsync();
            Assert.NotEqual(DBNull.Value, result);
            Assert.NotNull(result);
        }
    }
}
