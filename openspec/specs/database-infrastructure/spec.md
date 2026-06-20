# database-infrastructure Specification

## Purpose

Defines the EF Core data access infrastructure for QueryLab: the DbContext with SQL-capturing interceptor, the Docker Compose PostgreSQL environment with healthchecks, and migration application on startup.

## Requirements

### Requirement: QueryLabDbContext with SqlInterceptor registered
The `QueryLabDbContext` SHALL be registered via `IDbContextFactory<QueryLabDbContext>`. A `DbCommandInterceptor` (`SqlInterceptor`) SHALL be registered that captures CommandText, execution time via `Stopwatch`, and row count. In Development, `EnableSensitiveDataLogging()` and `EnableDetailedErrors()` SHALL be enabled.

#### Scenario: SqlInterceptor captures SQL for a query
- **WHEN** a LINQ query is executed against `QueryLabDbContext`
- **THEN** `SqlInterceptor` stores the CommandText in the current `AsyncLocal<QueryMetricsContext>` before the call returns

#### Scenario: Execution time is measured
- **WHEN** a database command completes
- **THEN** elapsed milliseconds are recorded in `QueryMetrics.TempoMs`

### Requirement: Docker Compose with PostgreSQL 16 and healthcheck
The `docker-compose.yml` SHALL define a `db` service using `postgres:16`, a named volume for persistence, and a healthcheck using `pg_isready`. The `api` service SHALL `depends_on` the `db` service with `condition: service_healthy`. A `seed` service SHALL share the same build, have `restart: no`, and exit after seeding.

#### Scenario: API waits for healthy database before starting
- **WHEN** `docker-compose up` is run from cold state
- **THEN** the `api` container does not start until the `db` healthcheck returns healthy

#### Scenario: Database data persists across restarts
- **WHEN** `docker-compose restart db` is run after seeding
- **THEN** data remains intact in the named volume

### Requirement: Migrations applied on API startup
The API SHALL apply pending EF Core migrations on startup using `MigrateAsync()` inside a retry loop (max 5 attempts, 2s delay) to handle container cold-start timing.

#### Scenario: Migrations apply on first start
- **WHEN** the API starts against a fresh empty PostgreSQL instance
- **THEN** all tables are created and the `__EFMigrationsHistory` table is populated

#### Scenario: Startup retries if database is not yet ready
- **WHEN** the API starts before PostgreSQL is fully ready
- **THEN** the API retries `MigrateAsync()` up to 5 times before throwing
