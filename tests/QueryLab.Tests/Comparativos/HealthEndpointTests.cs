using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using QueryLab.Tests;

namespace QueryLab.Tests.Comparativos;

// O Program.cs lê a connection string e aplica migrations no startup (antes de Build()),
// então o teste precisa de um banco real e da string injetada via variável de ambiente
// — config adicionada por WithWebHostBuilder só vale após Build(), tarde demais.
[Collection("Database")]
public sealed class HealthEndpointTests : IClassFixture<DatabaseFixture>, IDisposable
{
    private readonly DatabaseFixture _fixture;

    public HealthEndpointTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", fixture.ConnectionString);
    }

    [Fact]
    public async Task GetHealth_ReturnsOkWithHealthyStatus()
    {
        await using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder.UseEnvironment("Production"));

        var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("healthy", body, StringComparison.OrdinalIgnoreCase);
    }

    public void Dispose() =>
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", null);
}
