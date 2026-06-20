## ADDED Requirements

### Requirement: Domain entities with EF Core Fluent API configuration
The system SHALL define `Cliente`, `Produto`, `Pedido` and `ItemPedido` entities in `QueryLab.Domain` with all relationships and navigation properties. Configuration SHALL use Fluent API exclusively (no Data Annotations). Indexed columns SHALL include `DataPedido`, `ValorTotal`, and `CategoriaId`.

#### Scenario: Entities compile without Data Annotations
- **WHEN** the `QueryLab.Domain` project is built
- **THEN** no `[Key]`, `[Required]`, `[Column]` or similar annotation attributes appear in any entity class

#### Scenario: Fluent API configures all relationships
- **WHEN** EF Core model is built
- **THEN** `Pedido` has a FK to `Cliente`, `ItemPedido` has FKs to `Pedido` and `Produto`, all configured via `IEntityTypeConfiguration<T>` classes

#### Scenario: Indexes exist on filter columns
- **WHEN** migrations are applied
- **THEN** the `Pedidos` table has indexes on `DataPedido` and `ValorTotal`, and `Produtos` table has an index on `CategoriaId`

### Requirement: Nullable reference types enabled
All entity properties SHALL correctly annotate nullability. Reference types that can be null SHALL use `string?` / `T?` syntax. The project SHALL compile with `<Nullable>enable</Nullable>` with zero warnings.

#### Scenario: Nullable annotations compile cleanly
- **WHEN** `QueryLab.Domain` is built in Release configuration
- **THEN** build succeeds with zero nullable-related warnings or errors
