# LRU Cache
## Requirements
```
- The LRU cache should support the following operations:
- put(key, value): Insert a key-value pair into the cache. If the cache is at capacity, remove the least recently used item before inserting the new item.
- get(key): Get the value associated with the given key. If the key exists in the cache, move it to the front of the cache (most recently used) and return its value. If the key does not exist, return -1.
- The cache should have a fixed capacity, specified during initialization.
- The cache should be thread-safe, allowing concurrent access from multiple threads.
- The cache should be efficient in terms of time complexity for both put and get operations, ideally O(1).
```
## Class Design
- We use Dictionary and Doubly Linked List
- Doubly Linked Liste to maintain priority
- Map for random pointers to LL Nodes
## Using Built In Linked List
```cs
public class LRUCache
{
    private readonly int capacity;
    private readonly Dictionary<int, LinkedListNode<(int key, int value)>> map;
    private readonly LinkedList<(int key, int value)> list;
    private readonly object _lock = new();

    public LRUCache(int capacity)
    {
        if (capacity <= 0)
            throw new ArgumentException("Capacity must be greater than 0");

        this.capacity = capacity;

        map = new();
        list = new();
    }

    public int Get(int key)
    {
        lock (_lock)
        {
            if (!map.TryGetValue(key, out var node))
                return -1;

            // Move to front (most recently used)
            list.Remove(node);
            list.AddFirst(node);

            return node.Value.value;
        }
    }

    public void Put(int key, int value)
    {
        lock (_lock)
        {
            // Key already exists
            if (map.TryGetValue(key, out var node))
            {
                node.Value = (key, value);

                // Move to front
                list.Remove(node);
                list.AddFirst(node);

                return;
            }

            // Add new key
            var newNode = list.AddFirst((key, value));
            map[key] = newNode;

            // Evict least recently used
            if (map.Count > capacity)
            {
                var lru = list.Last!;

                list.RemoveLast();
                map.Remove(lru.Value.key);
            }
        }
    }
}
```
## Using Custom DLL Implementation
Node
```cs
class Node<K, V>
{
    public K key;
    public V value;
    public Node<K, V> prev;
    public Node<K, V> next;

    public Node(K key, V value)
    {
        this.key = key;
        this.value = value;
    }
}
```
Doubly Linked List
```cs
class DoublyLinkedList<K, V>
{
    private readonly Node<K, V> head;
    private readonly Node<K, V> tail;

    public DoublyLinkedList()
    {
        head = new Node<K, V>(default(K), default(V));
        tail = new Node<K, V>(default(K), default(V));
        head.next = tail;
        tail.prev = head;
    }

    public void AddFirst(Node<K, V> node)
    {
        node.next = head.next;
        node.prev = head;
        head.next.prev = node;
        head.next = node;
    }

    public void Remove(Node<K, V> node)
    {
        node.prev.next = node.next;
        node.next.prev = node.prev;
    }

    public void MoveToFront(Node<K, V> node)
    {
        Remove(node);
        AddFirst(node);
    }

    public Node<K, V> RemoveLast()
    {
        if (tail.prev == head) return null;
        Node<K, V> last = tail.prev;
        Remove(last);
        return last;
    }
}
```
LRU Cache
```cs
class LRUCache<K, V>
{
    private readonly int capacity;
    private readonly Dictionary<K, Node<K, V>> map;
    private readonly DoublyLinkedList<K, V> dll;
    private readonly object lockObject = new object();

    public LRUCache(int capacity)
    {
        this.capacity = capacity;
        this.map = new Dictionary<K, Node<K, V>>();
        this.dll = new DoublyLinkedList<K, V>();
    }

    public V Get(K key)
    {
        lock (lockObject)
        {
            if (!map.ContainsKey(key)) return default(V);
            Node<K, V> node = map[key];
            dll.MoveToFront(node);
            return node.value;
        }
    }

    public void Put(K key, V value)
    {
        lock (lockObject)
        {
            if (map.ContainsKey(key))
            {
                Node<K, V> node = map[key];
                node.value = value;
                dll.MoveToFront(node);
            }
            else
            {
                if (map.Count == capacity)
                {
                    Node<K, V> lru = dll.RemoveLast();
                    if (lru != null) map.Remove(lru.key);
                }
                Node<K, V> newNode = new Node<K, V>(key, value);
                dll.AddFirst(newNode);
                map[key] = newNode;
            }
        }
    }

    public void Remove(K key)
    {
        lock (lockObject)
        {
            if (!map.ContainsKey(key)) return;
            Node<K, V> node = map[key];
            dll.Remove(node);
            map.Remove(key);
        }
    }
}
```
Usage
```cs
LRUCache<string, int> cache = new LRUCache<string, int>(3);
cache.Put("a", 1);
cache.Put("b", 2);
cache.Put("c", 3);
Console.WriteLine(cache.Get("a")); // 1
```