# E-Commerce Cart

A shopping cart that holds items, applies stacked discounts and taxes through a pricing
pipeline, validates stock, and converts to an order at checkout.

## Prompt

```
"Design a shopping cart for an e-commerce site. Users add and remove products, the cart
shows a live total, promotions are applied, and checkout creates an order.

Promotions look like this:

{
  "code": "SAVE20",
  "type": "PERCENTAGE",
  "value": 20,
  "appliesTo": "CATEGORY:ELECTRONICS",
  "maxDiscount": 2000
}

Multiple promotions can apply to one cart. Focus on the pricing pipeline and checkout."
```

## Questions

```
"Guest carts or only logged-in users?"
> Both, cart keyed by cartId

"Can several promotions stack?"
> Yes, applied in a defined order

"Item-level or cart-level discounts?"
> Both - percent off a category, and flat off the cart

"When do we check inventory?"
> Soft check on add, hard reserve at checkout

"What if the price changes after an item is in the cart?"
> Show the current price; snapshot the price only at checkout

"Do we compute tax and shipping?"
> Yes, both after discounts

"Is payment in scope?"
> Just an interface, no gateway

"Concurrent modification of the same cart?"
> Assume single user session, mention it in extensibility
```

## Requirements

```
Requirements:
1. add(productId, qty), remove(productId), updateQuantity(productId, qty), clear()
2. Adding an existing product increases quantity
3. Cart rejects quantities beyond available stock
4. Pricing pipeline, strictly in this order:
   subtotal -> item discounts -> cart discounts -> shipping -> tax -> total
5. Multiple promotions stack; each can be capped by maxDiscount
6. Checkout: validate stock -> reserve -> snapshot prices -> create order -> clear cart
7. Checkout fails atomically if any item is out of stock (nothing is reserved)
8. Order stores the full price breakdown for auditing

Out of scope:
- Payment gateway integration
- Saved carts, wishlists, recommendations
- Multi-currency, multi-warehouse
- Persistence
```

## Core Entities

```
Cart              : Aggregate root, holds items
CartItem          : productId + quantity + unit price snapshot
Product           : Catalog data (price, category)
InventoryService  : Stock check and reservation
PricingEngine     : Runs the pricing pipeline
Discount          : Strategy - percentage, flat, BOGO
TaxCalculator     : Strategy - region-specific
ShippingCalculator: Strategy - flat, weight-based, free-over-X
PriceBreakdown    : Result object (auditable)
CheckoutService   : Orchestrator - cart to order
Order             : Immutable snapshot of a completed purchase
```

**Why these patterns**

| Concern | Pattern | Reason |
|---|---|---|
| Many discount types | Strategy | New promo = new class |
| Stacking discounts | Chain / pipeline | Order matters and must be explicit |
| Tax and shipping | Strategy | Vary by region and weight |
| Cart to order | Facade (CheckoutService) | Keeps `Cart` free of workflow logic |
| Price breakdown | Value object | Auditable, testable, printable on invoice |

## Class Design

```
class Cart:
    - id: string
    - userId: string
    - items: Map<productId, CartItem>

    + addItem(product, quantity) -> void
    + removeItem(productId) -> void
    + updateQuantity(productId, quantity) -> void
    + getItems() -> List<CartItem>
    + clear() -> void
    + isEmpty() -> boolean

class CartItem:
    - productId: string
    - name: string
    - unitPrice: double
    - quantity: int
    - category: string

    + lineTotal() -> double            // unitPrice * quantity

class PricingEngine:
    - itemDiscounts: List<Discount>
    - cartDiscounts: List<Discount>
    - shipping: ShippingCalculator
    - tax: TaxCalculator

    + price(cart, promoCodes) -> PriceBreakdown

interface Discount:
    + isApplicable(cart, item) -> boolean
    + calculate(baseAmount) -> double    // returns the discount amount
    + code() -> string

class PercentageDiscount implements Discount:
    - percent: double
    - maxDiscount: double
    - categoryFilter: string | null

class FlatDiscount implements Discount:
    - amount: double
    - minCartValue: double

class BuyXGetYDiscount implements Discount:
    - buyQty, freeQty: int

interface ShippingCalculator:
    + calculate(cart, amountAfterDiscount) -> double

interface TaxCalculator:
    + calculate(taxableAmount, cart) -> double

class PriceBreakdown:
    - subtotal: double
    - itemDiscount: double
    - cartDiscount: double
    - shipping: double
    - tax: double
    - total: double
    - appliedCodes: List<string>

class CheckoutService:
    - inventory: InventoryService
    - pricing: PricingEngine
    - payment: PaymentProcessor

    + checkout(cart, promoCodes, paymentMethod) -> Order

class Order:
    - id, userId: string
    - lines: List<CartItem>       // frozen copies
    - breakdown: PriceBreakdown
    - status: OrderStatus
```

