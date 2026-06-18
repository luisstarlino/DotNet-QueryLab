namespace QueryLab.Domain.Entities;

public class Pedido
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public DateTime DataPedido { get; set; }
    public decimal ValorTotal { get; set; }
    public string Status { get; set; } = string.Empty;
    public Cliente Cliente { get; set; } = null!;
    public ICollection<ItemPedido> Items { get; set; } = [];
}
