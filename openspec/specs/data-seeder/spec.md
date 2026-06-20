# data-seeder Specification

## Purpose

Defines the high-throughput, idempotent data seeding strategy for QueryLab using PostgreSQL BINARY COPY, including target record counts and realistic data distributions for meaningful query comparisons.

## Requirements

### Requirement: Bulk seed via PostgreSQL BINARY COPY
The `DataSeeder` SHALL insert all records using `NpgsqlBinaryImporter` (`BeginBinaryImportAsync`) to achieve throughput of 100k+ rows/second. It SHALL NOT use `DbContext.SaveChangesAsync()` in a loop for bulk data. Seed SHALL be idempotent: if the target table already has data, the seeder SHALL skip that table.

#### Scenario: Seeder inserts target record counts
- **WHEN** `DataSeeder.SeedAsync()` is called against an empty database
- **THEN** the database contains exactly 100 `Clientes`, 50 `Produtos`, 1,000,000 `Pedidos`, and approximately 3,000,000 `ItensPedido`

#### Scenario: Seeder is idempotent
- **WHEN** `DataSeeder.SeedAsync()` is called a second time
- **THEN** no duplicate records are inserted and counts remain unchanged

#### Scenario: Seeder shows progress on console
- **WHEN** `Pedidos` are being inserted
- **THEN** the console displays progress updates such as "Inserindo pedidos... 250.000/1.000.000" at regular intervals (every 250k rows at minimum)

### Requirement: Realistic seed data distribution
Seed data SHALL have realistic distributions:
- `Pedido.DataPedido`: random across the last 2 years
- `Pedido.ValorTotal`: varied, between R$10 and R$50,000
- `Pedido.Status`: distributed across at least 4 statuses (e.g., Pendente, Confirmado, Enviado, Entregue)
- `Produto.Preco`: between R$10 and R$5,000
- `Produto.CategoriaId`: between 1 and 10 (varied categories)

#### Scenario: Pedidos span the last 2 years
- **WHEN** seeding completes
- **THEN** `SELECT MIN(DataPedido), MAX(DataPedido) FROM Pedidos` shows a range within the last 2 years relative to seed execution date

#### Scenario: Multiple statuses are represented
- **WHEN** seeding completes
- **THEN** `SELECT DISTINCT Status FROM Pedidos` returns at least 4 distinct values
