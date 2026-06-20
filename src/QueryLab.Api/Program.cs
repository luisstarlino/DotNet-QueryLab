using Microsoft.EntityFrameworkCore;
using QueryLab.Api.Endpoints;
using QueryLab.Api.Infrastructure;
using QueryLab.Infra;
using QueryLab.Infra.Seed;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, services, config) =>
        config.ReadFrom.Configuration(ctx.Configuration)
              .ReadFrom.Services(services)
              .WriteTo.Console());

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    builder.Services.AddSingleton<SqlInterceptor>();

    builder.Services.AddDbContextFactory<QueryLabDbContext>((sp, opts) =>
    {
        opts.UseNpgsql(connectionString);
        opts.AddInterceptors(sp.GetRequiredService<SqlInterceptor>());

        if (builder.Environment.IsDevelopment())
        {
            opts.EnableSensitiveDataLogging();
            opts.EnableDetailedErrors();
        }
    });

    var app = builder.Build();

    app.UseMiddleware<DiagnosticMiddleware>();

    // Aplica migrations com retry para aguardar o PostgreSQL subir no Docker.
    // Roda em ambos os modos (api e seed) para garantir que as tabelas existam
    // mesmo quando o serviço de seed é executado isoladamente.
    await ApplyMigrationsWithRetryAsync(app.Services, app.Logger);

    // Modo seed: popula o banco e encerra
    if (args.Contains("--seed"))
    {
        await DataSeeder.RunAsync(connectionString);
        return;
    }

    app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

    app.MapCenario01();
    app.MapCenario02();
    app.MapCenario03();
    app.MapCenario04();
    app.MapCenario05();
    app.MapCenario06();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

static async Task ApplyMigrationsWithRetryAsync(IServiceProvider services, ILogger logger)
{
    const int maxAttempts = 5;
    const int delayMs = 2000;

    for (int attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            await using var scope = services.CreateAsyncScope();
            var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<QueryLabDbContext>>();
            await using var db = await factory.CreateDbContextAsync();
            await db.Database.MigrateAsync();
            logger.LogInformation("Migrations applied successfully.");
            return;
        }
        catch (Exception ex) when (attempt < maxAttempts)
        {
            logger.LogWarning(ex, "Migration attempt {Attempt}/{Max} failed. Retrying in {Delay}ms...",
                attempt, maxAttempts, delayMs);
            await Task.Delay(delayMs);
        }
    }
}
