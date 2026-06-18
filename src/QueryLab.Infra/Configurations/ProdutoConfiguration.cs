using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QueryLab.Domain.Entities;

namespace QueryLab.Infra.Configurations;

public sealed class ProdutoConfiguration : IEntityTypeConfiguration<Produto>
{
    public void Configure(EntityTypeBuilder<Produto> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Nome).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Preco).HasPrecision(18, 2);

        builder.HasIndex(p => p.CategoriaId);

        builder.HasMany(p => p.Itens)
               .WithOne(i => i.Produto)
               .HasForeignKey(i => i.ProdutoId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
