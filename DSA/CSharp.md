# C-Sharp

## Boilerplate

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class Program
{
    static void Main()
    {
        string input = Console.ReadLine()!;
        Console.WriteLine("output");
    }
}
```

## Primitives & Numeric Traps

```csharp
int    i = 0;       // 32-bit   -2,147,483,648 .. 2,147,483,647  (~2.1e9)
long   l = 0L;      // 64-bit   ±9,223,372,036,854,775,807        (~9.2e18)
double d = 0.0;     // 64-bit IEEE 754, ~15–17 significant digits
decimal m = 0m;     // 128-bit, exact for base-10 fractions
char   c = 'a';     // 16-bit UTF-16
bool   b = true;
var    x = 5;       // inferred at compile-time — still statically typed
```

### Limits

```csharp
int.MinValue   // -2_147_483_648
int.MaxValue   //  2_147_483_647
long.MinValue  // -9_223_372_036_854_775_808
long.MaxValue  //  9_223_372_036_854_775_807
```

## Casting & Parsing

```csharp
int    x = (int)3.9;                    // 3 — truncates toward zero
long   y = (long)i * i;                 // cast BEFORE multiplying to avoid int overflow
int    n = int.Parse("42");             // throws FormatException on bad input
bool  ok = int.TryParse(s, out int v);  // preferred: returns false, never throws
string s = n.ToString();
double d = double.Parse("3.14");

int  digit = c - '0';                   // '7' → 7
char back  = (char)(digit + '0');       // 7 → '7'
int  idx   = c - 'a';                   // 'c' → 2  (lowercase letter → 0-based index)
```

## Control Flow

```csharp
if (cond) { } else if (cond2) { } else { }
var x = cond ? a : b;                   // ternary

switch (x) { case 1: ...; break; default: ...; break; }

for (int i = 0; i < n; i++) { }
for (int i = n - 1; i >= 0; i--) { }
foreach (var x2 in list) { }           // never add/remove during iteration
while (cond) { }
do { } while (cond);
break; continue;
```

## Tuples

```cs
(int, int) p = (2, 5);
p.Item1;                                // unnamed access

var t = (x: 5, y: 10);                  // named fields — preferred
t.x;

var (mn, mx) = MinMax(arr);             // deconstruct to separate vars
(a, b) = (b, a);                        // swap — no temp variable needed
```

## Methods

```csharp
// Methods
static int    Add(int a, int b) => a + b;                       // expression-bodied
static void   Swap(ref int a, ref int b) { (a, b) = (b, a); }   // ref: mutates caller
static bool   Try(out int r) { r = 5; return true; }            // out: must set before return
static (int, int) MinMax(int[] a) => (a.Min(), a.Max());        // tuple return

// Lambdas
Func<int, int>      square = x => x * x;
Comparison<int>     desc   = (x, y) => y.CompareTo(x);
```

## Classes

```csharp
class Node                      // reference type — assignment copies the reference
{
    public int Val;
    public Node? Next;
    public Node(int v) { Val = v; }
}

struct Point                    // value type — assignment copies the whole struct
{
    public int X, Y;
    public Point(int x, int y) { X = x; Y = y; }
}

record Point2(int X, int Y);   // immutable, value-equality by default (C# 9+)
```

## Arrays

### Create & initialise

```csharp
int[] a = new int[n];               // all zeros
int[] c = [1, 2, 3];                // collection expression (C# 12+)

int len = a.Length;                 // arrays → Length; collections → Count
```

### Common operations

```csharp
Array.Sort(a);                                      // in-place ascending (IntroSort)
Array.Sort(a, (x, y) => y.CompareTo(x));            // descending
Array.Reverse(a);
Array.Fill(a, -1);
int bs  = Array.BinarySearch(a, 5);                 // MUST be sorted; bs < 0 → ~bs = insertion point
```

### 2D: rectangular vs jagged

```csharp
// Rectangular int[,] — single allocation, O(1) access, but LINQ won't work on rows
int[,] grid = new int[rows, cols];
grid[i, j] = 1;
int r = grid.GetLength(0), c2 = grid.GetLength(1);

