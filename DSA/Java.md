# Java

Target: **Java 17+**. Examples are independent snippets.

## Boilerplate

```java
import java.util.*;
import java.util.function.*;

public class Main {
    public static void main(String[] args) {
        Scanner scanner = new Scanner(System.in);
        String input = scanner.nextLine();
        System.out.println("output");
    }
}
```

## Primitives & Numeric Traps

```java
int     i = 0;       // 32-bit   -2,147,483,648 .. 2,147,483,647  (~2.1e9)
long    l = 0L;      // 64-bit   -9,223,372,036,854,775,808 .. 9,223,372,036,854,775,807
float   f = 0.0f;    // 32-bit IEEE 754, ~6-7 significant digits
double  d = 0.0;     // 64-bit IEEE 754, ~15-17 significant digits
char    c = 'a';     // 16-bit UTF-16 code unit
boolean b = true;
var     x = 5;       // local type inference - still statically typed (Java 10+)
```

Java has no primitive `decimal`. Use `BigDecimal` for exact base-10 arithmetic.

### Limits

```java
Integer.MIN_VALUE   // -2_147_483_648
Integer.MAX_VALUE   //  2_147_483_647
Long.MIN_VALUE      // -9_223_372_036_854_775_808L
Long.MAX_VALUE      //  9_223_372_036_854_775_807L
```

## Casting & Parsing

```java
int x = (int) 3.9;                        // 3 - truncates toward zero
long y = (long) i * i;                    // cast BEFORE multiplying to avoid int overflow
int n = Integer.parseInt("42");           // throws NumberFormatException on bad input
long value = Long.parseLong("42");
String s = Integer.toString(n);            // or String.valueOf(n)
double d = Double.parseDouble("3.14");

int digit = c - '0';                      // '7' -> 7
char back = (char) (digit + '0');          // 7 -> '7'
int index = c - 'a';                      // 'c' -> 2
```

Java has no built-in `TryParse`; catch `NumberFormatException` only when invalid input is expected.

## Control Flow

```java
if (condition) { } else if (condition2) { } else { }
var result = condition ? a : b;            // ternary

switch (x) {
    case 1 -> System.out.println("one");
    default -> System.out.println("other");
}

for (int index = 0; index < n; index++) { }
for (int index = n - 1; index >= 0; index--) { }
for (int item : list) { }                  // do not structurally modify while iterating
while (condition) { }
do { } while (condition);
break; continue;
```

## Tuples

Java has no built-in tuple type. Use a small `record` when fields belong together.

```java
record Point(int x, int y) { }
record MinMax(int min, int max) { }

Point point = new Point(2, 5);
int x = point.x();

MinMax range = minMax(array);
int min = range.min();
int max = range.max();

int temp = a;                              // swap needs a temporary variable
a = b;
b = temp;
```

For temporary two-value DSA entries, `int[]`, `Map.Entry<K, V>`, or a purpose-named record can also work.

## Methods

```java
static int add(int a, int b) {
    return a + b;
}

static MinMax minMax(int[] array) {
    int min = Arrays.stream(array).min().orElseThrow();
    int max = Arrays.stream(array).max().orElseThrow();
    return new MinMax(min, max);
}

// Java is always pass-by-value. Return changed primitive values instead of using ref/out.
static int increment(int value) {
    return value + 1;
}

IntUnaryOperator square = value -> value * value;
Comparator<Integer> descending = (left, right) -> Integer.compare(right, left);
```

## Classes

```java
class Node {                              // reference type; assignment copies the reference
    int val;
    Node next;

    Node(int val) {
        this.val = val;
    }
}

record Point(int x, int y) { }            // concise immutable data carrier with value equality
```

Java has no general-purpose value-type `struct`. Primitive values are copied; class and array variables hold references.

## Arrays

### Create & initialise

```java
int[] array = new int[n];                 // all zeros
int[] values = { 1, 2, 3 };

int length = array.length;                // arrays -> length; collections -> size()
```

### Common operations

```java
Arrays.sort(array);                       // in-place ascending
Arrays.fill(array, -1);
int result = Arrays.binarySearch(array, 5); // MUST be sorted; result < 0 -> -result - 1 insertion point

// Primitive arrays cannot use a comparator. Use Integer[] for descending sort.
Integer[] boxed = { 1, 2, 3 };
Arrays.sort(boxed, Comparator.reverseOrder());

// No Arrays.reverse for primitive arrays: swap from both ends.
for (int left = 0, right = array.length - 1; left < right; left++, right--) {
    int temp = array[left];
    array[left] = array[right];
    array[right] = temp;
}
```

