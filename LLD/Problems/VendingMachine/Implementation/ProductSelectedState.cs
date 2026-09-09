namespace VendingMachineLLD;

public class ProductSelectedState : IVendingMachineState
{
    public void SelectProduct(VendingMachine machine, string productId)
    {
        var product = machine.Inventory.GetProduct(productId);

        if (!machine.Inventory.IsAvailable(productId))
            throw new InvalidOperationException("Product is out of stock.");

        machine.SetSelectedProduct(product);
    }

    public void InsertMoney(VendingMachine machine, Money money)
    {
        machine.AddInsertedAmount(money.Denomination);
        machine.CashManager.AddMoney(money);
        machine.SetState(new AcceptingMoneyState());
    }

    public void Dispense(VendingMachine machine)
    {
        throw new InvalidOperationException(
            "Insufficient funds. Insert money first.");
    }

    public void Cancel(VendingMachine machine)
    {
        machine.ResetTransaction();
        machine.SetState(new IdleState());
    }
}
