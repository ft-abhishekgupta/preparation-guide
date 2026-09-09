using System.Collections.Concurrent;

namespace VendingMachineLLD;

public class Inventory
{
    private readonly ConcurrentDictionary<string, Product> _products = new();
    private readonly ConcurrentDictionary<string, int> _quantities = new();

    public void AddProduct(Product product, int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive.");

        _products.TryAdd(product.Id, product);
        _quantities.AddOrUpdate(
            product.Id,
            quantity,
            (_, existing) => existing + quantity);
    }

    public Product GetProduct(string productId)
    {
        if (!_products.TryGetValue(productId, out var product))
            throw new InvalidOperationException("Product not found.");

        return product;
    }

    public int GetQuantity(string productId)
    {
        return _quantities.TryGetValue(productId, out var quantity)
            ? quantity
            : 0;
    }

    public bool IsAvailable(string productId)
    {
        return GetQuantity(productId) > 0;
    }

    public bool TryRemoveOne(string productId)
    {
        while (true)
        {
            if (!_quantities.TryGetValue(productId, out var current) || current <= 0)
                return false;

            if (_quantities.TryUpdate(productId, current - 1, current))
                return true;
        }
    }
}