### 2D: rectangular vs jagged arrays

```java
// Java 2D arrays are arrays of row references; rectangular dimensions are convenient syntax.
int[][] grid = new int[rows][cols];
grid[i][j] = 1;
int rowCount = grid.length;
int columnCount = grid[0].length;

// Rows may have different lengths.
int[][] jagged = new int[rows][];
for (int row = 0; row < rows; row++) jagged[row] = new int[row + 1];

Arrays.sort(grid[i]);
```

### String

```java
String s = "hello";
String fromArray = new String(charArray);
int length = s.length();
char c = s.charAt(0);                      // read-only; String is immutable
String sub = s.substring(1, 4);            // [start, end) -> "ell", O(k)
boolean has = s.contains("ell");
int index = s.indexOf('l');                // first match; -1 if not found
int last = s.lastIndexOf('l');
boolean starts = s.startsWith("he"), ends = s.endsWith("lo");
String replaced = s.replace("l", "L");
String upper = s.toUpperCase(Locale.ROOT);
String lower = s.toLowerCase(Locale.ROOT);
String trimmed = s.trim();
String[] parts = s.split(",", -1);         // split argument is a regex; -1 keeps trailing empties
String joined = String.join(",", parts);
String concatenated = String.join("", "a", "b", "c");
String reversed = new StringBuilder(s).reverse().toString();
char[] charArray = s.toCharArray();         // mutable copy

// Comparison
boolean equal = s.equals(t);                // use equals, not ==, for contents
boolean equalIgnoreCase = s.equalsIgnoreCase(t);
int lexicographic = s.compareTo(t);
```

### StringBuilder

```java
var builder = new StringBuilder();
builder.append("hello");                   // O(1) amortised per character
builder.append(42);
builder.append("world").append('\n');
builder.insert(0, "prefix");               // O(n) shift
builder.delete(2, 5);                       // [start, end) removes indices 2, 3, 4
builder.replace(0, 2, "Hi");
char ch = builder.charAt(0);
builder.setCharAt(0, 'H');                  // mutable
int length = builder.length();
builder.setLength(0);                       // fast clear; reuses the buffer
String result = builder.toString();
```

---

## Comparator

```java
Comparator<Integer> descending = (a, b) -> Integer.compare(b, a);
Comparator<int[]> byFirstThenSecond = Comparator
    .comparingInt((int[] pair) -> pair[0])
    .thenComparingInt(pair -> pair[1]);
```

Prefer `Integer.compare(a, b)` over `a - b`, which can overflow.

### Math quick-reference

```java
Math.max(a, b); Math.min(a, b);
Math.abs(x);
Math.sqrt(x); Math.pow(2, 10);              // returns double; cast back: (int) Math.pow(2, 10)
Math.floor(x); Math.ceil(x); Math.round(x);
Math.log(x); Math.log10(x);                 // log is natural log; no Math.log2
Math.floorMod(value, modulus);              // non-negative result when modulus is positive
```

---

## Collections - Detail

Java generics require reference types: use `Integer`, not `int`, inside collections.

### ArrayList\<T\> - dynamic array, O(1) amortised append

```java
List<Integer> list = new ArrayList<>();
List<Integer> list2 = new ArrayList<>(List.of(1, 2, 3));
List<Integer> list3 = new ArrayList<>(sourceCollection);

list.add(1);                                // O(1) amortised, at end
list.addAll(List.of(2, 3, 4));              // bulk append
list.add(0, 9);                             // O(n) - shifts right
list.addAll(0, List.of(7, 8));

list.set(0, 5);                             // O(1) set
int first = list.get(0);
int last = list.get(list.size() - 1);

list.remove(0);                             // O(n) - removes by index
list.remove(Integer.valueOf(5));            // O(n) - removes first matching value
list.removeIf(value -> value < 0);

int count = list.size();
boolean has = list.contains(5);             // O(n)
int index = list.indexOf(5);                // O(n), -1 if not found

list.sort(Comparator.naturalOrder());
list.sort(Comparator.reverseOrder());
Collections.reverse(list);
List<Integer> sub = new ArrayList<>(list.subList(1, 4)); // [from, to), independent copy

int result = Collections.binarySearch(list, 5); // MUST be sorted
Integer[] array = list.toArray(Integer[]::new);
```

