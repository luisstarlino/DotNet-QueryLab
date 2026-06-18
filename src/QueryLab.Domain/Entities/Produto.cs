namespace QueryLab.Domain.Entities;

public class Produto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public int CategoriaId { get; set; }
    public ICollection<ItemPedido> Itens { get; set; } = [];
}
