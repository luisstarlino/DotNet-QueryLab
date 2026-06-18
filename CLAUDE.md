# CLAUDE.md — QueryLab

## O que é este projeto

QueryLab é um laboratório prático de .NET 10 para demonstrar, medir e **provar** as diferenças entre `IEnumerable`, `IQueryable`, `List` e `ToList()` em cenários reais com Entity Framework Core e PostgreSQL.

O objetivo não é didático-teórico. É **instrumentado**: cada cenário loga o SQL gerado, mede tempo, conta alocações e mostra quantos registros trafegaram do banco para a aplicação.

---

## Stack e Infraestrutura

- **.NET 10** (ASP.NET Core Minimal API)
- **Entity Framework Core** com provider Npgsql (PostgreSQL)
- **PostgreSQL 16** via Docker
- **Docker Compose** para orquestração (API + banco)
- **BenchmarkDotNet** para benchmarks formais
- **xUnit** para testes comparativos
- **Serilog** para structured logging

### Estrutura de pastas

```
QueryLab/
├── CLAUDE.md
├── docker-compose.yml
├── .dockerignore
├── src/
│   ├── QueryLab.Api/
│   │   ├── QueryLab.Api.csproj
│   │   ├── Program.cs
│   │   ├── Dockerfile
│   │   ├── Endpoints/
│   │   │   ├── Cenario01_FiltragemEndpoints.cs
│   │   │   ├── Cenario02_PaginacaoEndpoints.cs
│   │   │   ├── Cenario03_MultiplaEnumeracaoEndpoints.cs
│   │   │   ├── Cenario04_DeferredExecutionEndpoints.cs
│   │   │   ├── Cenario05_ProjecaoEndpoints.cs
│   │   │   └── Cenario06_ComposicaoEndpoints.cs
│   │   ├── Infrastructure/
│   │   │   ├── QueryMetrics.cs
│   │   │   ├── SqlInterceptor.cs
│   │   │   └── DiagnosticMiddleware.cs
│   │   └── appsettings.json
│   ├── QueryLab.Domain/
│   │   ├── QueryLab.Domain.csproj
│   │   └── Entities/
│   │       ├── Pedido.cs
│   │       ├── ItemPedido.cs
│   │       ├── Produto.cs
│   │       └── Cliente.cs
│   ├── QueryLab.Infra/
│   │   ├── QueryLab.Infra.csproj
│   │   ├── QueryLabDbContext.cs
│   │   ├── Configurations/
│   │   │   ├── PedidoConfiguration.cs
│   │   │   ├── ProdutoConfiguration.cs
│   │   │   └── ClienteConfiguration.cs
│   │   ├── Migrations/
│   │   └── Seed/
│   │       └── DataSeeder.cs
│   └── QueryLab.Benchmarks/
│       ├── QueryLab.Benchmarks.csproj
│       └── Benchmarks/
│           ├── FiltragemBenchmark.cs
│           ├── PaginacaoBenchmark.cs
│           └── ProjecaoBenchmark.cs
├── tests/
│   └── QueryLab.Tests/
│       ├── QueryLab.Tests.csproj
│       └── Comparativos/
│           ├── FiltragemTests.cs
│           ├── PaginacaoTests.cs
│           └── ProjecaoTests.cs
└── QueryLab.sln
```

---

## Regras de Codificação

### Gerais
- Toda resposta de endpoint deve retornar um **wrapper padronizado** com dados + métricas:

```csharp
public record QueryResult<T>(
    T Data,
    QueryMetrics Metrics
);

public record QueryMetrics(
    string Cenario,
    string Abordagem,           // "IQueryable" | "IEnumerable" | "List"
    string SqlGerado,
    long TempoMs,
    int RegistrosTrafegados,    // total de rows que vieram do banco
    int RegistrosRetornados,    // total de rows na resposta final
    long MemoriaAlocadaBytes
);
```

- Use **Minimal API** com `MapGroup` para cada cenário
- Não use Controllers. Agrupe endpoints por cenário usando extension methods (`app.MapCenario01()`, etc.)
- Use `IDbContextFactory<QueryLabDbContext>` ao invés de injetar DbContext direto (evita problemas de lifetime)
- Toda string de conexão deve vir de variável de ambiente / `appsettings.json`
- NÃO CRIE OS CENÁRIOS. O desenvolvedor irá desenvolver para fins de testes. Monte apenas a arquitetura

### Entity Framework Core
- Use **Fluent API** para configuração (nunca Data Annotations)
- Configure índices adequados nas colunas usadas em filtros (DataPedido, ValorTotal, CategoriaId)
- Habilite **Sensitive Data Logging** apenas em Development
- Use `EnableDetailedErrors()` em Development
- Registre o `SqlInterceptor` customizado para capturar SQL gerado

### SQL Interceptor
Implemente um `DbCommandInterceptor` que:
1. Captura o CommandText de cada query executada
2. Mede o tempo de execução via `Stopwatch`
3. Armazena no `QueryMetrics` via `AsyncLocal<QueryMetrics>` ou similar pattern thread-safe
4. Conta rows retornadas via `DbDataReader.RecordsAffected` ou iteração

