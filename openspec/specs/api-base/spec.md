# api-base Specification

## Purpose

Defines the ASP.NET Core Minimal API surface for QueryLab: the standardized response wrapper, per-scenario endpoint groups, request-scoped metrics capture middleware, and the health check endpoint.

## Requirements

### Requirement: Standardized QueryResult<T> response wrapper
Every scenario endpoint SHALL return `QueryResult<T>` wrapping the data payload and a `QueryMetrics` record. `QueryMetrics` SHALL contain: `Cenario`, `Abordagem`, `SqlGerado`, `TempoMs`, `RegistrosTrafegados`, `RegistrosRetornados`, `MemoriaAlocadaBytes`.

#### Scenario: Response serializes both data and metrics
- **WHEN** any scenario endpoint is called
- **THEN** the JSON response contains a top-level `data` field and a `metrics` field with all required metric properties

#### Scenario: Metrics reflect actual execution
- **WHEN** a query endpoint executes a LINQ query
- **THEN** `metrics.tempoMs` is greater than zero and `metrics.sqlGerado` contains the SQL text that was executed

### Requirement: Minimal API endpoint groups per scenario
Endpoints SHALL be organized using `MapGroup` with one extension method per scenario file (`app.MapCenario01()` through `app.MapCenario06()`). Each scenario file SHALL exist and register its route group even if the handler bodies are not yet implemented (returning `Results.Ok("not implemented")` is acceptable).

#### Scenario: All 6 scenario route groups are registered
- **WHEN** the API starts
- **THEN** GET requests to `/cenario01/...` through `/cenario06/...` return HTTP 200 (not 404)

### Requirement: DiagnosticMiddleware captures request metrics
A `DiagnosticMiddleware` SHALL initialize the `AsyncLocal<QueryMetricsContext>` at the start of each request and finalize it at the end, making captured SQL and timing available to the response wrapper.

#### Scenario: Middleware initializes context per request
- **WHEN** an HTTP request is received
- **THEN** `DiagnosticMiddleware` calls `QueryMetricsContext.Begin()` before the handler executes

#### Scenario: Middleware context does not leak between requests
- **WHEN** two concurrent requests execute
- **THEN** each request's `SqlGerado` contains only SQL from its own execution

### Requirement: Health check endpoint
The API SHALL expose `GET /health` returning HTTP 200 with `{"status":"healthy"}`.

#### Scenario: Health check returns healthy
- **WHEN** `GET /health` is called against a running API with database connected
- **THEN** HTTP 200 is returned with body containing `status: "healthy"`
