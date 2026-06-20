## 1. Solution and Projects

- [x] 1.1 Create `QueryLab.sln` and all 5 projects via `dotnet new` (`QueryLab.Api`, `QueryLab.Domain`, `QueryLab.Infra`, `QueryLab.Benchmarks`, `QueryLab.Tests`)
- [x] 1.2 Add all project references and NuGet packages (EF Core 10, Npgsql, Serilog, BenchmarkDotNet, xUnit, Testcontainers.PostgreSql, Microsoft.AspNetCore.Mvc.Testing)
- [x] 1.3 Configure `<Nullable>enable</Nullable>`, `<ImplicitUsings>enable</ImplicitUsings>` and `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` (Release only) in all csproj files
- [x] 1.4 Verify solution builds cleanly: `dotnet build`

## 2. Domain Entities

- [x] 2.1 Create `Cliente.cs` with properties: `Id` (Guid), `Nome` (string), `Email` (string), `DataCadastro` (DateTime)
- [x] 2.2 Create `Produto.cs` with properties: `Id` (Guid), `Nome` (string), `Preco` (decimal), `CategoriaId` (int)
- [x] 2.3 Create `Pedido.cs` with properties: `Id` (Guid), `ClienteId` (Guid), `DataPedido` (DateTime), `ValorTotal` (decimal), `Status` (string), nav property `Cliente`, `ICollection<ItemPedido> Items`
- [x] 2.4 Create `ItemPedido.cs` with properties: `Id` (Guid), `PedidoId` (Guid), `ProdutoId` (Guid), `Quantidade` (int), `PrecoUnitario` (decimal), nav properties `Pedido`, `Produto`

## 3. EF Core Infrastructure

- [x] 3.1 Create `QueryLabDbContext.cs` in `QueryLab.Infra` with `DbSet<>` for all 4 entities
- [x] 3.2 Create `ClienteConfiguration.cs`, `ProdutoConfiguration.cs`, `PedidoConfiguration.cs` implementing `IEntityTypeConfiguration<T>` with all FK relationships via Fluent API
- [x] 3.3 Add indexes to `Pedido` on `DataPedido` and `ValorTotal`; add index to `Produto` on `CategoriaId`
- [x] 3.4 Add initial EF Core migration: `dotnet ef migrations add InitialCreate --project src/QueryLab.Infra --startup-project src/QueryLab.Api`
- [x] 3.5 Verify migration SQL is correct (review `Migrations/` snapshot)

## 4. SqlInterceptor and QueryMetrics

- [x] 4.1 Create `QueryMetrics.cs` record with all required fields: `Cenario`, `Abordagem`, `SqlGerado`, `TempoMs`, `RegistrosTrafegados`, `RegistrosRetornados`, `MemoriaAlocadaBytes`
- [x] 4.2 Create `QueryResult<T>.cs` record wrapping `T Data` and `QueryMetrics Metrics`
- [x] 4.3 Create `QueryMetricsContext.cs` with `AsyncLocal<QueryMetricsContext>` and `Begin()` / `End()` / `Current` static members
- [x] 4.4 Create `SqlInterceptor.cs` extending `DbCommandInterceptor`, overriding `ReaderExecutedAsync` and `ScalarExecutedAsync` to capture `CommandText`, elapsed ms, and store in `QueryMetricsContext.Current`
- [x] 4.5 Create `DiagnosticMiddleware.cs` that calls `QueryMetricsContext.Begin()` before `next(context)` and `End()` after

## 5. API Base and Program.cs

- [x] 5.1 Configure `Program.cs`: register `IDbContextFactory<QueryLabDbContext>`, add `SqlInterceptor`, add Serilog, register `DiagnosticMiddleware`
- [x] 5.2 Add startup migration logic in `Program.cs` with retry loop (5 attempts, 2s delay)
- [x] 5.3 Create `GET /health` endpoint returning `{"status":"healthy"}`
- [x] 5.4 Create stub endpoint files for all 6 scenarios (`Cenario01_FiltragemEndpoints.cs` through `Cenario06_ComposicaoEndpoints.cs`), each with `MapGroup` and placeholder handlers returning `Results.Ok("not implemented")`
- [x] 5.5 Register all 6 scenario groups in `Program.cs` via extension methods (`app.MapCenario01()` etc.)
- [x] 5.6 Verify API starts and all routes respond: `dotnet run --project src/QueryLab.Api`

