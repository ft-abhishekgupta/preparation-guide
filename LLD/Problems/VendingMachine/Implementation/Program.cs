using VendingMachineLLD;

var inventory = new Inventory();
inventory.AddProduct(new Product("A1", "Coke", 35m), 5);
inventory.AddProduct(new Product("A2", "Pepsi", 30m), 3);

var cashManager = new CashManager();

// Seed machine with change.
cashManager.AddMoney(new Money(10m, MoneyType.Coin));
cashManager.AddMoney(new Money(10m, MoneyType.Coin));
cashManager.AddMoney(new Money(5m, MoneyType.Coin));

var machine = new VendingMachine(inventory, cashManager);

machine.SelectProduct("A1");
machine.InsertMoney(new Money(50m, MoneyType.Note));
machine.Dispense();

Console.WriteLine("Product dispensed successfully.");
Console.WriteLine($"Remaining Coke quantity: {inventory.GetQuantity("A1")}");
