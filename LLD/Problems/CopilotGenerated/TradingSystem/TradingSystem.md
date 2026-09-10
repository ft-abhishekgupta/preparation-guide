# Trading System (Order Matching Engine)

A stock exchange matching engine: traders place limit and market orders, the order book
matches them by price-time priority, and trades are published to subscribers.

## Prompt

```
"Design the core of a stock trading system. Traders place buy and sell orders on a symbol,
and the engine matches them.

{
  "orderId": "o1",
  "symbol": "AAPL",
  "side": "BUY",
  "type": "LIMIT",
  "price": 150.50,
  "quantity": 100
}

A buy matches a sell when the buy price >= the sell price. Partial fills are allowed.
Focus on the order book, matching rules and trade generation."
```

## Questions

```
"Which order types?"
> LIMIT and MARKET. Stop orders as an extension

"What is the matching priority?"
> Price first, then time (FIFO at the same price)

"Are partial fills allowed?"
> Yes, an order rests in the book until fully filled

"What happens to an unfilled market order?"
> Fill what it can, cancel the rest - it never rests in the book

"Can orders be cancelled or modified?"
> Cancel yes. Modify = cancel and re-place (loses time priority)

"One symbol or many?"
> Many, but each symbol has an independent book

"Do we need positions, balances and risk checks?"
> Basic validation only, portfolio accounting is out of scope

"Threading model?"
> One thread per symbol book - discuss why
```

## Requirements

```
Requirements:
1. placeOrder(order) for LIMIT and MARKET, BUY and SELL
2. Each symbol has its own order book with buy and sell sides
3. Matching by price-time priority:
   - Buy book: highest price first
   - Sell book: lowest price first
   - Ties broken by earliest arrival (FIFO)
4. A BUY matches a SELL when buyPrice >= sellPrice
5. Execution price is the resting (maker) order's price
6. Partial fills supported; the remainder rests in the book (LIMIT only)
7. MARKET orders fill against the best available prices, remainder is cancelled
8. cancelOrder(orderId) removes an open order
9. Each match emits a Trade to subscribers
10. Expose the top of book (best bid / best ask)

Out of scope:
- Settlement, clearing, margin
- Persistence and replay
- Circuit breakers, auctions, hidden/iceberg orders
- Network/FIX protocol layer
```

## Core Entities

```
MatchingEngine  : Orchestrator - routes orders to the right book
OrderBook       : One symbol; bids and asks with matching logic
Order           : Immutable identity + mutable remaining quantity
PriceLevel      : FIFO queue of orders at one price
Trade           : Result of a match (buy, sell, price, qty)
TradeListener   : Observer for executions
OrderStatus     : NEW, PARTIALLY_FILLED, FILLED, CANCELLED, REJECTED
```

**Why this shape**

| Concern | Choice | Reason |
|---|---|---|
| Price ordering | Sorted map keyed by price | O(log n) best price, ordered walk for sweeps |
| Time priority | FIFO queue per price level | Fairness at the same price, O(1) head access |
| Trade publication | Observer | Market data, risk and persistence subscribe separately |
| Isolation | One book per symbol | AAPL and TSLA never contend for the same lock |

The data structure is the design here: `SortedDictionary<price, Queue<Order>>` gives price
priority from the map and time priority from the queue, together in one structure.

## Class Design

```
class MatchingEngine:
    - books: Map<symbol, OrderBook>
    - orderIndex: Map<orderId, Order>
    - listeners: List<TradeListener>

    + placeOrder(order) -> List<Trade>
    + cancelOrder(orderId) -> boolean
    + getBestBid(symbol) / getBestAsk(symbol) -> double | null
    + subscribe(listener) -> void

class OrderBook:
    - symbol: string
    - bids: SortedMap<double, Queue<Order>>   // descending price
    - asks: SortedMap<double, Queue<Order>>   // ascending price

    + addOrder(order) -> List<Trade>
    + cancel(order) -> boolean
    + bestBid() -> double | null
    + bestAsk() -> double | null

class Order:
    - id, symbol, traderId: string
    - side: Side               // BUY, SELL
    - type: OrderType          // LIMIT, MARKET
    - price: double            // ignored for MARKET
    - quantity: int            // original
    - remaining: int
    - status: OrderStatus
    - timestamp: long

    + fill(qty) -> void
    + isFilled() -> boolean

class Trade:
    - id: string
    - symbol: string
    - buyOrderId, sellOrderId: string
    - price: double
    - quantity: int
    - timestamp: long

interface TradeListener:
    + onTrade(trade) -> void
```

