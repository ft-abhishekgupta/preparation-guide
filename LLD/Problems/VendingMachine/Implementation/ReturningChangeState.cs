namespace VendingMachineLLD;

public class ReturningChangeState : IVendingMachineState
{
    public void SelectProduct(VendingMachine machine, string productId)
        => throw new InvalidOperationException("Complete current transaction first.");

    public void InsertMoney(VendingMachine machine, Money money)
        => throw new InvalidOperationException("Complete current transaction first.");

    public void Cancel(VendingMachine machine)
        => throw new InvalidOperationException("Change is being returned.");

    public void Dispense(VendingMachine machine)
    {
        // Change has already been calculated and removed.
        // Physical product/change dispensing would happen here.
        machine.ResetTransaction();
        machine.SetState(new IdleState());
    }
}
