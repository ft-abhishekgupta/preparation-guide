namespace VendingMachineLLD;

public enum MoneyType
{
    Coin,
    Note
}

public class Money
{
    public decimal Denomination { get; }
    public MoneyType Type { get; }

    public Money(decimal denomination, MoneyType type)
    {
        if (denomination <= 0)
            throw new ArgumentException("Denomination must be positive.");

        Denomination = denomination;
        Type = type;
    }
}
