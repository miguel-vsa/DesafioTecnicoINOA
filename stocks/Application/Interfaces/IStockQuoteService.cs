interface IStockQuoteService
{
    Task<StockQuote> GetStockQuoteAsync(string symbol);
}