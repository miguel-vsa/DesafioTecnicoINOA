class AlertStateEvaluator
{
    public AlertState Evaluate(
        decimal price,
        decimal sellPrice,
        decimal buyPrice)
    {
        if (price > sellPrice)
        {
            return AlertState.Sell;
        }

        if (price < buyPrice)
        {
            return AlertState.Buy;
        }

        return AlertState.Normal;
    }
}