// Jagged int[][] — rows are independent arrays; LINQ works, rows can be sorted/reversed
int[][] jag = new int[rows][];
for (int i = 0; i < rows; i++) jag[i] = new int[cols];   // each row needs its own allocation
jag[i][j] = 1;
int nRows = jag.Length, nCols = jag[0].Length;

Array.Sort(jag[i]);                    // sort one row
Array.Reverse(jag[i]);                 // reverse one row
```

### String

```csharp
string s = "hello";
string fromArr = new string(arr2);
int len = s.Length;
char c = s[0];                             // read-only index — s[0]='x' won't compile
string sub = s.Substring(1, 3);            // (startIndex, length) → "ell"  O(k)
bool has = s.Contains("ell");
int  idx = s.IndexOf('l');                 // first match; -1 if not found
int  last = s.LastIndexOf('l');
bool sw = s.StartsWith("he"), ew = s.EndsWith("lo");
string rep = s.Replace("l", "L");
string up = s.ToUpper(), lo2 = s.ToLower();
string trimmed = s.Trim();
string[] parts = s.Split(',');
string joined = string.Join(",", parts);
string cat = string.Concat("a", "b", "c");
string rev2 = new string(s.Reverse().ToArray());    // LINQ Reverse on IEnumerable<char>
char[] arr2 = s.ToCharArray();              // mutable copy

// Comparison
bool eq  = s == t;                          // value equality — NOT reference (unlike Java!)
bool eqi = string.Equals(s, t, StringComparison.OrdinalIgnoreCase);
int  lex = string.Compare(s, t, StringComparison.Ordinal);
```

### StringBuilder

```csharp
var sb = new StringBuilder();
sb.Append("hello");                 // O(1) amortised; overloads for int, char, etc.
sb.Append(42);
sb.AppendLine("world");
sb.Insert(0, "prefix");             // O(n) shift
sb.Remove(2, 3);                    // (startIndex, count) removes 3 chars
sb.Replace("l", "L");
char ch = sb[0];                    // index access
sb[0] = 'H';                        // mutable
int slen = sb.Length;
sb.Length = 0;                      // fast clear — reuses the buffer
string r2 = sb.ToString();
```

---

## Comparator

```cs
IComparer<int> desc = Comparer<int>.Create((a, b) => b.CompareTo(a));
```

### Math quick-reference

```csharp
Math.Max(a, b); Math.Min(a, b);
Math.Abs(x);
Math.Sqrt(x); Math.Pow(2, 10);          // returns double; cast back: (int)Math.Pow(2,10)
Math.Floor(x); Math.Ceiling(x); Math.Round(x);
Math.Log2(x); Math.Log10(x);
```

---

## Collections — Detail

### List\<T\> — dynamic array, O(1) amortised append

```csharp
var list = new List<int>();
var list3 = new List<int> { 1, 2, 3 };
var list4 = new List<int>(sourceArray);         // copy from array or IEnumerable

list.Add(1);                                    // O(1) amortised, at end
list.AddRange(new[] { 2, 3, 4 });               // bulk append
list.Insert(0, 9);                              // O(n) — shifts right
list.InsertRange(0, new[] { 7, 8 });

list[0] = 5;                                    // O(1) set
list[^1]                                        // Last Node

list.RemoveAt(0);                               // O(n) — by index
list.Remove(5);                                 // O(n) — removes first match
list.RemoveAll(x => x < 0);                    // O(n) — remove all matching

int cnt = list.Count;
bool hasx = list.Contains(5);                  // O(n)
int idxl = list.IndexOf(5);                    // O(n), -1 if not found

list.Sort();                                    // in-place IntroSort
list.Sort((x, y) => y.CompareTo(x));           // descending
list.Reverse();                                 // in-place
List<int> sub2 = list.GetRange(1, 3);          // (index, count) — new list

int bsResult = list.BinarySearch(5);           // MUST be sorted; <0 → ~result = insertion point

