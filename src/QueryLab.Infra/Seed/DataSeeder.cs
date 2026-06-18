using Npgsql;
using NpgsqlTypes;

namespace QueryLab.Infra.Seed;

public static class DataSeeder
{
    private static readonly string[] Statuses = ["Pendente", "Aprovado", "Enviado", "Entregue", "Cancelado"];
    private static readonly string[] Categorias = ["Eletrônicos", "Roupas", "Alimentos", "Livros", "Casa"];

    public static async Task RunAsync(string connectionString)
    {
        Console.WriteLine("Iniciando seed do banco de dados...");

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();

        if (await IsAlreadySeededAsync(conn))
        {
            Console.WriteLine("Banco já populado. Nenhuma ação necessária.");
            return;
        }

        var clienteIds = await SeedClientesAsync(conn);
        var produtoIds = await SeedProdutosAsync(conn);
        var pedidoIds = await SeedPedidosAsync(conn, clienteIds);
        await SeedItensPedidoAsync(conn, pedidoIds, produtoIds);

        Console.WriteLine("Seed concluído com sucesso.");
    }

    private static async Task<bool> IsAlreadySeededAsync(NpgsqlConnection conn)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM \"Pedidos\"";
        var count = (long)(await cmd.ExecuteScalarAsync() ?? 0L);
        return count > 0;
    }

    private static async Task<Guid[]> SeedClientesAsync(NpgsqlConnection conn)
    {
        Console.WriteLine("Inserindo clientes... 100/100");
        const int total = 100;
        var ids = new Guid[total];
        var random = new Random(42);
        var now = DateTime.UtcNow;

        await using var writer = await conn.BeginBinaryImportAsync(
            "COPY \"Clientes\" (\"Id\", \"Nome\", \"Email\", \"DataCadastro\") FROM STDIN (FORMAT BINARY)");

        for (int i = 0; i < total; i++)
        {
            ids[i] = Guid.NewGuid();
            await writer.StartRowAsync();
            await writer.WriteAsync(ids[i], NpgsqlDbType.Uuid);
            await writer.WriteAsync($"Cliente {i + 1}", NpgsqlDbType.Text);
            await writer.WriteAsync($"cliente{i + 1}@querylab.dev", NpgsqlDbType.Text);
            await writer.WriteAsync(now.AddDays(-random.Next(0, 730)), NpgsqlDbType.TimestampTz);
        }

        await writer.CompleteAsync();
        Console.WriteLine($"Clientes inseridos: {total}");
        return ids;
    }

    private static async Task<Guid[]> SeedProdutosAsync(NpgsqlConnection conn)
    {
        Console.WriteLine("Inserindo produtos... 50/50");
        const int total = 50;
        var ids = new Guid[total];
        var random = new Random(43);

        await using var writer = await conn.BeginBinaryImportAsync(
            "COPY \"Produtos\" (\"Id\", \"Nome\", \"Preco\", \"CategoriaId\") FROM STDIN (FORMAT BINARY)");

        for (int i = 0; i < total; i++)
        {
            ids[i] = Guid.NewGuid();
            var preco = Math.Round(10m + (decimal)random.NextDouble() * 4990m, 2);
            var categoriaId = random.Next(1, 11);

            await writer.StartRowAsync();
            await writer.WriteAsync(ids[i], NpgsqlDbType.Uuid);
            await writer.WriteAsync($"Produto {i + 1} - {Categorias[categoriaId % Categorias.Length]}", NpgsqlDbType.Text);
            await writer.WriteAsync(preco, NpgsqlDbType.Numeric);
            await writer.WriteAsync(categoriaId, NpgsqlDbType.Integer);
        }

        await writer.CompleteAsync();
        Console.WriteLine($"Produtos inseridos: {total}");
        return ids;
    }

    private static async Task<Guid[]> SeedPedidosAsync(NpgsqlConnection conn, Guid[] clienteIds)
    {
        const int total = 1_000_000;
        const int batchSize = 50_000;
        var ids = new Guid[total];
        var random = new Random(44);
        var now = DateTime.UtcNow;
        var inserted = 0;

        Console.Write("Inserindo pedidos...");

        for (int batch = 0; batch < total; batch += batchSize)
        {
            var batchCount = Math.Min(batchSize, total - batch);

            await using var writer = await conn.BeginBinaryImportAsync(
                "COPY \"Pedidos\" (\"Id\", \"ClienteId\", \"DataPedido\", \"ValorTotal\", \"Status\") FROM STDIN (FORMAT BINARY)");

            for (int i = 0; i < batchCount; i++)
            {
                var idx = batch + i;
                ids[idx] = Guid.NewGuid();
                var clienteId = clienteIds[random.Next(clienteIds.Length)];
                var dataPedido = now.AddDays(-random.Next(0, 730));
                var valorTotal = Math.Round(10m + (decimal)random.NextDouble() * 9990m, 2);
                var status = Statuses[random.Next(Statuses.Length)];

                await writer.StartRowAsync();
                await writer.WriteAsync(ids[idx], NpgsqlDbType.Uuid);
                await writer.WriteAsync(clienteId, NpgsqlDbType.Uuid);
                await writer.WriteAsync(dataPedido, NpgsqlDbType.TimestampTz);
                await writer.WriteAsync(valorTotal, NpgsqlDbType.Numeric);
                await writer.WriteAsync(status, NpgsqlDbType.Text);
            }

            await writer.CompleteAsync();
            inserted += batchCount;

            if (inserted % 250_000 == 0 || inserted == total)
                Console.WriteLine($" {inserted:N0}/{total:N0}");
        }

        return ids;
    }

    private static async Task SeedItensPedidoAsync(NpgsqlConnection conn, Guid[] pedidoIds, Guid[] produtoIds)
    {
        const int avgItensPerPedido = 3;
        const int batchSize = 50_000;
        var random = new Random(45);
        var inserted = 0;

        Console.Write("Inserindo itens de pedido...");

        var batch = new List<(Guid id, Guid pedidoId, Guid produtoId, int qtd, decimal preco)>(batchSize);

        foreach (var pedidoId in pedidoIds)
        {
            var numItens = random.Next(1, avgItensPerPedido * 2);
            for (int i = 0; i < numItens; i++)
            {
                batch.Add((
                    Guid.NewGuid(),
                    pedidoId,
                    produtoIds[random.Next(produtoIds.Length)],
                    random.Next(1, 11),
                    Math.Round(10m + (decimal)random.NextDouble() * 990m, 2)
                ));

                if (batch.Count == batchSize)
                {
                    await WriteBatchAsync(conn, batch);
                    inserted += batch.Count;
                    batch.Clear();

                    if (inserted % 500_000 == 0)
                        Console.WriteLine($" {inserted:N0}");
                }
            }
        }

        if (batch.Count > 0)
        {
            await WriteBatchAsync(conn, batch);
            inserted += batch.Count;
        }

        Console.WriteLine($" {inserted:N0} (total)");
    }

    private static async Task WriteBatchAsync(
        NpgsqlConnection conn,
        List<(Guid id, Guid pedidoId, Guid produtoId, int qtd, decimal preco)> items)
    {
        await using var writer = await conn.BeginBinaryImportAsync(
            "COPY \"ItensPedido\" (\"Id\", \"PedidoId\", \"ProdutoId\", \"Quantidade\", \"PrecoUnitario\") FROM STDIN (FORMAT BINARY)");

        foreach (var (id, pedidoId, produtoId, qtd, preco) in items)
        {
            await writer.StartRowAsync();
            await writer.WriteAsync(id, NpgsqlDbType.Uuid);
            await writer.WriteAsync(pedidoId, NpgsqlDbType.Uuid);
            await writer.WriteAsync(produtoId, NpgsqlDbType.Uuid);
            await writer.WriteAsync(qtd, NpgsqlDbType.Integer);
            await writer.WriteAsync(preco, NpgsqlDbType.Numeric);
        }

        await writer.CompleteAsync();
    }
}
