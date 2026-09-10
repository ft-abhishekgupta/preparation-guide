# My LLD Notes

## Template

```cs
public class XService                       // orchestrator: ParkingLot, BookingSystem, Locker,
{                                           // RateLimiter, RideService, MatchingEngine, JobScheduler
    private readonly Dictionary<string, Entity> _byId = new();   // 1. indexes for O(1) routing
    private readonly HashSet<string> _claimed = new();           // 2. occupancy / availability
    private readonly IStrategy _strategy;                        // 3. injected pluggable rule
    private readonly List<IListener> _listeners = new();         // 4. observers (optional)
    private readonly object _lock = new();                       // 5. lock if shared mutable state

    public Result DoTheThing(Input input)
    {
        // guard clauses → find resource → claim atomically → issue token → notify → return
    }
}
```

## Design Patterns

### Factory

```cs
interface IPayment {
    void Pay();
}
class UpiPayment : IPayment {
    public void Pay() {
        Console.WriteLine("UPI Payment");
    }
}
class CardPayment : IPayment {
    public void Pay() {
        Console.WriteLine("Card Payment");
    }
}
class PaymentFactory {
    public static IPayment Create(string type) {
        return type switch {
            "UPI" => new UpiPayment(),
            "CARD" => new CardPayment(),
            _ => throw new ArgumentException("Invalid payment type")
        };
    }
}
class Client {
    public void Process() {
        IPayment payment = PaymentFactory.Create("UPI");
        payment.Pay();
    }
}
```

### Singleton

```cs
class Logger {
    private static readonly Lazy<Logger> _instance = new(() => new Logger());
    private Logger() {
    }
    public static Logger Instance => _instance.Value;
    public void Log(string message) {
        Console.WriteLine(message);
    }
}
```

### Strategy

```cs
interface IPricingStrategy {
    decimal Calculate(decimal amount);
}
class RegularPricing : IPricingStrategy {
    public decimal Calculate(decimal amount) {
        return amount;
    }
}
class PremiumPricing : IPricingStrategy {
    public decimal Calculate(decimal amount) {
        return amount * 0.9m;
    }
}
class Checkout {
    private readonly IPricingStrategy _strategy;
    public Checkout(IPricingStrategy strategy) {
        _strategy = strategy;
    }
    public decimal GetPrice(decimal amount) {
        return _strategy.Calculate(amount);
    }
}
// Usage
IPricingStrategy strategy = new PremiumPricing();
Checkout checkout = new Checkout(strategy);
decimal price = checkout.GetPrice(100); // 90
```

### Observer

```cs
interface IOrderObserver {
    void Update(string status);
}
class EmailNotifier : IOrderObserver {
    public void Update(string status) {
        Console.WriteLine($"Email: {status}");
    }
}
class SmsNotifier : IOrderObserver {
    public void Update(string status) {
        Console.WriteLine($"SMS: {status}");
    }
}
class Order {
    private readonly List<IOrderObserver> _observers = new();
    public void Subscribe(IOrderObserver observer) {
        _observers.Add(observer);
    }
    public void SetStatus(string status) {
        foreach (var observer in _observers)
            observer.Update(status);
    }
}
```

### State

```cs
// ENUM STATE - ONLY VALUES NEEDED
private StateEnum state;

public SetState(StateEnum newState) => state = newState;
```

```cs
// STATE OBJECTS - BEHAVIOUR CHANGES BASED ON STATE
interface IOrderState {
    void Process(Order order);
}
class Order {
    private IOrderState _state;
    public Order(IOrderState state) {
        _state = state;
    }
    public void SetState(IOrderState state) {
        _state = state;
    }
    public void Process() {
        _state.Process(this);
    }
}
class NewState : IOrderState {
    public void Process(Order order) {
        Console.WriteLine("Processing new order");
        order.SetState(new PaidState());
    }
}
class PaidState : IOrderState {
    public void Process(Order order) {
        Console.WriteLine("Shipping order");
        order.SetState(new ShippedState());
    }
}
class ShippedState : IOrderState {
    public void Process(Order order) {
        Console.WriteLine("Already shipped");
    }
}
// Usage
Order order = new Order(new NewState());
order.Process(); // Processing new order
order.Process(); // Shipping order
order.Process(); // Already shipped
```

