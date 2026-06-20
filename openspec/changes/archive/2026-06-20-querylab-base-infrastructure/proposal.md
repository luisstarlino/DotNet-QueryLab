## Why

QueryLab precisa de uma base de infraestrutura funcional — banco de dados PostgreSQL via Docker, aplicação ASP.NET Core Minimal API configurada, seed de dados com 1M+ registros e pipeline de CI — para que o desenvolvedor possa implementar os 6 cenários de comparação IQueryable/IEnumerable/List de forma isolada e mensurável.

## What Changes

- Criação da solution .NET 10 com projetos `QueryLab.Api`, `QueryLab.Domain`, `QueryLab.Infra`, `QueryLab.Benchmarks` e `QueryLab.Tests`
- Definição das entidades de domínio: `Pedido`, `ItemPedido`, `Produto`, `Cliente`
- Configuração do `QueryLabDbContext` com Fluent API, índices e migrations
- `SqlInterceptor` para captura de SQL gerado e métricas de execução
- `DataSeeder` com bulk insert via `COPY` do PostgreSQL para 100 clientes, 50 produtos, 1M pedidos e 3M itens
- `docker-compose.yml` orquestrando API + PostgreSQL 16 + seed
- Estrutura de endpoints com wrapper `QueryResult<T>` + `QueryMetrics` (sem implementar os cenários)
- Inicialização do repositório git
- Pipeline GitHub Actions para rodar testes com export de resultados

## Capabilities

### New Capabilities

- `domain-entities`: Entidades de domínio com Fluent API e migrations EF Core
- `database-infrastructure`: DbContext, interceptor SQL, métricas e Docker Compose com PostgreSQL
- `data-seeder`: Bulk insert via PostgreSQL COPY com progresso no console
- `api-base`: Minimal API com estrutura de endpoints, middleware de diagnóstico e wrapper de resposta
- `test-infrastructure`: xUnit com Testcontainers.PostgreSql e estrutura de testes comparativos
- `ci-pipeline`: GitHub Actions pipeline com export de resultados de testes

### Modified Capabilities

## Impact

- Criação de toda a estrutura de pastas conforme definida no CLAUDE.md
- Dependências NuGet: EF Core 10, Npgsql, Serilog, BenchmarkDotNet, xUnit, Testcontainers.PostgreSql
- Docker: serviços `db`, `api` e `seed` no compose
- Git: repositório inicializado, `.gitignore` para .NET, branch `main`
- GitHub Actions: workflow `.github/workflows/tests.yml` com artifact export
