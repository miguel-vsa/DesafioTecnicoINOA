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
        IStockQuoteService service = new StockQuoteService(httpClient);
        IEmailService emailService = new EmailService(emailConfiguration);
        AlertState state = AlertState.Normal;
        AlertStateEvaluator evaluator = new AlertStateEvaluator();

        using var cancellationTokenSource = new CancellationTokenSource();
        CancellationToken cancellationToken = cancellationTokenSource.Token;

        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            cancellationTokenSource.Cancel();
        };

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                StockQuote quote = await service.GetStockQuoteAsync(asset);

                Console.WriteLine($"Cotação atual: {quote.Price}");

                AlertState newState = evaluator.Evaluate(
                    quote.Price,
                    sellPrice,
                    buyPrice
                );

                if (newState != state)
                {
                    AlertState previousState = state;
                    state = newState;

                    switch (state)
                    {
                        case AlertState.Buy:
                            Console.WriteLine("Alerta: Cotação abaixo do preço de compra.");

                            await emailService.SendEmailAsync(
                                $"Alerta de compra - {asset}",
                                $"A cotação de {asset} está abaixo do preço de compra definido.\n\n" +
                                $"Cotação atual: R$ {quote.Price:F2}\n" +
                                $"Preço de compra: R$ {buyPrice:F2}\n\n" +
                                $"Recomendação: compra."
                            );

                            Console.WriteLine("E-mail de compra enviado.");
                            break;

                        case AlertState.Sell:
                            Console.WriteLine("Alerta: Cotação acima do preço de venda.");

                            await emailService.SendEmailAsync(
                                $"Alerta de venda - {asset}",
                                $"A cotação de {asset} está acima do preço de venda definido.\n\n" +
                                $"Cotação atual: R$ {quote.Price:F2}\n" +
                                $"Preço de venda: R$ {sellPrice:F2}\n\n" +
                                $"Recomendação: venda."
                            );

                            Console.WriteLine("E-mail de venda enviado.");
                            break;

                        case AlertState.Normal:
                            Console.WriteLine("Cotação voltou para a faixa normal.");

                            if (previousState == AlertState.Buy)
                            {
                                await emailService.SendEmailAsync(
                                    $"Oportunidade de compra encerrada - {asset}",
                                    $"A cotação de {asset} voltou para a faixa normal de monitoramento.\n\n" +
                                    $"Cotação atual: R$ {quote.Price:F2}\n" +
                                    $"Preço de compra: R$ {buyPrice:F2}\n\n" +
                                    $"A oportunidade de compra não está mais ativa."
                                );

                                Console.WriteLine("E-mail de encerramento da compra enviado.");
                            }
                            else if (previousState == AlertState.Sell)
                            {
                                await emailService.SendEmailAsync(
                                    $"Oportunidade de venda encerrada - {asset}",
                                    $"A cotação de {asset} voltou para a faixa normal de monitoramento.\n\n" +
                                    $"Cotação atual: R$ {quote.Price:F2}\n" +
                                    $"Preço de venda: R$ {sellPrice:F2}\n\n" +
                                    $"A oportunidade de venda não está mais ativa."
                                );

                                Console.WriteLine("E-mail de encerramento da venda enviado.");
                            }

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