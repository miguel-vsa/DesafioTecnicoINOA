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

        Console.WriteLine($"Ativo: {ativo}");
    }
}