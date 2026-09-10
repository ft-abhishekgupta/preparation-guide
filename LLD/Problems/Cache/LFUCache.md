# LFU Cache

Eviction based on frequency

## Class Design

```
Dictionary<key, Node>
        +
Dictionary<frequency, LinkedList<Node>>
        +
minFrequency
```

## Implementation

```cs
public class LFUCache
{
    private class Node
    {
        public int Key;
        public int Value;
        public int Frequency;

        public Node(int key, int value)
        {
            Key = key;
            Value = value;
            Frequency = 1;
        }
    }

    private readonly int capacity;
    private int minFrequency;

    // key -> LinkedListNode<Node> (holds O(1) pointers)
    private readonly Dictionary<int, LinkedListNode<Node>> map;
    private readonly Dictionary<int, LinkedList<Node>> freqMap;
    private readonly object _lock = new();

    public LFUCache(int capacity)
    {
        if (capacity <= 0)
            throw new ArgumentException("Capacity must be greater than 0");

        this.capacity = capacity;
        map = new();
        freqMap = new();
    }

    public int Get(int key)
    {
        lock (_lock)
        {
            if (!map.TryGetValue(key, out var listNode))
                return -1;

            IncreaseFrequency(listNode);
            return listNode.Value.Value;
        }
    }

    public void Put(int key, int value)
    {
        lock (_lock)
        {
            if (map.TryGetValue(key, out var listNode))
            {
                listNode.Value.Value = value;
                IncreaseFrequency(listNode);
                return;
            }

            if (map.Count >= capacity)
            {
                Evict();
            }

            var node = new Node(key, value);
            if (!freqMap.TryGetValue(1, out var list))
            {
                list = new LinkedList<Node>();
                freqMap[1] = list;
            }

            // AddFirst returns the newly created LinkedListNode wrapper
            map[key] = list.AddFirst(node);
            minFrequency = 1;
        }
    }

    private void IncreaseFrequency(LinkedListNode<Node> listNode)
    {
        var node = listNode.Value;
        int oldFrequency = node.Frequency;
        var oldList = freqMap[oldFrequency];

        // O(1) pointer unlinking
        oldList.Remove(listNode);

        if (oldFrequency == minFrequency && oldList.Count == 0)
        {
            minFrequency++;
        }

        node.Frequency++;

        if (!freqMap.TryGetValue(node.Frequency, out var nextList))
        {
            nextList = new LinkedList<Node>();
            freqMap[node.Frequency] = nextList;
        }

        // O(1) insertion reusing the same listNode wrapper
        nextList.AddFirst(listNode);
    }

    private void Evict()
    {
        var list = freqMap[minFrequency];
        var lruNode = list.Last!;

        list.RemoveLast();
        map.Remove(lruNode.Value.Key);
    }
}
```
