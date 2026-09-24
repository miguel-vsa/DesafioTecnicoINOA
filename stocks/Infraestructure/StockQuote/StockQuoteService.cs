using System.Text.Json;

class StockQuoteService : IStockQuoteService
{
    private readonly HttpClient _httpClient;

    public StockQuoteService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<StockQuote> GetStockQuoteAsync(string symbol)
    {
    string url = $"https://brapi.dev/api/v2/stocks/quote?symbols={symbol}";

    HttpResponseMessage response = await _httpClient.GetAsync(url);

    response.EnsureSuccessStatusCode();

    string json = await response.Content.ReadAsStringAsync();

    using JsonDocument document = JsonDocument.Parse(json);

    JsonElement quote = document
        .RootElement
        .GetProperty("results")[0]
        .GetProperty("data");

    decimal price = quote
        .GetProperty("regularMarketPrice")
        .GetDecimal();

    return new StockQuote(
        symbol,
        price,
        DateTime.Now
    );
    }
}