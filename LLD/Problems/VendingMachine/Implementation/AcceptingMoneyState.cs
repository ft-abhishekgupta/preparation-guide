namespace VendingMachineLLD;

public class AcceptingMoneyState : IVendingMachineState
{
    public void SelectProduct(VendingMachine machine, string productId)
    {
        throw new InvalidOperationException(
            "Cannot change product after inserting money.");
    }

    public void InsertMoney(VendingMachine machine, Money money)
    {
        machine.AddInsertedAmount(money.Denomination);
        machine.CashManager.AddMoney(money);
    }

    public void Dispense(VendingMachine machine)
    {
        var product = machine.SelectedProduct
            ?? throw new InvalidOperationException("No product selected.");

        if (machine.InsertedAmount < product.Price)
            throw new InvalidOperationException("Insufficient funds.");

        machine.SetState(new DispensingState());
        machine.Dispense();
    }

    public void Cancel(VendingMachine machine)
    {
        machine.ResetTransaction();
        machine.SetState(new IdleState());
    }
}