## 6. Data Seeder

- [x] 6.1 Create `DataSeeder.cs` in `QueryLab.Infra/Seed/` accepting `NpgsqlConnection` (direct, not via DbContext)
- [x] 6.2 Implement idempotency check: query count for each table and skip if already populated
- [x] 6.3 Implement `Cliente` seed (100 records) via `BeginBinaryImportAsync`
- [x] 6.4 Implement `Produto` seed (50 records, price R$10–R$5000, CategoriaId 1–10) via `BeginBinaryImportAsync`
- [x] 6.5 Implement `Pedido` seed (1,000,000 records in batches of 50k) with random dates across last 2 years, varied ValorTotal and 4+ status values; print progress every 250k rows
- [x] 6.6 Implement `ItemPedido` seed (~3 items per pedido = ~3,000,000 records in batches of 50k); print progress every 500k rows
- [x] 6.7 Wire `DataSeeder` as a console entry point (check `args` for `--seed` flag) in `Program.cs` or as a dedicated project entrypoint in `QueryLab.Api`

## 7. Docker Compose

- [x] 7.1 Create `docker-compose.yml` with `db` service (postgres:16, named volume, healthcheck `pg_isready`)
- [x] 7.2 Create `Dockerfile` for `QueryLab.Api` (multi-stage: sdk build + runtime)
- [x] 7.3 Add `api` service to compose: build from Dockerfile, port 5000:8080, depends_on db with `condition: service_healthy`, `DATABASE_URL` env var
- [x] 7.4 Add `seed` service to compose: same build as api, entrypoint `["dotnet", "QueryLab.Api.dll", "--seed"]`, depends_on db healthy, `restart: no`
- [x] 7.5 Add `.dockerignore` file
- [ ] 7.6 Test full compose stack: `docker-compose up -d --build` and verify API health and seed completion
- [ ] 7.7 Test seed idempotency: run `docker-compose run --rm seed` a second time and verify no duplicates

## 8. Tests

- [x] 8.1 Configure `QueryLab.Tests.csproj` with references to `QueryLab.Api`, `QueryLab.Infra`, xUnit, Testcontainers.PostgreSql, WebApplicationFactory
- [x] 8.2 Create `DatabaseFixture.cs` that starts a Testcontainers PostgreSQL, applies migrations, and exposes connection string; implement `IAsyncLifetime`
- [x] 8.3 Create `MigrationsTests.cs`: verify all expected tables exist after `MigrateAsync()`
- [x] 8.4 Create `SeederTests.cs`: run `DataSeeder` with reduced counts (100 pedidos), assert exact counts in each table
- [x] 8.5 Create `SqlInterceptorTests.cs`: execute a simple `QueryLabDbContext` LINQ query inside `QueryMetricsContext.Begin()` scope and assert `SqlGerado` contains "SELECT"
- [x] 8.6 Create `HealthEndpointTests.cs` using `WebApplicationFactory<Program>`: call `GET /health` and assert HTTP 200 + `"healthy"` in body
- [x] 8.7 Run all tests and verify they pass: `dotnet test`

## 9. CI Pipeline

- [x] 9.1 Create `.github/workflows/tests.yml` with trigger on push and pull_request to `main`
- [x] 9.2 Add steps: checkout, setup-dotnet (version 10), restore, build Release, test with `--logger "junit;LogFilePath=TestResults/results.xml"` and `--results-directory TestResults`
- [x] 9.3 Add `dorny/test-reporter@v1` step with `reporter: java-junit` pointing to `TestResults/results.xml`
- [x] 9.4 Add `actions/upload-artifact` step to upload `TestResults/` as artifact `test-results`

## 10. Git and Repository

- [x] 10.1 Run `git init` in project root
- [x] 10.2 Create `.gitignore` using `dotnet new gitignore`
- [x] 10.3 Stage all files and create initial commit: "feat: initial QueryLab infrastructure"
- [x] 10.4 Verify `git log` shows clean initial commit with all expected files tracked