**Order book layout**

```
              ASKS (sell)                      BIDS (buy)
   price      queue (FIFO)             price      queue (FIFO)
   -------------------------           -------------------------
   151.00  -> [o7, o9]                 150.00  -> [o2, o5]     <- best bid
   150.75  -> [o4]                      149.50  -> [o1]
   150.50  -> [o3]        <- best ask   149.00  -> [o6, o8]

   spread = bestAsk - bestBid = 150.50 - 150.00 = 0.50
```

An incoming BUY @ 150.50 crosses the spread and matches `o3` at **150.50 - the resting
order's price**, not the incoming price. The maker sets the price; the taker accepts it.

## Implementation

OrderBook.addOrder - match first, rest afterwards

```
addOrder(order)
    trades = []
    oppositeBook = (order.side == BUY) ? asks : bids

    while order.remaining > 0 && !oppositeBook.isEmpty()
        bestPrice = (order.side == BUY) ? oppositeBook.firstKey()   // lowest ask
                                        : oppositeBook.lastKey()    // highest bid

        if !crosses(order, bestPrice)
            break                        // no more matchable price levels

        queue = oppositeBook[bestPrice]

        while order.remaining > 0 && !queue.isEmpty()
            resting = queue.peek()
            qty = min(order.remaining, resting.remaining)

            order.fill(qty)
            resting.fill(qty)

            trades.add(new Trade(
                symbol, buyId, sellId,
                price: bestPrice,        // resting order's price
                quantity: qty))

            if resting.isFilled()
                queue.poll()

        if queue.isEmpty()
            oppositeBook.remove(bestPrice)

    // whatever is left
    if order.remaining > 0
        if order.type == MARKET
            order.status = CANCELLED     // market orders never rest
        else
            addToBook(order)             // becomes a resting maker order

    return trades

crosses(order, bestOppositePrice)
    if order.type == MARKET: return true
    return order.side == BUY ? order.price >= bestOppositePrice
                             : order.price <= bestOppositePrice
```

addToBook / cancel

```
addToBook(order)
    book = (order.side == BUY) ? bids : asks
    if !book.contains(order.price)
        book[order.price] = new Queue()
    book[order.price].add(order)         // appended => time priority preserved

cancel(order)
    book = (order.side == BUY) ? bids : asks
    queue = book.get(order.price)
    if queue == null: return false

    removed = queue.remove(order)
    if queue.isEmpty(): book.remove(order.price)

    if removed: order.status = CANCELLED
    return removed
```

MatchingEngine.placeOrder

```
placeOrder(order)
    validate(order)                      // qty > 0, limit needs a price, symbol known

    book = books.computeIfAbsent(order.symbol, s -> new OrderBook(s))
    orderIndex[order.id] = order

    trades = book.addOrder(order)

    for trade in trades
        for listener in listeners
            listener.onTrade(trade)      // market data, risk, persistence

    return trades
```

Worked example

```
Book:  ASK 150.50 x 100 (o3, resting)
Incoming: BUY LIMIT 150.50 x 150

  crosses? 150.50 >= 150.50 -> yes
  match 100 @ 150.50  -> Trade(o3, incoming, 150.50, 100), o3 FILLED
  ask level empty     -> removed
  remaining 50, LIMIT -> rests at bid 150.50

Result: 1 trade, book now shows best bid 150.50 x 50, no asks
```

## Code

Order & Trade