int[] toArr = list.ToArray();
```

### Stack\<T\> — LIFO

```csharp
var st = new Stack<int>();
var st2 = new Stack<int>(source);       // Reversed order!
st.Push(1);                             // O(1)
int top2 = st.Peek();                   // throws if empty
int popped = st.Pop();                  // O(1), throws if empty
bool ok2 = st.TryPop(out int xp);      // safe — returns false when empty
bool ok3 = st.TryPeek(out int xk);
int stCnt = st.Count;
foreach (var xi in st) { }             // enumerates top → bottom (surprising!)
```

### Queue\<T\> — FIFO

```csharp
var q = new Queue<int>();
q.Enqueue(1);                           // add at back, O(1)
int front2 = q.Peek();                  // throws if empty
int d2 = q.Dequeue();                   // remove from front, O(1), throws if empty
bool qok2 = q.TryDequeue(out int yq);   // safe
bool qok3 = q.TryPeek(out int yk);
int qCnt = q.Count;
```

### LinkedList\<T\> — doubly linked, O(1) at both ends

```csharp
var ll = new LinkedList<int>();         // use as a deque when you need O(1) both ends
ll.AddFirst(1);                         // O(1)
ll.AddLast(2);                          // O(1)
int firstV = ll.First!.Value;
int lastV  = ll.Last!.Value;
LinkedListNode<int> ln = ll.First!;
var nextNode = ln.Next;
var prevNode = ln.Previous;
ll.RemoveFirst();                       // O(1)
ll.RemoveLast();                        // O(1)
int llCnt = ll.Count;
foreach (var xi2 in ll) { }            // first → last
```

### HashSet\<T\> — unordered, O(1) average

```csharp
var set = new HashSet<int>();
var set2 = new HashSet<int>(sourceArray);       // construct from array — deduplicates

bool added = set.Add(1);                        // returns false if already present — dedupe idiom
bool has2 = set.Contains(1);                   // O(1) average
set.Remove(1);
int setCnt = set.Count;
```

### SortedSet\<T\> — sorted, no duplicates, O(log n)

```csharp
var ss = new SortedSet<int>();
var ssDesc = new SortedSet<int>(Comparer<int>.Create((a, b) => b.CompareTo(a)));

ss.Add(5); ss.Remove(5);
int ssMin = ss.Min; int ssMax = ss.Max;         // O(log n)
bool ssCont = ss.Contains(3);
```

### Dictionary\<K,V\> — unordered, O(1) average

```csharp
var map = new Dictionary<string, int>();
var map = new Dictionary<(int, int), string>();        // tuple key — works out of the box

map["a"] = 1;                                           // add or overwrite
int mv = map["a"];                                      // throws KeyNotFoundException if missing
bool hasK = map.ContainsKey("a");
map.Remove("a");

// Safe reads
bool got = map.TryGetValue("a", out int val2);

// Frequency-counting idiom
map[key] = map.GetValueOrDefault(key) + 1;

// Iterate (insertion order not guaranteed)
foreach (var kv in map) { _ = kv.Key; _ = kv.Value; }
foreach (var (k, v) in map) { }                        // deconstruct KeyValuePair

// Convert
List<int>    vals  = map.Values.ToList();
List<string> keys2 = map.Keys.ToList();
```

### SortedDictionary\<K,V\> — sorted by key, O(log n)

```csharp
var sd = new SortedDictionary<int, int>();              // iterates in ascending key order
sd[1] = 10; sd[2] = 20;
int sdFirst = sd.Keys.First();                          // smallest key
int sdLast  = sd.Keys.Last();                           // largest key
```

### SortedList\<K,V\> — sorted array-backed, O(log n) lookup, O(n) insert

```csharp
var sl = new SortedList<int, int>();    // like SortedDictionary but random access by index
int valAtIdx = sl.Values[0];
int keyAtIdx = sl.Keys[0];
```

### PriorityQueue\<E,P\> — min-heap (.NET 6+)

```csharp
var pq = new PriorityQueue<int, int>();                 // (element, priority), min-priority leaves first
var maxPq = new PriorityQueue<int, int>(
    Comparer<int>.Create((a, b) => b.CompareTo(a)));    // max-heap

