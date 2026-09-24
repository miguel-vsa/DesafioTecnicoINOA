class StockMonitoringService
{
    private readonly IStockQuoteService _stockQuoteService;
    private readonly IEmailService _emailService;
    private readonly AlertStateEvaluator _evaluator;

    private readonly string _asset;
    private readonly decimal _sellPrice;
    private readonly decimal _buyPrice;

    private AlertState _state = AlertState.Normal;

    public StockMonitoringService(
        IStockQuoteService stockQuoteService,
        IEmailService emailService,
        AlertStateEvaluator evaluator,
        string asset,
        decimal sellPrice,
        decimal buyPrice)
    {
        _stockQuoteService = stockQuoteService;
        _emailService = emailService;
        _evaluator = evaluator;
        _asset = asset;
        _sellPrice = sellPrice;
        _buyPrice = buyPrice;
    }

    public async Task MonitorAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            StockQuote quote =
                await _stockQuoteService.GetStockQuoteAsync(_asset);

            Console.WriteLine($"Cotação atual: {quote.Price}");

            AlertState newState = _evaluator.Evaluate(
                quote.Price,
                _sellPrice,
                _buyPrice
            );

            if (newState != _state)
            {
                AlertState previousState = _state;
                _state = newState;

                switch (_state)
                {
                    case AlertState.Buy:
                        await SendBuyAlertAsync(quote);
                        break;

                    case AlertState.Sell:
                        await SendSellAlertAsync(quote);
                        break;

                    case AlertState.Normal:
                        await SendNormalAlertAsync(
                            quote,
                            previousState
                        );
                        break;
                }
            }

            await Task.Delay(15000, cancellationToken);
        }
    }

    private async Task SendBuyAlertAsync(StockQuote quote)
    {
        Console.WriteLine(
            "Alerta: Cotação abaixo do preço de compra."
        );

        await _emailService.SendEmailAsync(
            $"Alerta de compra - {_asset}",
            $"A cotação de {_asset} está abaixo do preço de compra definido.\n\n" +
            $"Cotação atual: R$ {quote.Price:F2}\n" +
            $"Preço de compra: R$ {_buyPrice:F2}\n\n" +
            $"Recomendação: compra."
        );

        Console.WriteLine("E-mail de compra enviado.");
    }

    private async Task SendSellAlertAsync(StockQuote quote)
    {
        Console.WriteLine(
            "Alerta: Cotação acima do preço de venda."
        );

        await _emailService.SendEmailAsync(
            $"Alerta de venda - {_asset}",
            $"A cotação de {_asset} está acima do preço de venda definido.\n\n" +
            $"Cotação atual: R$ {quote.Price:F2}\n" +
            $"Preço de venda: R$ {_sellPrice:F2}\n\n" +
            $"Recomendação: venda."
        );

        Console.WriteLine("E-mail de venda enviado.");
    }

    private async Task SendNormalAlertAsync(
        StockQuote quote,
        AlertState previousState)
    {
        Console.WriteLine(
            "Cotação voltou para a faixa normal."
        );

        if (previousState == AlertState.Buy)
        {
            await _emailService.SendEmailAsync(
                $"Oportunidade de compra encerrada - {_asset}",
                $"A cotação de {_asset} voltou para a faixa normal de monitoramento.\n\n" +
                $"Cotação atual: R$ {quote.Price:F2}\n" +
                $"Preço de compra: R$ {_buyPrice:F2}\n\n" +
                $"A oportunidade de compra não está mais ativa."
            );

            Console.WriteLine(
                "E-mail de encerramento da compra enviado."
            );
        }
        else if (previousState == AlertState.Sell)
        {
            await _emailService.SendEmailAsync(
                $"Oportunidade de venda encerrada - {_asset}",
                $"A cotação de {_asset} voltou para a faixa normal de monitoramento.\n\n" +
                $"Cotação atual: R$ {quote.Price:F2}\n" +
                $"Preço de venda: R$ {_sellPrice:F2}\n\n" +
                $"A oportunidade de venda não está mais ativa."
            );

            Console.WriteLine(
                "E-mail de encerramento da venda enviado."
            );
        }
    }
}