---

## Concurrency Control

### Concurrent Dictionary

```cs
private readonly ConcurrentDictionary<string, int> dict = new();
dict.TryRemove(key, out value);
dict.TryAdd(key, value);
dict.TryGetValue(key, out value);
dict.AddOrUpdate(
    key,
    addValue,
    (key, oldValue) => newValue);
```

### Locking

```cs
class ClassName
{
    private readonly object _lock = new object();

    public void MethodName()
    {
        lock (_lock)
        {
            // Only one thread can be here at a time
        }
    }
}
```

### FineGrain Locking

```cs
class Showtime {
    private readonly IDictionary<string, object> _locks = new();
    Showtime(...){
        ...
        foreach(var s in availableSeats){
            _locks[s] = new object();
        }
        ...
    }
    public Reservation BookSeats(User user, IList<string> seats){
        var orderedSeat = new List<seats>();
        Array.Sort(orderedSeat);
        var locks = new List<object>();
        foreach(var s in orderedSeat)
            locks.Add(_locks[s]);

        foreach(var l in locks)
            Monitor.Enter(l);

        try {
            ... booking flow
        }
        finally {
            foreach(int i = locks.Count-1; i >= 0; i--)
                Monitor.Exit(locks[i]);
        }

    }
}
```

### Semaphore

```cs
using System.Threading;

var permits = new SemaphoreSlim(5);     // Allow 5 concurrent operations
await permits.WaitAsync();              // Block if no permits available
try
{
    DoWork();
}
finally
{
    permits.Release();  // Always release, even on exception
}
```

### Blocking Queues

```cs
using System.Collections.Concurrent;

var queue = new BlockingCollection<Task>(boundedCapacity: 100);
queue.Add(task);     // Blocks if queue is full
var t = queue.Take();  // Blocks if queue is empty
```

### Manual Pub Sub

```cs
// Producer
lock (queue)
{
    while (queue.Count == capacity)
        Monitor.Wait(queue);

    queue.Enqueue(task);

    Monitor.PulseAll(queue);
}
// Consumer
lock (queue)
{
    while (queue.Count == 0)
        Monitor.Wait(queue);

    var task = queue.Dequeue();

    Monitor.PulseAll(queue);
}
```

## Problem Patterns

### Rule Engine

```cs
public interface IRule
{
    bool IsApplicable(Input input);
    Output Execute(Input input);
}
public class RuleEngine
{
    private readonly List<IRule> rules;

    public RuleEngine(List<IRule> rules)
    {
        this.rules = rules;
    }

    public List<Output> Evaluate(Input input)
    {
        var results = new List<Output>();

        foreach (var rule in rules)
        {
            if (rule.IsApplicable(input))
                results.Add(rule.Execute(input));
        }

        return results;
    }
}
```

### Background Workers + PubSub

```cs
using System.Collections.Concurrent;

public class TaskQueue
{
    private readonly BlockingCollection<int> queue;
    private readonly List<Thread> workers = new();

    public TaskQueue(int capacity, int workerCount)
    {
        queue = new BlockingCollection<int>(capacity);

        for (int i = 0; i < workerCount; i++)
        {
            var worker = new Worker(queue);
            var thread = new Thread(worker.Run);

            workers.Add(thread);
            thread.Start();
        }
    }

    public void Add(int task)
    {
        queue.Add(task); // Blocks if queue is full
    }

    public void Stop()
    {
        queue.CompleteAdding();

        foreach (var thread in workers)
            thread.Join();
    }
}

public class Worker
{
    private readonly BlockingCollection<int> queue;

    public Worker(BlockingCollection<int> queue)
    {
        this.queue = queue;
    }

    public void Run()
    {
        foreach (var task in queue.GetConsumingEnumerable())
        {
            Console.WriteLine(
                $"Worker {Thread.CurrentThread.ManagedThreadId} processing {task}"
            );
        }
    }
}
```

