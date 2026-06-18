using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QueryLab.Domain.Entities;

namespace QueryLab.Infra.Configurations;

public sealed class ItemPedidoConfiguration : IEntityTypeConfiguration<ItemPedido>
{
    public void Configure(EntityTypeBuilder<ItemPedido> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.PrecoUnitario).HasPrecision(18, 2);
    }
}
