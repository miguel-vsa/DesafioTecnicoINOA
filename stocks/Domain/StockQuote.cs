class StockQuote
{
    public string Symbol { get; set; }
    public decimal Price { get; set; }
    public DateTime Timestamp { get; set; }

    public StockQuote(string symbol, decimal price, DateTime timestamp)
    {
        Symbol = symbol;
        Price = price;
        Timestamp = timestamp;
    }
}