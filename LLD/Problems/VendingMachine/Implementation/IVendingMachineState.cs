namespace VendingMachineLLD;

public interface IVendingMachineState
{
    void SelectProduct(VendingMachine machine, string productId);
    void InsertMoney(VendingMachine machine, Money money);
    void Dispense(VendingMachine machine);
    void Cancel(VendingMachine machine);
}
