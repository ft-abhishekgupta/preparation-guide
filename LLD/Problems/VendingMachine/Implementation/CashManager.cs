namespace VendingMachineLLD;

public class CashManager
{
    private readonly Dictionary<decimal, int> _cash = new();
    private readonly object _lock = new();

    public void AddMoney(Money money)
    {
        lock (_lock)
        {
            if (_cash.ContainsKey(money.Denomination))
                _cash[money.Denomination]++;
            else
                _cash[money.Denomination] = 1;
        }
    }

    public List<Money> CalculateChange(decimal amount)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative.");

        lock (_lock)
        {
            var available = _cash
                .OrderByDescending(x => x.Key)
                .ToDictionary(x => x.Key, x => x.Value);

            var result = new List<Money>();
            var remaining = amount;

            foreach (var entry in available)
            {
                var denomination = entry.Key;
                var count = Math.Min(
                    entry.Value,
                    (int)(remaining / denomination));

                for (var i = 0; i < count; i++)
                {
                    result.Add(new Money(
                        denomination,
                        denomination < 20
                            ? MoneyType.Coin
                            : MoneyType.Note));

                    remaining -= denomination;
                    remaining = Math.Round(
                        remaining, 2, MidpointRounding.AwayFromZero);
                }
            }

            if (remaining != 0)
                throw new InvalidOperationException(
                    "Exact change cannot be provided.");

            return result;
        }
    }

    public void RemoveChange(IEnumerable<Money> change)
    {
        lock (_lock)
        {
            foreach (var money in change)
            {
                if (!_cash.TryGetValue(money.Denomination, out var count) ||
                    count <= 0)
                {
                    throw new InvalidOperationException(
                        "Insufficient cash for change.");
                }

                if (count == 1)
                    _cash.Remove(money.Denomination);
                else
                    _cash[money.Denomination] = count - 1;
            }
        }
    }

    public decimal GetTotalCash()
    {
        lock (_lock)
        {
            return _cash.Sum(x => x.Key * x.Value);
        }
    }

    public List<Money> CollectAllMoney()
    {
        lock (_lock)
        {
            var result = new List<Money>();

            foreach (var entry in _cash)
            {
                for (var i = 0; i < entry.Value; i++)
                {
                    result.Add(new Money(
                        entry.Key,
                        entry.Key < 20
                            ? MoneyType.Coin
                            : MoneyType.Note));
                }
            }

            _cash.Clear();
            return result;
        }
    }
}