**Pricing pipeline**

```
  items
    |  sum(unitPrice * qty)
    v
 SUBTOTAL ---------------------------------- 1000
    |  item discounts (per line, category filters)
    v
  -200  ------------------------------------  800
    |  cart discounts (flat / coupon, on the discounted base)
    v
  -50   ------------------------------------  750
    |  shipping (free over threshold)
    v
  +0    ------------------------------------  750
    |  tax on (discounted goods + shipping)
    v
  +135  ------------------------------------  885 = TOTAL
```

Discount-before-tax is the important rule: taxing the pre-discount amount overcharges the
customer and is a common bug in cart implementations.

## Implementation

Cart mutations

```
addItem(product, quantity)
    if quantity <= 0
        throw new InvalidQuantityException()

    existing = items.get(product.id)
    newQty = (existing != null ? existing.quantity : 0) + quantity

    if !inventory.isAvailable(product.id, newQty)
        throw new InsufficientStockException(product.id)

    if existing != null
        existing.quantity = newQty
    else
        items[product.id] = new CartItem(product, quantity)
```

PricingEngine.price

```
price(cart, promoCodes)
    subtotal = 0
    itemDiscountTotal = 0

    for item in cart.items
        lineTotal = item.lineTotal()
        subtotal += lineTotal

        for discount in itemDiscounts
            if discount.isApplicable(cart, item) && promoCodes.contains(discount.code())
                d = discount.calculate(lineTotal)
                itemDiscountTotal += d
                appliedCodes.add(discount.code())

    afterItem = subtotal - itemDiscountTotal

    cartDiscountTotal = 0
    for discount in cartDiscounts
        if discount.isApplicable(cart, null) && promoCodes.contains(discount.code())
            d = discount.calculate(afterItem)
            cartDiscountTotal += d
            appliedCodes.add(discount.code())

    afterDiscount = max(0, afterItem - cartDiscountTotal)
    shippingCost  = shipping.calculate(cart, afterDiscount)
    taxAmount     = tax.calculate(afterDiscount + shippingCost, cart)
    total         = afterDiscount + shippingCost + taxAmount

    return new PriceBreakdown(subtotal, itemDiscountTotal, cartDiscountTotal,
                              shippingCost, taxAmount, total, appliedCodes)
```

Discounts

```
PercentageDiscount.isApplicable(cart, item)
    return categoryFilter == null || item.category == categoryFilter

PercentageDiscount.calculate(base)
    return min(base * percent / 100, maxDiscount)

FlatDiscount.isApplicable(cart, item)
    return cart.subtotal() >= minCartValue

FlatDiscount.calculate(base)
    return min(amount, base)          // never discount below zero
```

CheckoutService.checkout - the only place needing transactional care

```
checkout(cart, promoCodes, paymentMethod)
    if cart.isEmpty()
        throw new EmptyCartException()

    // 1. validate everything BEFORE reserving anything
    for item in cart.items
        if !inventory.isAvailable(item.productId, item.quantity)
            throw new InsufficientStockException(item.productId)

    // 2. reserve, rolling back on partial failure
    reserved = []
    try
        for item in cart.items
            inventory.reserve(item.productId, item.quantity)
            reserved.add(item)
    catch error
        for item in reserved
            inventory.release(item.productId, item.quantity)
        throw error

    // 3. price and pay
    breakdown = pricing.price(cart, promoCodes)
    paymentOk = payment.charge(paymentMethod, breakdown.total)

    if !paymentOk
        for item in reserved
            inventory.release(item.productId, item.quantity)
        throw new PaymentFailedException()

    // 4. freeze the order and empty the cart
    order = new Order(generateId(), cart.userId, copyOf(cart.items), breakdown, CONFIRMED)
    cart.clear()
    return order
```

