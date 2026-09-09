namespace VendingMachineLLD;

public class VendingMachine
{
    private IVendingMachineState _state;
    private readonly object _transactionLock = new();

    private Product? _selectedProduct;
    private decimal _insertedAmount;

    public Inventory Inventory { get; }
    public CashManager CashManager { get; }

    public VendingMachine(
        Inventory inventory,
        CashManager cashManager)
    {
        Inventory = inventory;
        CashManager = cashManager;
        _state = new IdleState();
    }

    public void SelectProduct(string productId)
    {
        lock (_transactionLock)
        {
            _state.SelectProduct(this, productId);
        }
    }

    public void InsertMoney(Money money)
    {
        lock (_transactionLock)
        {
            _state.InsertMoney(this, money);
        }
    }

    public void Dispense()
    {
        lock (_transactionLock)
        {
            _state.Dispense(this);
        }
    }

    public void Cancel()
    {
        lock (_transactionLock)
        {
            _state.Cancel(this);
        }
    }

    public void Restock(Product product, int quantity)
    {
        Inventory.AddProduct(product, quantity);
    }

    public List<Money> CollectMoney()
    {
        lock (_transactionLock)
        {
            if (_insertedAmount != 0)
                throw new InvalidOperationException(
                    "Cannot collect money during an active transaction.");

            return CashManager.CollectAllMoney();
        }
    }

    internal void SetState(IVendingMachineState state)
    {
        _state = state;
    }

    internal Product? SelectedProduct => _selectedProduct;
    internal decimal InsertedAmount => _insertedAmount;

    internal void SetSelectedProduct(Product product)
    {
        _selectedProduct = product;
    }

    internal void AddInsertedAmount(decimal amount)
    {
        _insertedAmount += amount;
    }

    internal void ResetTransaction()
    {
        _selectedProduct = null;
        _insertedAmount = 0;
    }

    internal void DispenseCompleted(List<Money> change)
    {
        // Physical product/change dispensing would happen here.
        ResetTransaction();
        SetState(new IdleState());
    }

    internal List<Money> ReturnInsertedMoney()
    {
        // In a real machine, these would be physically returned.
        // For this LLD, return the equivalent denominations.
        var result = new List<Money>();

        if (_insertedAmount > 0)
        {
            result.Add(new Money(
                _insertedAmount,
                _insertedAmount < 20
                    ? MoneyType.Coin
                    : MoneyType.Note));
        }

        return result;
    }
}
