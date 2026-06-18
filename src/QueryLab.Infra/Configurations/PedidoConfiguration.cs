using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QueryLab.Domain.Entities;

namespace QueryLab.Infra.Configurations;

public sealed class PedidoConfiguration : IEntityTypeConfiguration<Pedido>
{
    public void Configure(EntityTypeBuilder<Pedido> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.ValorTotal).HasPrecision(18, 2);
        builder.Property(p => p.Status).IsRequired().HasMaxLength(50);

        builder.HasIndex(p => p.DataPedido);
        builder.HasIndex(p => p.ValorTotal);

        builder.HasMany(p => p.Items)
               .WithOne(i => i.Pedido)
               .HasForeignKey(i => i.PedidoId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