```cs
using System;

public enum Side { Buy, Sell }
public enum OrderType { Limit, Market }
public enum OrderStatus { New, PartiallyFilled, Filled, Cancelled, Rejected }

public class Order
{
    public string Id { get; }
    public string Symbol { get; }
    public string TraderId { get; }
    public Side Side { get; }
    public OrderType Type { get; }
    public double Price { get; }
    public int Quantity { get; }
    public int Remaining { get; private set; }
    public OrderStatus Status { get; set; } = OrderStatus.New;
    public long Timestamp { get; }

    public Order(string id, string symbol, string traderId, Side side, OrderType type,
                 double price, int quantity)
    {
        Id = id;
        Symbol = symbol;
        TraderId = traderId;
        Side = side;
        Type = type;
        Price = price;
        Quantity = quantity;
        Remaining = quantity;
        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    public void Fill(int qty)
    {
        Remaining -= qty;
        Status = Remaining == 0 ? OrderStatus.Filled : OrderStatus.PartiallyFilled;
    }

    public bool IsFilled => Remaining == 0;
}

public class Trade
{
    public string Id { get; }
    public string Symbol { get; }
    public string BuyOrderId { get; }
    public string SellOrderId { get; }
    public double Price { get; }
    public int Quantity { get; }
    public long Timestamp { get; }

    public Trade(string symbol, string buyOrderId, string sellOrderId, double price, int quantity)
    {
        Id = Guid.NewGuid().ToString();
        Symbol = symbol;
        BuyOrderId = buyOrderId;
        SellOrderId = sellOrderId;
        Price = price;
        Quantity = quantity;
        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}
```

OrderBook

```cs
using System.Collections.Generic;
using System.Linq;

public class OrderBook
{
    private readonly string _symbol;

    // Both sorted ascending by price; we read the correct end for each side.
    private readonly SortedDictionary<double, LinkedList<Order>> _bids = new();
    private readonly SortedDictionary<double, LinkedList<Order>> _asks = new();

    public OrderBook(string symbol) => _symbol = symbol;

    public double? BestBid => _bids.Count == 0 ? null : _bids.Keys.Last();
    public double? BestAsk => _asks.Count == 0 ? null : _asks.Keys.First();

    public List<Trade> AddOrder(Order order)
    {
        var trades = new List<Trade>();
        var opposite = order.Side == Side.Buy ? _asks : _bids;

        while (order.Remaining > 0 && opposite.Count > 0)
        {
            var bestPrice = order.Side == Side.Buy ? opposite.Keys.First() : opposite.Keys.Last();

            if (!Crosses(order, bestPrice))
            {
                break;
            }

            var queue = opposite[bestPrice];

            while (order.Remaining > 0 && queue.Count > 0)
            {
                var resting = queue.First.Value;
                var qty = System.Math.Min(order.Remaining, resting.Remaining);

                order.Fill(qty);
                resting.Fill(qty);

                var buyId = order.Side == Side.Buy ? order.Id : resting.Id;
                var sellId = order.Side == Side.Buy ? resting.Id : order.Id;
                trades.Add(new Trade(_symbol, buyId, sellId, bestPrice, qty));

                if (resting.IsFilled)
                {
                    queue.RemoveFirst();
                }
            }

            if (queue.Count == 0)
            {
                opposite.Remove(bestPrice);
            }
        }

        if (order.Remaining > 0)
        {
            if (order.Type == OrderType.Market)
            {
                order.Status = OrderStatus.Cancelled;
            }
            else
            {
                AddToBook(order);
            }
        }

        return trades;
    }

    private static bool Crosses(Order order, double bestOppositePrice)
    {
        if (order.Type == OrderType.Market)
        {
            return true;
        }

        return order.Side == Side.Buy
            ? order.Price >= bestOppositePrice
            : order.Price <= bestOppositePrice;
    }

    private void AddToBook(Order order)
    {
        var book = order.Side == Side.Buy ? _bids : _asks;

        if (!book.TryGetValue(order.Price, out var queue))
        {
            queue = new LinkedList<Order>();
            book[order.Price] = queue;
        }

        queue.AddLast(order);   // FIFO => time priority
    }

    public bool Cancel(Order order)
    {
        var book = order.Side == Side.Buy ? _bids : _asks;

        if (!book.TryGetValue(order.Price, out var queue))
        {
            return false;
        }

        var removed = queue.Remove(order);

        if (queue.Count == 0)
        {
            book.Remove(order.Price);
        }

        if (removed)
        {
            order.Status = OrderStatus.Cancelled;
        }

        return removed;
    }
}
```

MatchingEngine

