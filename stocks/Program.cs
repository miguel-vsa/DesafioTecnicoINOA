using System.Globalization;
using Microsoft.Extensions.Configuration;
class Program
{
    static async Task Main(string[] args)
    {

        string asset = args[0];
        decimal sellPrice;
        decimal buyPrice;

        if (args.Length != 3)
        {
            Console.WriteLine("Uso: stock-quote-alert <ativo> <preço-venda> <preço-compra>");
            return;
        }

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

        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        EmailConfiguration? emailConfiguration =
            configuration
                .GetSection("Email")
                .Get<EmailConfiguration>();

        if (emailConfiguration == null || !emailConfiguration.IsValid())
        {
            Console.WriteLine("Erro: configuração de e-mail inválida.");
            return;
        }

        HttpClient httpClient = new HttpClient();

        IStockQuoteService service = new StockQuoteService(httpClient);
    
        AlertState state = AlertState.Normal;

        using var cancellationTokenSource = new CancellationTokenSource();
        CancellationToken cancellationToken = cancellationTokenSource.Token;

        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            cancellationTokenSource.Cancel();
        };

        try
        {
            while (!cancellationToken.IsCancellationRequested){
                StockQuote quote = await service.GetStockQuoteAsync(asset);
                Console.WriteLine($"Cotação atual: {quote.Price}");
                AlertState newState;

                if (quote.Price > sellPrice)
                {
                    newState = AlertState.Sell;
                }
                else if (quote.Price < buyPrice)
                {
                    newState = AlertState.Buy;
                }
                else
                {
                    newState = AlertState.Normal;
                }

                if (newState != state)
                {
                    state = newState;
                    switch (state)
                    {
                        case AlertState.Buy:
                            Console.WriteLine("Alerta: Cotação abaixo do preço de compra.");
                            break;
                        case AlertState.Sell:
                            Console.WriteLine("Alerta: Cotação acima do preço de venda.");
                            break;
                        case AlertState.Normal:
                            Console.WriteLine("Cotação voltou para a faixa normal.");
                            break;
                    }
                }
                await Task.Delay(15000, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Monitoramento encerrado."); 
        }
    }
}