### Docker
- `docker-compose.yml` na raiz
- Serviço `db`: PostgreSQL 16, porta 5432, volume nomeado para persistência
- Serviço `api`: build do Dockerfile, porta 5000, depends_on db com healthcheck
- Serviço `seed`: mesmo build, entrypoint para rodar o DataSeeder e sair (ou usar um endpoint `/seed`)
- Variável `DATABASE_URL` compartilhada

### Seed de dados
O `DataSeeder` deve popular:
- **100 Clientes**
- **50 Produtos** (com categorias variadas e preços entre R$10 e R$5.000)
- **1.000.000 de Pedidos** com datas aleatórias nos últimos 2 anos, valores variados, status variados
- **3.000.000 de ItensVendidos** (~3 itens por pedido em média)
- Use `BulkInsert` ou `COPY` do PostgreSQL para performance no seed. Não insira 1M de registros via `SaveChangesAsync` em loop.
- Mostre progresso no console durante o seed (ex: "Inserindo pedidos... 250.000/1.000.000")

---

## Os 6 Cenários (detalhe de implementação) Obs.: NÃO IMPLEMENTAR (Implementação manual Luis Starlino)

### Cenário 1 — Filtragem: WHERE no banco vs WHERE na memória

**Endpoint A — `GET /cenario01/iqueryable`**
```
dbContext.Pedidos
    .Where(p => p.ValorTotal > 500 && p.DataPedido >= umMesAtras)
    // IQueryable: WHERE é traduzido para SQL
    .ToListAsync()
```
→ SQL esperado: `SELECT ... FROM "Pedidos" WHERE "ValorTotal" > 500 AND "DataPedido" >= @p0`

**Endpoint B — `GET /cenario01/ienumerable`**
```
dbContext.Pedidos
    .AsEnumerable()  // MATERIALIZA TUDO AQUI
    .Where(p => p.ValorTotal > 500 && p.DataPedido >= umMesAtras)
    .ToList()
```
→ SQL esperado: `SELECT ... FROM "Pedidos"` (sem WHERE — 1M de registros trafegam)

Ambos retornam o mesmo resultado, mas as métricas mostram a diferença brutal.

### Cenário 2 — Paginação: OFFSET no banco vs Skip na memória

**Endpoint A — `GET /cenario02/iqueryable?page=5&size=20`**
```
dbContext.Pedidos
    .OrderBy(p => p.DataPedido)
    .Skip((page - 1) * size)
    .Take(size)
    // IQueryable: OFFSET/FETCH no SQL
    .ToListAsync()
```

**Endpoint B — `GET /cenario02/ienumerable?page=5&size=20`**
```
dbContext.Pedidos
    .OrderBy(p => p.DataPedido)
    .AsEnumerable()
    .Skip((page - 1) * size)
    .Take(size)
    .ToList()
```

### Cenário 3 — Múltipla Enumeração

**Endpoint A — `GET /cenario03/problema`**
```
IEnumerable<Pedido> pedidos = dbContext.Pedidos.Where(p => p.ValorTotal > 1000);
// Cada operação abaixo executa uma query separada no banco!
var count = pedidos.Count();     // Query 1
var first = pedidos.First();     // Query 2
var list = pedidos.ToList();     // Query 3
```
→ Mostra 3 SQLs gerados. Três roundtrips ao banco.

**Endpoint B — `GET /cenario03/solucao`**
```
var pedidos = await dbContext.Pedidos
    .Where(p => p.ValorTotal > 1000)
    .ToListAsync();  // Materializa UMA vez

var count = pedidos.Count;       // Memória
var first = pedidos.First();     // Memória
// Mesmo resultado, 1 query só
```
→ Mostra 1 SQL gerado.

### Cenário 4 — Deferred Execution e Lifetime do DbContext

**Endpoint A — `GET /cenario04/problema`**
```
IEnumerable<Pedido> GetPedidosPerigoso()
{
    using var db = contextFactory.CreateDbContext();
    return db.Pedidos.Where(p => p.ValorTotal > 500);
    // Retorna IQueryable como IEnumerable — NÃO executou ainda!
    // O DbContext é disposto ao sair do using
}

// Ao enumerar aqui: ObjectDisposedException!
var resultado = GetPedidosPerigoso().ToList(); // BOOM
```

**Endpoint B — `GET /cenario04/solucao`**
```
List<Pedido> GetPedidosSeguro()
{
    using var db = contextFactory.CreateDbContext();
    return db.Pedidos.Where(p => p.ValorTotal > 500).ToList();
    // ToList() força execução ANTES do dispose
}
```

Este cenário retorna a exception no endpoint A (capturada) e sucesso no B.

### Cenário 5 — Projeção (Select)

**Endpoint A — `GET /cenario05/iqueryable`**
```
dbContext.Pedidos
    .Where(p => p.ValorTotal > 500)
    .Select(p => new { p.Id, p.ValorTotal, p.DataPedido })
    // IQueryable: SELECT só das 3 colunas
    .ToListAsync()
```
→ SQL: `SELECT "Id", "ValorTotal", "DataPedido" FROM "Pedidos" WHERE ...`

