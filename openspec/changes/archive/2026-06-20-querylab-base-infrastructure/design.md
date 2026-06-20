## Context

QueryLab é um laboratório de .NET 10 para demonstrar diferenças mensuráveis entre IQueryable, IEnumerable e List com EF Core e PostgreSQL. A base de infraestrutura precisa estar funcional antes que os cenários de comparação sejam implementados manualmente pelo desenvolvedor.

Stack: .NET 10, EF Core + Npgsql, PostgreSQL 16, Docker Compose, Serilog, BenchmarkDotNet, xUnit, Testcontainers.

## Goals / Non-Goals

**Goals:**
- Solution .NET 10 com todos os projetos configurados e compilando
- Domínio completo (entidades + Fluent API + migrations) com índices nas colunas de filtro
- DbContext com `SqlInterceptor` capturando SQL gerado e métricas via `AsyncLocal`
- Seed funcional via `BINARY COPY` do PostgreSQL: 100 clientes, 50 produtos, 1M pedidos, 3M itens
- Docker Compose orquestrando `db` + `api` com healthcheck; serviço `seed` separado
- Wrapper `QueryResult<T>` + `QueryMetrics` disponível para os cenários implementarem
- Estrutura de endpoints por cenário (arquivos criados, rotas mapeadas, sem lógica de cenário)
- Testes de infraestrutura com Testcontainers (migrations, seed parcial, health)
- Pipeline GitHub Actions com export de resultados em JUnit XML
- Git inicializado com `.gitignore` e commit inicial

**Non-Goals:**
- Implementação dos 6 cenários de comparação (responsabilidade do desenvolvedor)
- BenchmarkDotNet executado na CI (apenas estrutura de benchmark criada)
- Auth, HTTPS, rate limiting, observabilidade de produção

## Decisions

### D1 — Seed via Npgsql BINARY COPY, não EF Core SaveChanges

Inserir 4M+ rows via `SaveChanges` em loop é inviável (horas). O Npgsql suporta `BeginBinaryImport` / `COPY FROM STDIN (FORMAT BINARY)` que processa centenas de milhares de rows por segundo via protocolo binário do PostgreSQL.

**Alternativas consideradas:**
- `BulkExtensions` para EF Core: dependência extra, mas funciona. Descartado para manter a stack mínima.
- SQL `COPY FROM CSV` via arquivo temp: funciona mas requer escrita em disco e permissões de arquivo.
- `COPY FROM STDIN` com texto: mais simples que binário mas 30-50% mais lento.

**Decisão:** `NpgsqlBinaryImporter` direto no `DataSeeder`, sem dependência adicional.

### D2 — `AsyncLocal<QueryMetricsContext>` no SqlInterceptor

O interceptor precisa correlacionar SQL executado com a requisição HTTP atual sem acoplar ao `HttpContext`. `AsyncLocal<T>` propaga pelo contexto de execução assíncrona, funciona com EF Core async e não cria acoplamento.

**Alternativas consideradas:**
- Injetar `IHttpContextAccessor` no interceptor: cria dependência de ASP.NET no interceptor EF, problemático em benchmarks/testes.
- Retornar SQL como parte do resultado de cada query: muda a assinatura de todos os repositórios.

**Decisão:** `AsyncLocal<QueryMetricsContext>` com método `Begin()` / `End()` chamado pelo middleware.

### D3 — Serviço `seed` separado no Docker Compose

O seed de 4M+ rows pode levar vários minutos. Rodá-lo como parte do startup da API introduz timeout de healthcheck. Um serviço `seed` separado com `restart: no` e `depends_on: db` roda uma vez e sai.

**Alternativas consideradas:**
- Endpoint `/seed` na API: conveniente mas inseguro para produção e bloqueia a API durante o seed.
- Init container: não suportado nativamente no Compose sem workaround.

**Decisão:** Serviço `seed` independente no Compose.

### D4 — Testcontainers apenas nos testes de infraestrutura

Testes de cenário (os 6 comparativos) são responsabilidade do desenvolvedor. Os testes criados na base devem validar apenas: migrations aplicam, seed popula counts corretos, healthcheck responde, wrapper de métricas serializa.

### D5 — GitHub Actions com JUnit XML export

O runner `dorny/test-reporter` lê JUnit XML e publica resultados diretamente no PR/commit, sem depender de paid features do GitHub. O `dotnet test` gera JUnit via `--logger "junit;LogFilePath=..."`.

## Risks / Trade-offs

- **[Risco] Seed lento mesmo com COPY** → Mitigação: executar em batches de 50k rows, mostrar progresso no console. Em hardware moderno, 1M rows via COPY binário leva 10-30s.
- **[Risco] Migrations falham no cold start do container** → Mitigação: healthcheck no serviço `db` com `pg_isready`; a API aplica migrations no startup com retry.
- **[Risco] `AsyncLocal` não propagar corretamente em alguns cenários de paralelismo** → Mitigação: os cenários do lab são sempre single-request; documentar a limitação.
- **[Trade-off] `IDbContextFactory` vs injeção direta** → Factory escolhida conforme CLAUDE.md para evitar problemas de lifetime em Minimal API.

## Migration Plan

1. Criar solution e projetos via `dotnet new`
2. Definir entidades e DbContext
3. Adicionar migration inicial: `dotnet ef migrations add InitialCreate`
4. Testar seed localmente com `docker-compose up db && dotnet run --seed`
5. Testar compose completo: `docker-compose up -d --build`
6. Rodar testes: `dotnet test`
7. `git init` + primeiro commit + push para GitHub

## Open Questions

- Nenhuma. Todos os aspectos da base estão definidos no CLAUDE.md.
