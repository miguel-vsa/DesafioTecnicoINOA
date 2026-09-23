using System.Globalization;
class Program
{
    static async Task Main(string[] args)
    {
        if (args.Length != 3)
        {
            Console.WriteLine("Uso: stock-quote-alert <ativo> <preço-venda> <preço-compra>");
            return;
        }

        string asset = args[0];
        decimal sellPrice;
        decimal buyPrice;

        if (!decimal.TryParse(args[1], NumberStyles.Any, CultureInfo.InvariantCulture, out sellPrice) ||
            !decimal.TryParse(args[2], NumberStyles.Any, CultureInfo.InvariantCulture, out buyPrice))
        {
            Console.WriteLine("Erro: Preço de venda e preço de compra devem ser números válidos.");
            return;
        }
        if (sellPrice <= 0 || buyPrice <= 0)
        {
            Console.WriteLine("Erro: Preço de venda e preço de compra devem ser maiores que zero.");
            return;
        }
        if (sellPrice <= buyPrice)
        {
            Console.WriteLine("Erro: Preço de venda deve ser maior que o preço de compra.");
            return;
        }
        Console.WriteLine($"Ativo: {asset}");
        Console.WriteLine($"Preço de venda: {sellPrice}");
        Console.WriteLine($"Preço de compra: {buyPrice}");

        HttpClient httpClient = new HttpClient();

        IStockQuoteService service =
            new StockQuoteService(httpClient);
        StockQuote quote =
            await service.GetStockQuoteAsync(asset);
        Console.WriteLine($"Cotação atual: {quote.Price}");

        if (quote.Price > sellPrice)
        {
            Console.WriteLine("Alerta: Cotação acima do preço de venda.");
        }
        else if (quote.Price < buyPrice)
        {
            Console.WriteLine("Alerta: Cotação abaixo do preço de compra.");
        }
        else
        {
            Console.WriteLine("Preço dentro da faixa de monitoramento.");
        }
    }
}