**Overload trap:** `remove(2)` removes index 2; `remove(Integer.valueOf(2))` removes value 2.

### Deque\<T\> as a stack - LIFO

```java
Deque<Integer> stack = new ArrayDeque<>();
stack.push(1);                              // add at top, O(1)
int top = stack.peek();                     // returns null if empty; unboxing null throws
int popped = stack.pop();                   // O(1), throws if empty
Integer safePop = stack.pollFirst();        // null if empty
int count = stack.size();
for (int value : stack) { }                 // top -> bottom
```

Prefer `ArrayDeque` over the legacy `Stack` class.

### Queue\<T\> - FIFO

```java
Queue<Integer> queue = new ArrayDeque<>();
queue.offer(1);                             // add at back, O(1)
Integer front = queue.peek();               // null if empty
Integer removed = queue.poll();             // remove front, O(1); null if empty
int count = queue.size();
```

`add`/`remove`/`element` throw on failure; `offer`/`poll`/`peek` use a boolean or `null` result.

### Deque\<T\> - O(1) at both ends

```java
Deque<Integer> deque = new ArrayDeque<>();
deque.offerFirst(1);
deque.offerLast(2);
Integer first = deque.peekFirst();
Integer last = deque.peekLast();
Integer removedFirst = deque.pollFirst();
Integer removedLast = deque.pollLast();
int count = deque.size();
for (int value : deque) { }                 // first -> last
```

`LinkedList` also implements `Deque`, but `ArrayDeque` is usually the better stack/queue choice. Java collections do not expose linked-list nodes.

### HashSet\<T\> - unordered, O(1) average

```java
Set<Integer> set = new HashSet<>();
Set<Integer> set2 = new HashSet<>(sourceCollection); // deduplicates

boolean added = set.add(1);                 // false if already present
boolean has = set.contains(1);              // O(1) average
set.remove(1);
int count = set.size();
```

### TreeSet\<T\> - sorted, no duplicates, O(log n)

```java
NavigableSet<Integer> set = new TreeSet<>();
NavigableSet<Integer> descendingSet = new TreeSet<>(Comparator.reverseOrder());

set.add(5); set.remove(5);
int min = set.first();
int max = set.last();
Integer floor = set.floor(3);
Integer ceiling = set.ceiling(3);
boolean contains = set.contains(3);
```

A comparator defines both ordering and uniqueness: values comparing as `0` are treated as duplicates.

### HashMap\<K,V\> - unordered, O(1) average

```java
Map<String, Integer> map = new HashMap<>();
Map<Point, String> coordinateMap = new HashMap<>(); // records work well as compound keys

map.put("a", 1);                           // add or overwrite
Integer value = map.get("a");              // null if missing
boolean hasKey = map.containsKey("a");
map.remove("a");

// Safe read with a fallback
int valueOrZero = map.getOrDefault("a", 0);

// Frequency-counting idiom
map.put(key, map.getOrDefault(key, 0) + 1);
// Equivalent: map.merge(key, 1, Integer::sum);

// Iterate; HashMap order is not guaranteed
for (Map.Entry<String, Integer> entry : map.entrySet()) {
    String key = entry.getKey();
    int count = entry.getValue();
}

List<Integer> values = new ArrayList<>(map.values());
List<String> keys = new ArrayList<>(map.keySet());
```

### TreeMap\<K,V\> - sorted by key, O(log n)

```java
NavigableMap<Integer, Integer> map = new TreeMap<>();
map.put(1, 10); map.put(2, 20);
int firstKey = map.firstKey();
int lastKey = map.lastKey();
Map.Entry<Integer, Integer> floor = map.floorEntry(3);
Map.Entry<Integer, Integer> ceiling = map.ceilingEntry(3);
```

### SortedList equivalent

Java has no direct equivalent of C# `SortedList<K,V>`. Use `TreeMap` for sorted key operations or a sorted `ArrayList` of entries for indexed access.

```java
List<Map.Entry<Integer, Integer>> entries = new ArrayList<>();
entries.add(Map.entry(2, 20));
entries.add(Map.entry(1, 10));
entries.sort(Map.Entry.comparingByKey());

int keyAtIndex = entries.get(0).getKey();
int valueAtIndex = entries.get(0).getValue();
```