Validate-then-reserve-then-charge with compensating releases is the point of this method:
every failure path leaves inventory exactly as it was.

## Code

CartItem & Product

```cs
public class Product
{
    public string Id { get; }
    public string Name { get; }
    public double Price { get; }
    public string Category { get; }

    public Product(string id, string name, double price, string category)
    {
        Id = id;
        Name = name;
        Price = price;
        Category = category;
    }
}

public class CartItem
{
    public string ProductId { get; }
    public string Name { get; }
    public double UnitPrice { get; }
    public string Category { get; }
    public int Quantity { get; set; }

    public CartItem(Product product, int quantity)
    {
        ProductId = product.Id;
        Name = product.Name;
        UnitPrice = product.Price;
        Category = product.Category;
        Quantity = quantity;
    }

    public double LineTotal() => UnitPrice * Quantity;

    public CartItem Copy() => new(new Product(ProductId, Name, UnitPrice, Category), Quantity);
}
```

Cart

```cs
using System;
using System.Collections.Generic;
using System.Linq;

public class Cart
{
    private readonly Dictionary<string, CartItem> _items = new();

    public string Id { get; }
    public string UserId { get; }

    public Cart(string id, string userId)
    {
        Id = id;
        UserId = userId;
    }

    public void AddItem(Product product, int quantity, IInventoryService inventory)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be positive");
        }

        var newQty = (_items.TryGetValue(product.Id, out var existing) ? existing.Quantity : 0) + quantity;

        if (!inventory.IsAvailable(product.Id, newQty))
        {
            throw new InvalidOperationException($"Insufficient stock for {product.Id}");
        }

        if (existing != null)
        {
            existing.Quantity = newQty;
        }
        else
        {
            _items[product.Id] = new CartItem(product, quantity);
        }
    }

    public void RemoveItem(string productId) => _items.Remove(productId);

    public void UpdateQuantity(string productId, int quantity)
    {
        if (quantity <= 0)
        {
            _items.Remove(productId);
            return;
        }

        if (_items.TryGetValue(productId, out var item))
        {
            item.Quantity = quantity;
        }
    }

    public IReadOnlyList<CartItem> Items => _items.Values.ToList();
    public double Subtotal() => _items.Values.Sum(i => i.LineTotal());
    public bool IsEmpty => _items.Count == 0;
    public void Clear() => _items.Clear();
}
```

Discounts

```cs
using System;

public interface IDiscount
{
    string Code { get; }
    bool IsApplicable(Cart cart, CartItem item);
    double Calculate(double baseAmount);
}

public class PercentageDiscount : IDiscount
{
    private readonly double _percent;
    private readonly double _maxDiscount;
    private readonly string _categoryFilter;

    public string Code { get; }

    public PercentageDiscount(string code, double percent, double maxDiscount, string categoryFilter = null)
    {
        Code = code;
        _percent = percent;
        _maxDiscount = maxDiscount;
        _categoryFilter = categoryFilter;
    }

    public bool IsApplicable(Cart cart, CartItem item)
        => _categoryFilter == null || item?.Category == _categoryFilter;

    public double Calculate(double baseAmount)
        => Math.Min(baseAmount * _percent / 100, _maxDiscount);
}

public class FlatDiscount : IDiscount
{
    private readonly double _amount;
    private readonly double _minCartValue;

    public string Code { get; }

    public FlatDiscount(string code, double amount, double minCartValue)
    {
        Code = code;
        _amount = amount;
        _minCartValue = minCartValue;
    }

    public bool IsApplicable(Cart cart, CartItem item) => cart.Subtotal() >= _minCartValue;

    public double Calculate(double baseAmount) => Math.Min(_amount, baseAmount);
}
```

Shipping & Tax

```cs
public interface IShippingCalculator
{
    double Calculate(Cart cart, double amountAfterDiscount);
}

public class FreeOverThresholdShipping : IShippingCalculator
{
    private readonly double _flatFee;
    private readonly double _threshold;

    public FreeOverThresholdShipping(double flatFee, double threshold)
    {
        _flatFee = flatFee;
        _threshold = threshold;
    }

    public double Calculate(Cart cart, double amountAfterDiscount)
        => amountAfterDiscount >= _threshold ? 0 : _flatFee;
}

public interface ITaxCalculator
{
    double Calculate(double taxableAmount, Cart cart);
}

public class FlatRateTaxCalculator : ITaxCalculator
{
    private readonly double _ratePercent;

    public FlatRateTaxCalculator(double ratePercent) => _ratePercent = ratePercent;

    public double Calculate(double taxableAmount, Cart cart) => taxableAmount * _ratePercent / 100;
}
```

