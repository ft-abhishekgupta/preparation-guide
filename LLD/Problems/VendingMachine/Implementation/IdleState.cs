namespace VendingMachineLLD;

public class IdleState : IVendingMachineState
{
    public void SelectProduct(VendingMachine machine, string productId)
    {
        var product = machine.Inventory.GetProduct(productId);

        if (!machine.Inventory.IsAvailable(productId))
            throw new InvalidOperationException("Product is out of stock.");

        machine.SetSelectedProduct(product);
        machine.SetState(new ProductSelectedState());
    }

    public void InsertMoney(VendingMachine machine, Money money)
    {
        throw new InvalidOperationException(
            "Select a product before inserting money.");
    }

    public void Dispense(VendingMachine machine)
    {
        throw new InvalidOperationException(
            "No product selected.");
    }

    public void Cancel(VendingMachine machine)
    {
        // Nothing to cancel.
    }
}