```cs
using System;
using System.Collections.Generic;

public interface ITradeListener
{
    void OnTrade(Trade trade);
}

public class MatchingEngine
{
    private readonly Dictionary<string, OrderBook> _books = new();
    private readonly Dictionary<string, Order> _orders = new();
    private readonly List<ITradeListener> _listeners = new();

    public void Subscribe(ITradeListener listener) => _listeners.Add(listener);

    public List<Trade> PlaceOrder(Order order)
    {
        Validate(order);

        if (!_books.TryGetValue(order.Symbol, out var book))
        {
            book = new OrderBook(order.Symbol);
            _books[order.Symbol] = book;
        }

        _orders[order.Id] = order;
        var trades = book.AddOrder(order);

        foreach (var trade in trades)
        {
            foreach (var listener in _listeners)
            {
                listener.OnTrade(trade);
            }
        }

        return trades;
    }

    public bool CancelOrder(string orderId)
    {
        if (!_orders.TryGetValue(orderId, out var order) || order.IsFilled)
        {
            return false;
        }

        return _books.TryGetValue(order.Symbol, out var book) && book.Cancel(order);
    }

    public double? GetBestBid(string symbol)
        => _books.TryGetValue(symbol, out var book) ? book.BestBid : null;

    public double? GetBestAsk(string symbol)
        => _books.TryGetValue(symbol, out var book) ? book.BestAsk : null;

    private static void Validate(Order order)
    {
        if (order.Quantity <= 0)
        {
            throw new ArgumentException("Quantity must be positive");
        }

        if (order.Type == OrderType.Limit && order.Price <= 0)
        {
            throw new ArgumentException("Limit order requires a positive price");
        }
    }
}
```

Usage

```cs
var engine = new MatchingEngine();
engine.Subscribe(new ConsoleTradeListener());

engine.PlaceOrder(new Order("o1", "AAPL", "t1", Side.Sell, OrderType.Limit, 150.50, 100));
var trades = engine.PlaceOrder(new Order("o2", "AAPL", "t2", Side.Buy, OrderType.Limit, 150.50, 150));

// trades: 1 trade of 100 @ 150.50
// book:   best bid 150.50 (50 remaining from o2), no asks
```

## Extensibility

### "How would you add stop-loss orders?"

A stop order is not in the book - it is a trigger. Keep a separate `SortedDictionary<triggerPrice,
List<Order>>` per symbol. After each trade, check the last traded price and convert any
triggered stop orders into MARKET (or LIMIT) orders submitted to the book. This keeps
triggering out of the hot matching loop.

### "How would you support IOC / FOK orders?"

Add a `TimeInForce` field checked at the end of `AddOrder`:

- **IOC** (immediate-or-cancel): cancel the remainder instead of resting - the market-order rule
- **FOK** (fill-or-kill): pre-scan the opposite side for sufficient liquidity; if not enough,
  reject without executing anything
- **GTC**: current default behaviour

### "How do you make this thread safe?"

Do not lock a shared book from many threads. The standard exchange design is **single-writer
per symbol**: each symbol's book is owned by one thread that consumes from a lock-free ring
buffer (LMAX Disruptor style). Matching then needs no locks at all, is deterministic, and
different symbols scale horizontally across threads. Locking would both slow matching and
risk non-deterministic fill order - unacceptable for an exchange.

### "How do you keep O(1) cancellation?"

Today `queue.Remove(order)` is O(n) within a price level. Store a node handle:
`Map<orderId, LinkedListNode<Order>>` and remove by node - O(1). Real engines do exactly this,
since cancels typically outnumber fills.

### "How would you publish market data efficiently?"

Two feeds from the same `TradeListener` hook: trades (last price/size) and book deltas
(price-level changes). Publish incremental deltas plus a periodic snapshot rather than the
whole book each time. Subscribers can rebuild from snapshot + deltas.

### "How do you guarantee fairness?"

Time priority within a price level, deterministic single-threaded matching per symbol, and a
monotonic sequence number on every accepted order so ordering is reproducible on replay.

### "How would you validate risk before matching?"

A pre-trade risk check in `PlaceOrder`: buying power, position limits, price collars
(reject orders far from the last traded price). It belongs before the book so a bad order
never reaches the matching path.