PricingEngine

```cs
using System;
using System.Collections.Generic;

public class PriceBreakdown
{
    public double Subtotal { get; }
    public double ItemDiscount { get; }
    public double CartDiscount { get; }
    public double Shipping { get; }
    public double Tax { get; }
    public double Total { get; }
    public List<string> AppliedCodes { get; }

    public PriceBreakdown(double subtotal, double itemDiscount, double cartDiscount,
                          double shipping, double tax, double total, List<string> appliedCodes)
    {
        Subtotal = subtotal;
        ItemDiscount = itemDiscount;
        CartDiscount = cartDiscount;
        Shipping = shipping;
        Tax = tax;
        Total = total;
        AppliedCodes = appliedCodes;
    }
}

public class PricingEngine
{
    private readonly List<IDiscount> _itemDiscounts;
    private readonly List<IDiscount> _cartDiscounts;
    private readonly IShippingCalculator _shipping;
    private readonly ITaxCalculator _tax;

    public PricingEngine(List<IDiscount> itemDiscounts, List<IDiscount> cartDiscounts,
                         IShippingCalculator shipping, ITaxCalculator tax)
    {
        _itemDiscounts = itemDiscounts;
        _cartDiscounts = cartDiscounts;
        _shipping = shipping;
        _tax = tax;
    }

    public PriceBreakdown Price(Cart cart, HashSet<string> promoCodes)
    {
        var applied = new List<string>();
        double subtotal = 0, itemDiscountTotal = 0;

        foreach (var item in cart.Items)
        {
            var lineTotal = item.LineTotal();
            subtotal += lineTotal;

            foreach (var discount in _itemDiscounts)
            {
                if (!promoCodes.Contains(discount.Code) || !discount.IsApplicable(cart, item))
                {
                    continue;
                }

                itemDiscountTotal += discount.Calculate(lineTotal);
                if (!applied.Contains(discount.Code))
                {
                    applied.Add(discount.Code);
                }
            }
        }

        var afterItem = subtotal - itemDiscountTotal;
        double cartDiscountTotal = 0;

        foreach (var discount in _cartDiscounts)
        {
            if (!promoCodes.Contains(discount.Code) || !discount.IsApplicable(cart, null))
            {
                continue;
            }

            cartDiscountTotal += discount.Calculate(afterItem);
            applied.Add(discount.Code);
        }

        var afterDiscount = Math.Max(0, afterItem - cartDiscountTotal);
        var shippingCost = _shipping.Calculate(cart, afterDiscount);
        var taxAmount = _tax.Calculate(afterDiscount + shippingCost, cart);

        return new PriceBreakdown(subtotal, itemDiscountTotal, cartDiscountTotal, shippingCost,
                                  taxAmount, afterDiscount + shippingCost + taxAmount, applied);
    }
}
```

Inventory & Checkout

