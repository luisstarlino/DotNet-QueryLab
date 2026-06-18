namespace QueryLab.Domain.Entities;

public class ItemPedido
{
    public Guid Id { get; set; }
    public Guid PedidoId { get; set; }
    public Guid ProdutoId { get; set; }
    public int Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
    public Pedido Pedido { get; set; } = null!;
    public Produto Produto { get; set; } = null!;
}
