## ADDED Requirements

### Requirement: xUnit tests with Testcontainers.PostgreSql
The `QueryLab.Tests` project SHALL use `Testcontainers.PostgreSql` to spin up an isolated PostgreSQL instance per test class. The container SHALL have migrations applied before tests run. Tests SHALL NOT depend on the Docker Compose stack.

#### Scenario: Migrations apply cleanly in Testcontainers
- **WHEN** the `QueryLabDbContext` is pointed at a Testcontainers PostgreSQL instance
- **THEN** `MigrateAsync()` completes without error and all expected tables exist

#### Scenario: Testcontainers instance is isolated per test class
- **WHEN** two test classes run in parallel
- **THEN** each class operates on its own container with no shared state

### Requirement: Infrastructure validation tests
Tests SHALL verify: migrations create the correct schema, the seeder inserts expected record counts (using a reduced seed for speed), and the `SqlInterceptor` captures SQL when a query runs.

#### Scenario: Seeder inserts records in test environment
- **WHEN** `DataSeeder.SeedAsync()` is called in a Testcontainers environment with reduced counts (100 pedidos instead of 1M)
- **THEN** `Clientes`, `Produtos`, and `Pedidos` tables have the expected record counts

#### Scenario: SqlInterceptor captures SQL in tests
- **WHEN** a LINQ query is executed within a `QueryMetricsContext.Begin()` / `End()` scope
- **THEN** the captured `SqlGerado` is a non-empty string containing "SELECT"

### Requirement: Health endpoint test
An integration test SHALL call `GET /health` using `WebApplicationFactory<Program>` and assert HTTP 200.

#### Scenario: Health endpoint returns 200
- **WHEN** `GET /health` is called via WebApplicationFactory
- **THEN** response status is 200 OK and body contains `"healthy"`