```cs
using System;
using System.Collections.Generic;
using System.Linq;

public interface IInventoryService
{
    bool IsAvailable(string productId, int quantity);
    void Reserve(string productId, int quantity);
    void Release(string productId, int quantity);
}

public interface IPaymentProcessor
{
    bool Charge(string paymentMethod, double amount);
}

public enum OrderStatus { Confirmed, Cancelled }

public class Order
{
    public string Id { get; }
    public string UserId { get; }
    public IReadOnlyList<CartItem> Lines { get; }
    public PriceBreakdown Breakdown { get; }
    public OrderStatus Status { get; }

    public Order(string id, string userId, IReadOnlyList<CartItem> lines,
                 PriceBreakdown breakdown, OrderStatus status)
    {
        Id = id;
        UserId = userId;
        Lines = lines;
        Breakdown = breakdown;
        Status = status;
    }
}

public class CheckoutService
{
    private readonly IInventoryService _inventory;
    private readonly PricingEngine _pricing;
    private readonly IPaymentProcessor _payment;

    public CheckoutService(IInventoryService inventory, PricingEngine pricing, IPaymentProcessor payment)
    {
        _inventory = inventory;
        _pricing = pricing;
        _payment = payment;
    }

    public Order Checkout(Cart cart, HashSet<string> promoCodes, string paymentMethod)
    {
        if (cart.IsEmpty)
        {
            throw new InvalidOperationException("Cart is empty");
        }

        foreach (var item in cart.Items)
        {
            if (!_inventory.IsAvailable(item.ProductId, item.Quantity))
            {
                throw new InvalidOperationException($"Out of stock: {item.ProductId}");
            }
        }

        var reserved = new List<CartItem>();
        try
        {
            foreach (var item in cart.Items)
            {
                _inventory.Reserve(item.ProductId, item.Quantity);
                reserved.Add(item);
            }

            var breakdown = _pricing.Price(cart, promoCodes);

            if (!_payment.Charge(paymentMethod, breakdown.Total))
            {
                throw new InvalidOperationException("Payment failed");
            }

            var order = new Order(Guid.NewGuid().ToString(), cart.UserId,
                                  cart.Items.Select(i => i.Copy()).ToList(),
                                  breakdown, OrderStatus.Confirmed);
            cart.Clear();
            return order;
        }
        catch
        {
            foreach (var item in reserved)
            {
                _inventory.Release(item.ProductId, item.Quantity);
            }
            throw;
        }
    }
}
```

Usage

```cs
var pricing = new PricingEngine(
    itemDiscounts: new List<IDiscount> { new PercentageDiscount("SAVE20", 20, 2000, "ELECTRONICS") },
    cartDiscounts: new List<IDiscount> { new FlatDiscount("FLAT50", 50, minCartValue: 500) },
    shipping: new FreeOverThresholdShipping(flatFee: 40, threshold: 500),
    tax: new FlatRateTaxCalculator(18));

var cart = new Cart("c1", "u1");
cart.AddItem(new Product("p1", "Headphones", 1000, "ELECTRONICS"), 1, inventory);

var breakdown = pricing.Price(cart, new HashSet<string> { "SAVE20", "FLAT50" });
// subtotal 1000 -> item -200 -> cart -50 -> 750, shipping 0, tax 135, total 885
```

## Extensibility

### "How would you add a Buy-2-Get-1-Free promotion?"

New `IDiscount`:

```
class BuyXGetYDiscount implements Discount:
    calculate(base) -> freeUnits = (item.quantity / (buyQty + freeQty)) * freeQty
                       return freeUnits * item.unitPrice
```

Register it in `itemDiscounts`. The pipeline is unchanged.

### "Two promo codes shouldn't stack - how do you enforce that?"

Add exclusivity metadata (`stackable: false`, or a mutex group) and resolve before pricing:
sort candidates by benefit and keep the best from each group. Making this a separate
`PromotionResolver` step keeps `PricingEngine` from growing conditionals.

### "Two users buy the last unit at the same time - what happens?"

The `IsAvailable` → `Reserve` window is a race. Fix it with an atomic conditional decrement in
the inventory store (`UPDATE stock SET qty = qty - :n WHERE id = :id AND qty >= :n`) and treat
zero rows affected as out-of-stock. Optimistic concurrency with a version column works too;
pessimistic row locks are simpler but hurt throughput on hot SKUs.

### "The price changes while an item sits in the cart - what should happen?"

`CartItem` holds a snapshot only for display. At checkout, re-fetch catalog prices and either
re-price silently (common) or show a "price changed" confirmation. Never charge a stale
snapshot - that is a revenue and trust bug.

### "How do you keep a guest cart after login?"

Merge on authentication: for each guest item, add to the user cart, summing quantities and
re-validating stock. Keep it in a `CartMergeService` so neither cart owns the policy.

### "How would you support multiple currencies?"

Replace `double` with a `Money { amount, currency }` value object and make every calculator
currency-aware. Doing this early is much cheaper than retrofitting; it also removes the
floating-point rounding risk (use `decimal` for money).

### "How do you handle abandoned carts and reserved-but-unpaid stock?"

Reservations get a TTL; a scheduled sweeper releases expired reservations. That is exactly
the job scheduler problem, plugged in here.