pq.Enqueue(1, 3);                   // element=1, priority=3
pq.Enqueue(2, -2);                  // -2 dequeues before 3
int topE = pq.Peek();               // look at element with lowest priority; throws if empty
int deqE = pq.Dequeue();            // O(log n)
bool deqOk = pq.TryDequeue(out int el, out int pri);   // safe
bool peekOk = pq.TryPeek(out int el2, out int pri2);
int pqCnt = pq.Count;
```

---

## Graph Representation

```csharp
// Adjacency list — sparse graphs, O(V + E) space
List<int>[] g = new List<int>[n];
for (int i = 0; i < n; i++) g[i] = new List<int>();
g[u].Add(v); g[v].Add(u);          // undirected

// Weighted adjacency list
List<(int to, int w)>[] wg = new List<(int, int)>[n];
for (int i = 0; i < n; i++) wg[i] = new List<(int, int)>();
wg[u].Add((v, w));
foreach (var (to, w2) in wg[u]) { }

// Adjacency matrix — dense graphs, O(1) edge lookup, O(V²) space
int[,] mat = new int[n, n];
mat[u, v] = 1;
```

### Conversions

```text
IEnumerable<T>
     ├──→ .ToList()
     ├──→ .ToArray()
     ├──→ new HashSet<T>(source)
     ├──→ new Stack<T>(source)     // reversed!
     └──→ new Queue<T>(source)

string   → char[]       : s.ToCharArray()
char[]   → string       : new string(arr)
int[]    → List<int>    : arr.ToList()
List<T>  → T[]          : list.ToArray()

Dictionary → keys       : dict.Keys          (ICollection<K>)
Dictionary → values     : dict.Values        (ICollection<V>)
Dictionary → KVP list   : dict.ToList()

Anything → Dictionary   : source.ToDictionary(keySelector, valueSelector)
Anything → Lookup       : source.ToLookup(keySelector)   // allows duplicate keys
```

### Collection Memory Shortcuts

**Default rule:** collections usually use `new Type<T>()`, `.Count`, `.Contains(x)`,
`.Remove(x)`, and `foreach`. Memorise the exceptions below.

| Remember by shape            | Add                          | Read / Remove                             |
| ---------------------------- | ---------------------------- | ----------------------------------------- |
| **List / Set** — general bag | `Add(x)`                     | `Contains(x)` / `Remove(x)`               |
| **Stack** — top              | `Push(x)`                    | `Peek()` / `Pop()`                        |
| **Queue** — line             | `Enqueue(x)`                 | `Peek()` / `Dequeue()`                    |
| **LinkedList** — two ends    | `AddFirst/Last(x)`           | `First/Last.Value` / `RemoveFirst/Last()` |
| **Dictionary** — key → value | `map[k] = v`                 | `TryGetValue(k, out v)` / `Remove(k)`     |
| **PriorityQueue** — priority | `Enqueue(element, priority)` | `Peek()` / `Dequeue()`                    |

| Shortcut                             | Applies to                               | Exception / reminder                                            |
| ------------------------------------ | ---------------------------------------- | --------------------------------------------------------------- |
| `Length` for indexable buffers       | array, `string`, `StringBuilder`         | other collections use `Count`                                   |
| `[i]` for positional access          | array, `string`, `StringBuilder`, `List` | dictionary uses `[key]`; others have no index                   |
| `Contains(x)` for search             | most collections                         | dictionary: `ContainsKey(k)`; array: `Array.IndexOf`            |
| `Try...` avoids empty/missing errors | stack, queue, dictionary, priority queue | `TryPop`, `TryDequeue`, `TryGetValue`, `TryPeek`                |
| `Sort()` changes the object          | array, `List`                            | array uses `Array.Sort(a)`; LINQ `OrderBy` returns new sequence |
