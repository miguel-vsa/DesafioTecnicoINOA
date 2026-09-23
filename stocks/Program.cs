using System.Globalization;
class Program
{
    static void Main(string[] args)
    {
        if (args.Length != 3)
        {
            Console.WriteLine("Uso: stock-quote-alert <ativo> <preço-venda> <preço-compra>");
            return;
        }

        string ativo = args[0];
        decimal precoVenda;
        decimal precoCompra;

        if (!decimal.TryParse(args[1], NumberStyles.Any, CultureInfo.InvariantCulture, out precoVenda) ||
            !decimal.TryParse(args[2], NumberStyles.Any, CultureInfo.InvariantCulture, out precoCompra))
        {
            Console.WriteLine("Erro: Preço de venda e preço de compra devem ser números válidos.");
            return;
        }
        if (precoVenda <= 0 || precoCompra <= 0)
        {
            Console.WriteLine("Erro: Preço de venda e preço de compra devem ser maiores que zero.");
            return;
        }
        if (precoVenda <= precoCompra)
        {
            Console.WriteLine("Erro: Preço de venda deve ser maior que o preço de compra.");
            return;
        }
        Console.WriteLine($"Ativo: {ativo}");
        Console.WriteLine($"Preço de venda: {precoVenda}");
        Console.WriteLine($"Preço de compra: {precoCompra}");
    }
}