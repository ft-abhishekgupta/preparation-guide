namespace VendingMachineLLD;

public class DispensingState : IVendingMachineState
{
    public void SelectProduct(VendingMachine machine, string productId)
        => throw new InvalidOperationException("Currently dispensing.");

    public void InsertMoney(VendingMachine machine, Money money)
        => throw new InvalidOperationException("Currently dispensing.");

    public void Cancel(VendingMachine machine)
        => throw new InvalidOperationException("Currently dispensing.");

    public void Dispense(VendingMachine machine)
    {
        var product = machine.SelectedProduct
            ?? throw new InvalidOperationException("No product selected.");

        var changeAmount = machine.InsertedAmount - product.Price;

        // Calculate change before consuming the product.
        var change = machine.CashManager.CalculateChange(changeAmount);

        // Remove product atomically from inventory.
        if (!machine.Inventory.TryRemoveOne(product.Id))
            throw new InvalidOperationException("Product is out of stock.");

        // Remove change from machine cash.
        machine.CashManager.RemoveChange(change);

        machine.SetState(new ReturningChangeState());
        machine.DispenseCompleted(change);
    }
}