### Manual Pub Sub with DLQ

```cs
using System;
using System.Collections.Generic;
using System.Threading;

public class TaskItem
{
    public int Id { get; }
    public DateTime ExecuteAt { get; }
    public int RetryCount { get; set; }

    public TaskItem(int id, DateTime executeAt)
    {
        Id = id;
        ExecuteAt = executeAt;
    }
}

public class TaskScheduler
{
    // Ready-to-execute tasks
    private readonly Queue<TaskItem> queue = new();

    // Delayed tasks, ordered by execution time
    private readonly PriorityQueue<TaskItem, DateTime> delayQueue = new();

    // Failed tasks
    private readonly Queue<TaskItem> dlq = new();

    private readonly object locker = new();
    private readonly int maxRetries;

    public TaskScheduler(int workerCount, int maxRetries)
    {
        this.maxRetries = maxRetries;

        for (int i = 0; i < workerCount; i++)
        {
            var thread = new Thread(Worker);
            thread.Start();
        }

        // Separate thread moves delayed tasks → ready queue
        var delayThread = new Thread(ProcessDelayQueue);
        delayThread.Start();
    }

    // Add immediately executable task
    public void Submit(TaskItem task)
    {
        lock (locker)
        {
            queue.Enqueue(task);

            // Wake up workers
            Monitor.PulseAll(locker);
        }
    }

    // Add delayed task
    public void SubmitDelayed(TaskItem task)
    {
        lock (locker)
        {
            delayQueue.Enqueue(task, task.ExecuteAt);

            // Wake delay thread because a new task may have
            // an earlier execution time
            Monitor.PulseAll(locker);
        }
    }

    private void Worker()
    {
        while (true)
        {
            TaskItem task;

            lock (locker)
            {
                // Nothing to execute
                while (queue.Count == 0)
                {
                    Monitor.Wait(locker);
                }

                task = queue.Dequeue();
            }

            // Execute outside lock
            try
            {
                Execute(task);
            }
            catch
            {
                HandleFailure(task);
            }
        }
    }

    private void ProcessDelayQueue()
    {
        while (true)
        {
            TaskItem task;

            lock (locker)
            {
                while (delayQueue.Count == 0)
                {
                    Monitor.Wait(locker);
                }

                delayQueue.TryPeek(out task, out DateTime executeAt);

                var waitTime = executeAt - DateTime.Now;

                if (waitTime > TimeSpan.Zero)
                {
                    // Don't hold the lock while sleeping
                    Monitor.Wait(locker, waitTime);
                    continue;
                }

                delayQueue.Dequeue();
                queue.Enqueue(task);

                // Wake a worker
                Monitor.PulseAll(locker);
            }
        }
    }

    private void Execute(TaskItem task)
    {
        Console.WriteLine($"Executing task {task.Id}");

        // Simulate work
        Thread.Sleep(500);

        // Example failure
        if (task.Id == 3)
            throw new Exception("Task failed");
    }

    private void HandleFailure(TaskItem task)
    {
        lock (locker)
        {
            task.RetryCount++;

            if (task.RetryCount > maxRetries)
            {
                // Give up → DLQ
                dlq.Enqueue(task);

                Console.WriteLine(
                    $"Task {task.Id} moved to DLQ"
                );
            }
            else
            {
                // Retry
                queue.Enqueue(task);

                Console.WriteLine(
                    $"Retrying task {task.Id}, attempt {task.RetryCount}"
                );

                Monitor.PulseAll(locker);
            }
        }
    }
}
```

### Game Syste

```
Game
    Player
    Board
State
Rule
```

### Inventory / Booking

```
User → Cart → Order → Payment
          │
          ▼
      Inventory
          │
          ▼
      Booking/Reservation
```
