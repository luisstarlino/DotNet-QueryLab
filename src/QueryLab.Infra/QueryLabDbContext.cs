using Microsoft.EntityFrameworkCore;
using QueryLab.Domain.Entities;

namespace QueryLab.Infra;

public class QueryLabDbContext : DbContext
{
    public QueryLabDbContext(DbContextOptions<QueryLabDbContext> options) : base(options) { }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<ItemPedido> ItensPedido => Set<ItemPedido>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(QueryLabDbContext).Assembly);
    }
}
