using System.Globalization;
using Microsoft.Extensions.Configuration;
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

        IStockQuoteService service =
            new StockQuoteService(httpClient);

        IEmailService emailService =
            new EmailService(emailConfiguration);

        AlertStateEvaluator evaluator =
            new AlertStateEvaluator();

        using var cancellationTokenSource =
            new CancellationTokenSource();

        CancellationToken cancellationToken =
            cancellationTokenSource.Token;

        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            cancellationTokenSource.Cancel();
        };

        StockMonitoringService monitoringService =
            new StockMonitoringService(
                service,
                emailService,
                evaluator,
                asset,
                sellPrice,
                buyPrice
            );

        try
        {
            await monitoringService.MonitorAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Monitoramento encerrado.");
        }
    }
}