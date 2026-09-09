namespace VendingMachineLLD;

public class Product
{
    public string Id { get; }
    public string Name { get; }
    public decimal Price { get; }

    public Product(string id, string name, decimal price)
    {
        Id = id;
        Name = name;
        Price = price;
    }
}
