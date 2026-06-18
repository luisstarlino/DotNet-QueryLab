using Microsoft.EntityFrameworkCore;
using QueryLab.Infra.Seed;

namespace QueryLab.Tests.Comparativos;

[Collection("Database")]
public sealed class SeederTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public SeederTests(DatabaseFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task SeederPopulatesAllTablesWithExpectedCounts()
    {
        // Roda o seeder com os dados reais (pequeno: testcontainer é efêmero)
        await DataSeeder.RunAsync(_fixture.ConnectionString);

        await using var db = _fixture.CreateDbContext();

        var clientes = await db.Clientes.CountAsync();
        var produtos = await db.Produtos.CountAsync();
        var pedidos = await db.Pedidos.CountAsync();
        var itens = await db.ItensPedido.CountAsync();

        Assert.Equal(100, clientes);
        Assert.Equal(50, produtos);
        Assert.True(pedidos > 0, "Deve haver pelo menos um pedido");
        Assert.True(itens >= pedidos, "Itens devem ser pelo menos iguais aos pedidos");
    }
}
