# LLD Notes

### Create the lock variable as instance of a class, not inside a method, so that everyone shares that lock

```cs
class ClassName
{
    private readonly object _lock = new object();
    ...
    // constructor and methods
}
```

### Fine Grain Locking

- Order the elements to prevent deadlock
- Release in reverse order

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

### Use ConcurrentDictionary for thread safe writes, if multiple thread can share a map
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