**Endpoint B — `GET /cenario05/ienumerable`**
```
dbContext.Pedidos
    .Where(p => p.ValorTotal > 500)
    .AsEnumerable()
    .Select(p => new { p.Id, p.ValorTotal, p.DataPedido })
    .ToList()
```
→ SQL: `SELECT "Id", "ValorTotal", "DataPedido", "ClienteId", "Status", ... FROM "Pedidos" WHERE ...` (todas as colunas)

### Cenário 6 — Composição Dinâmica de Queries

**Endpoint A — `GET /cenario06/iqueryable?valorMin=100&categoria=3&dataInicio=2024-01-01`**
```
IQueryable<Pedido> query = dbContext.Pedidos.AsQueryable();

if (valorMin.HasValue)
    query = query.Where(p => p.ValorTotal >= valorMin.Value);
if (categoriaId.HasValue)
    query = query.Where(p => p.Items.Any(i => i.Produto.CategoriaId == categoriaId.Value));
if (dataInicio.HasValue)
    query = query.Where(p => p.DataPedido >= dataInicio.Value);

return await query.ToListAsync();
// SQL tem apenas os WHEREs relevantes
```

**Endpoint B — `GET /cenario06/list?valorMin=100&categoria=3&dataInicio=2024-01-01`**
```
var todos = await dbContext.Pedidos
    .Include(p => p.Items).ThenInclude(i => i.Produto)
    .ToListAsync(); // Carrega TUDO com Includes

if (valorMin.HasValue)
    todos = todos.Where(p => p.ValorTotal >= valorMin.Value).ToList();
if (categoriaId.HasValue)
    todos = todos.Where(p => p.Items.Any(i => i.Produto.CategoriaId == categoriaId.Value)).ToList();
if (dataInicio.HasValue)
    todos = todos.Where(p => p.DataPedido >= dataInicio.Value).ToList();

return todos;
// Carregou milhões de registros incluindo joins pra filtrar na memória
```

---

## Benchmarks (BenchmarkDotNet)

O projeto `QueryLab.Benchmarks` deve ter benchmarks formais para os cenários 1, 2 e 5.

Cada benchmark compara a abordagem IQueryable vs IEnumerable/List, medindo:
- Tempo médio (Mean)
- Alocação de memória (MemoryDiagnoser)
- GC Collections (Gen0, Gen1, Gen2)

Rode com: `dotnet run -c Release --project src/QueryLab.Benchmarks`

Os benchmarks precisam de um PostgreSQL rodando. Use a mesma connection string do docker-compose.

---

## Testes (xUnit)

Os testes em `QueryLab.Tests` devem:
1. Usar `Testcontainers.PostgreSql` para subir um PostgreSQL isolado por test run
2. Provar que **os resultados** de ambas as abordagens são idênticos (mesmo Count, mesmos Ids)
3. Provar que as **métricas** são diferentes (mais registros trafegados no IEnumerable)

Exemplo de assertion:
```csharp
// Resultados iguais
Assert.Equal(resultQueryable.Data.Count, resultEnumerable.Data.Count);

// Performance diferente
Assert.True(resultEnumerable.Metrics.RegistrosTrafegados > resultQueryable.Metrics.RegistrosTrafegados);
Assert.True(resultEnumerable.Metrics.TempoMs > resultQueryable.Metrics.TempoMs);
```

---

## Como rodar

```bash
# Subir tudo
docker-compose up -d --build

# Seed (se for serviço separado)
docker-compose run --rm seed

# Testar um cenário
curl http://localhost:5000/cenario01/iqueryable | jq
curl http://localhost:5000/cenario01/ienumerable | jq

# Comparar lado a lado
curl -s http://localhost:5000/cenario01/iqueryable | jq '.metrics'
curl -s http://localhost:5000/cenario01/ienumerable | jq '.metrics'

# Rodar benchmarks (fora do container, com .NET SDK)
dotnet run -c Release --project src/QueryLab.Benchmarks

# Rodar testes
dotnet test
```

---

## Convenções

- Nomes de classes, métodos e propriedades em **inglês** (padrão .NET)
- Comentários explicativos em **português**
- Cada endpoint deve ter um comentário no topo explicando o que demonstra
- Usar `record` para DTOs e respostas
- Usar `sealed class` onde aplicável
- Nullable reference types habilitado (`<Nullable>enable</Nullable>`)
- Implicit usings habilitado
- Tratar warnings como errors em Release

---

## Ordem de implementação sugerida

1. Solution e projetos (csproj + sln)
2. Domain (entidades)
3. Infra (DbContext, Configurations, Migrations)
4. Docker Compose (PostgreSQL)
5. Seed de dados
6. Infrastructure de métricas (SqlInterceptor, QueryMetrics, middleware)
7. Cenários 1 a 6 (um de cada vez, testando)
8. Testes com Testcontainers
9. Benchmarks
10. README.md final com resultados exemplo