### PriorityQueue\<T\> - min-heap

```java
PriorityQueue<Integer> minHeap = new PriorityQueue<>();
PriorityQueue<Integer> maxHeap = new PriorityQueue<>(Comparator.reverseOrder());

minHeap.offer(3);
minHeap.offer(-2);                          // -2 leaves before 3
Integer top = minHeap.peek();               // null if empty
Integer removed = minHeap.poll();           // O(log n), null if empty
int count = minHeap.size();

record Entry(int element, int priority) { }
PriorityQueue<Entry> byPriority = new PriorityQueue<>(Comparator.comparingInt(Entry::priority));
byPriority.offer(new Entry(1, 3));
byPriority.offer(new Entry(2, -2));
Entry next = byPriority.poll();
```

Iteration is **not** sorted; repeatedly `poll()` to consume heap order.

---

## Graph Representation

```java
// Adjacency list - sparse graphs, O(V + E) space
List<List<Integer>> graph = new ArrayList<>();
for (int vertex = 0; vertex < n; vertex++) graph.add(new ArrayList<>());
graph.get(u).add(v);
graph.get(v).add(u);                        // undirected

// Weighted adjacency list
record Edge(int to, int weight) { }
List<List<Edge>> weightedGraph = new ArrayList<>();
for (int vertex = 0; vertex < n; vertex++) weightedGraph.add(new ArrayList<>());
weightedGraph.get(u).add(new Edge(v, weight));
for (Edge edge : weightedGraph.get(u)) {
    int to = edge.to();
    int edgeWeight = edge.weight();
}

// Adjacency matrix - dense graphs, O(1) edge lookup, O(V^2) space
int[][] matrix = new int[n][n];
matrix[u][v] = 1;
```

### Conversions

```text
Collection<T>
     |--> new ArrayList<>(source)
     |--> source.toArray(Type[]::new)
     |--> new HashSet<>(source)
     |--> new ArrayDeque<>(source)

String       -> char[]       : text.toCharArray()
char[]       -> String       : new String(array)
int[]        -> List<Integer>: Arrays.stream(array).boxed().toList()  (unmodifiable)
List<Integer>-> int[]        : list.stream().mapToInt(Integer::intValue).toArray()

Map -> keys                  : map.keySet()     (backed Set<K> view)
Map -> values                : map.values()     (backed Collection<V> view)
Map -> entries               : map.entrySet()   (backed Set<Entry<K,V>> view)

Stream -> mutable List       : stream.collect(Collectors.toCollection(ArrayList::new))
Stream -> Map                : stream.collect(Collectors.toMap(keyMapper, valueMapper))
Grouping                     : stream.collect(Collectors.groupingBy(keyMapper))
```

### Collection Memory Shortcuts

**Default rule:** collections usually use `new Type<>()`, `.size()`, `.contains(x)`, `.remove(x)`, and enhanced `for`. Memorise the exceptions below.

| Remember by shape            | Add                  | Read / Remove                           |
| ---------------------------- | -------------------- | --------------------------------------- |
| **List / Set** - general bag | `add(x)`             | `contains(x)` / `remove(x)`             |
| **Stack** - top              | `push(x)`            | `peek()` / `pop()`                      |
| **Queue** - line             | `offer(x)`           | `peek()` / `poll()`                     |
| **Deque** - two ends         | `offerFirst/Last(x)` | `peekFirst/Last()` / `pollFirst/Last()` |
| **Map** - key -> value       | `put(key, value)`    | `get(key)` / `remove(key)`              |
| **PriorityQueue** - priority | `offer(x)`           | `peek()` / `poll()`                     |

| Shortcut                   | Applies to                            | Exception / reminder                                        |
| -------------------------- | ------------------------------------- | ----------------------------------------------------------- |
| Length by container        | array: `.length`; String: `.length()` | collections and maps use `.size()`                          |
| Positional access          | array: `[i]`; String: `charAt(i)`     | list: `get(i)`; map: `get(key)`                             |
| `contains(x)` for search   | most collections                      | map: `containsKey(key)`; array: loop/binary search          |
| Safe empty reads           | queue, deque, priority queue          | `peek`/`poll` return `null`; beware primitive unboxing      |
| Sorting changes the object | arrays, mutable lists                 | `Arrays.sort(a)` / `list.sort(comparator)`                  |
| Equality                   | objects                               | use `.equals`; `==` compares references (except primitives) |
