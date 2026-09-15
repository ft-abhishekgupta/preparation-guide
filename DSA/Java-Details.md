# Java for DSA and LLD Interviews

Target: **Java 17+**, with Java 21 features explicitly marked. Examples are independent reference snippets, not one combined program. Prefer ordinary loops and standard collections; introduce abstractions and concurrency when the problem needs them.

## 1. Language Essentials

### 1.1 Imports and Simple Input

```java
import java.util.*;

public class Main {
    public static void main(String[] args) {
        Scanner scanner = new Scanner(System.in);
        int count = scanner.nextInt();
        int[] values = new int[count];
        for (int index = 0; index < count; index++) {
            values[index] = scanner.nextInt();
        }
        System.out.println(Arrays.toString(values));
    }
}
```

- Coding platforms usually supply the method signature; no input parser is needed.
- `Scanner` is sufficient for ordinary interview input. For large input, use `BufferedReader` with `StringTokenizer`; a custom byte parser is rarely worth memorising.
- `nextInt()` leaves the line separator unread; a following `nextLine()` may return an empty string.
- Imports used later: `java.io.*`, `java.math.*`, `java.time.*`, `java.util.concurrent.*`, `java.util.concurrent.atomic.*`, and `java.util.concurrent.locks.*`. Importing a package does not import its subpackages.

### 1.2 Types, Parsing, and Numeric Safety

| Type / operation           | Remember                                                                                       |
| -------------------------- | ---------------------------------------------------------------------------------------------- |
| `int`                      | Signed 32-bit; about +/-2.1 billion                                                            |
| `long`                     | Signed 64-bit; about +/-9.2 quintillion; use `L` for long literals                             |
| `double`                   | Floating point; not exact decimal arithmetic                                                   |
| `char`                     | One UTF-16 code unit, not necessarily a complete Unicode character                             |
| `boolean`                  | `true` / `false`; integers are not booleans                                                    |
| `Integer`, `Long`, etc.    | Boxed reference types; required by generics; can be `null`                                     |
| `var`                      | Local type inference, not dynamic typing                                                       |
| Limits                     | `Integer.MIN_VALUE`, `Integer.MAX_VALUE`, `Long.MAX_VALUE`                                     |
| Parse                      | `Integer.parseInt(text)`, `Long.parseLong(text)`; invalid input throws `NumberFormatException` |
| Format                     | `String.valueOf(value)`                                                                        |
| ASCII digit / letter index | `digitChar - '0'`, `letter - 'a'`; validate the expected range                                 |

```java
int value = 50_000;
long square = (long) value * value;
double ratio = (double) 5 / 2;
int truncated = (int) 3.9;
int digit = '7' - '0';
char digitChar = (char) (digit + '0');
```

- Cast **before** arithmetic: assigning an overflowing `int * int` result to `long` is too late. Integer overflow normally wraps; `Math.addExact` / `multiplyExact` throw instead.
- `5 / 2` is `2`; integer division truncates toward zero. `Math.abs(Integer.MIN_VALUE)` is still negative.
- Use `long` for sums, distances, products, and counts that can exceed `int`. Avoid adding to an infinity sentinel without checking it first.
- For a positive modulus, `Math.floorMod(value, modulus)` produces a non-negative result. Multiplication may still need a `long` intermediate.
- Use `BigInteger` when even `long` is insufficient. For money, use minor units in a `long` or `BigDecimal` created from a decimal string, with an explicit rounding policy.

### 1.3 Control Flow and Methods

```java
static long sumPositive(int[] values) {
    long total = 0;
    for (int value : values) {
        if (value <= 0) continue;
        total += value;
    }
    return total;
}

static String describe(int code) {
    return switch (code) {
        case 1 -> "ready";
        case 2 -> "busy";
        default -> "unknown";
    };
}
```

- Indexed loop: `for (int index = 0; index < size; index++)`; reverse: start at `size - 1` and check `index >= 0`.
- Use `while` for condition-driven loops; `break` exits the loop, `continue` skips to its next iteration.
- Ternary: `condition ? whenTrue : whenFalse`. Arrow-style switch cases do not fall through.
- Java is **always pass-by-value**. Mutating an object through a copied reference affects the caller; reassigning the parameter does not. There is no `ref` / `out`.
- Locals must be assigned before reading; fields and array elements have defaults (`0`, `false`, `null`).

## 2. DSA Data Structures and APIs

### 2.1 Arrays and Grids

```java
int[] values = { 3, 1, 2 };
int[] buffer = new int[10];
Arrays.fill(buffer, -1);
Arrays.sort(values);
int result = Arrays.binarySearch(values, 2);
int insertionPoint = result >= 0 ? result : -result - 1;
int[] copy = values.clone();
int[] slice = Arrays.copyOfRange(values, 0, 2);
boolean equal = Arrays.equals(values, copy);

int[][] grid = new int[3][4];
grid[1][2] = 7;
for (int[] row : grid) Arrays.fill(row, -1);
```

- Array length: `values.length`. A grid is an array of rows; use `grid.length` and, only if a row exists, `grid[0].length`. Rows can have different lengths or be `null`.
- Ranges such as `copyOfRange(from, to)` are end-exclusive. `binarySearch` requires the same ordering used for sorting; duplicate matches are not necessarily the first match.
- A 2D array's `clone()` copies only the outer array. Clone each row for an independent grid; use `Arrays.deepEquals` / `deepToString` for nested contents.
- There is no `Arrays.reverse(int[])`; swap endpoints in a two-pointer loop. Primitive arrays do not accept custom comparators.

### 2.2 Strings and StringBuilder

```java
String text = "hello";
char first = text.charAt(0);
String middle = text.substring(1, 4);
int position = text.indexOf('l');
boolean contains = text.contains("ell");
String replaced = text.replace("l", "L");
char[] letters = text.toCharArray();
String restored = new String(letters);
String[] parts = "a,b,".split(",", -1);
String joined = String.join(",", parts);

StringBuilder builder = new StringBuilder();
builder.append("answer=").append(42).append('\n');
builder.setCharAt(0, 'A');
String output = builder.toString();
String reversed = new StringBuilder(text).reverse().toString();
```

- `substring(1, 4)` is `"ell"`: **start inclusive, end exclusive**. `indexOf` returns `-1` if absent.
- `String` is immutable. Use `StringBuilder` for repeated concatenation; appending a single character is amortised O(1), appending k characters is O(k), and `toString()` is O(n).
- String and builder length: `.length()`. Builder editing: `insert(index, text)`, `delete(from, to)`, `setLength(0)`; inserts/deletes may shift O(n) characters.
- `split` takes a **regex**: use `"\\."` for a literal dot. `-1` retains trailing empty fields. `replace` is literal; `replaceAll` is regex-based.
- Use `equals` for contents, `compareTo` for lexicographic order, `Objects.equals(left, right)` if either may be null. For locale-independent case conversion, use `Locale.ROOT`.
- `charAt` and indices operate on UTF-16 code units. Clarify whether an algorithm assumes ASCII/lowercase letters or needs Unicode code points (`text.codePoints()`).

### 2.3 Choosing a Collection

| Need                           | Default choice                            | Typical cost / caveat                                          |
| ------------------------------ | ----------------------------------------- | -------------------------------------------------------------- |
| Indexed sequence               | `ArrayList<T>`                            | O(1) get/set; amortised O(1) append; O(n) middle insert/remove |
| Stack / queue / deque          | `ArrayDeque<T>`                           | Amortised O(1) end operations; no null elements                |
| Membership / uniqueness        | `HashSet<T>`                              | Expected O(1) add/contains/remove with suitable hashing        |
| Key to value                   | `HashMap<K,V>`                            | Expected O(1) get/put/remove with suitable hashing             |
| Sorted membership / neighbours | `TreeSet<T>`                              | O(log n) add/remove/search; comparator determines uniqueness   |
| Sorted key lookup / neighbours | `TreeMap<K,V>`                            | O(log n) get/put/remove                                        |
| Repeated min/max               | `PriorityQueue<T>`                        | O(1) peek; O(log n) offer/poll; O(n) contains/remove-by-value  |
| Insertion-ordered map / set    | `LinkedHashMap<K,V>` / `LinkedHashSet<T>` | Hash lookup with predictable encounter order                   |

Most collections use `.size()`, `.isEmpty()`, `.contains(value)`, and enhanced `for`. Maps use `.containsKey(key)` and `.entrySet()` for iteration. Declare against an interface where useful, such as `List<Integer> values = new ArrayList<>();`.

`LinkedList` has O(n) indexed access and does not expose nodes for constant-time arbitrary-node removal. Prefer `ArrayDeque` for stacks/queues; implement your own doubly linked nodes when an LRU design needs direct node handles. Avoid legacy `Stack`, `Vector`, and `Hashtable` as defaults.

For an LRU cache when library helpers are allowed, an access-ordered `LinkedHashMap` (`new LinkedHashMap<>(16, 0.75f, true)`) with `removeEldestEntry` is simpler. Its reads can reorder entries, so concurrent use still needs coordination.

### 2.4 ArrayList

```java
List<Integer> values = new ArrayList<>(List.of(3, 1, 2));
values.add(4);
values.add(0, 9);
values.set(0, 5);
int first = values.get(0);
int last = values.get(values.size() - 1);
values.remove(0);
values.remove(Integer.valueOf(2));
values.removeIf(value -> value < 0);
values.sort(Comparator.naturalOrder());
Collections.reverse(values);
List<Integer> copy = new ArrayList<>(values.subList(0, 2));
```

**Critical overload:** `remove(2)` removes the element at index 2; `remove(Integer.valueOf(2))` removes the first matching value. `subList` is a backed view, so wrap it in `new ArrayList<>(...)` for an independent copy.

### 2.5 Stack, Queue, and Deque

```java
Deque<Integer> stack = new ArrayDeque<>();
stack.push(10);
Integer top = stack.peek();
int popped = stack.pop();

Deque<Integer> queue = new ArrayDeque<>();
queue.offerLast(10);
Integer front = queue.peekFirst();
Integer removed = queue.pollFirst();
```

| Mode                | Add                      | Read               | Remove             |
| ------------------- | ------------------------ | ------------------ | ------------------ |
| Stack               | `push(value)`            | `peek()`           | `pop()`            |
| FIFO queue          | `offerLast(value)`       | `peekFirst()`      | `pollFirst()`      |
| Deque at either end | `offerFirst/Last(value)` | `peekFirst/Last()` | `pollFirst/Last()` |

`peek` / `poll` return `null` when empty; `pop`, `getFirst`, and `removeFirst` throw. Keep nullable results as `Integer`: assigning null to `int` throws during unboxing. Iterating an `ArrayDeque` visits front to back, which is top to bottom when used as a stack.

### 2.6 Sets and Maps

```java
Set<Integer> seen = new HashSet<>();
boolean firstVisit = seen.add(7);
boolean present = seen.contains(7);
seen.remove(7);

Map<String, Integer> counts = new HashMap<>();
String key = "apple";
counts.put(key, counts.getOrDefault(key, 0) + 1);
for (Map.Entry<String, Integer> entry : counts.entrySet()) {
    String word = entry.getKey();
    int frequency = entry.getValue();
}
counts.remove(key);

Map<String, List<Integer>> positions = new HashMap<>();
positions.computeIfAbsent(key, ignored -> new ArrayList<>()).add(3);
```

- `add` returns false for an existing set element; `put` inserts or overwrites. `get` returns null for a missing mapping; `getOrDefault` does not replace an explicitly stored null.
- Hash collections do not promise iteration order. `keySet`, `values`, and `entrySet` are backed views, not snapshots.
- Do not structurally change ordinary collections during enhanced `for`. Use `removeIf`, explicit `Iterator.remove()` when supported, or a separate collection of changes. Fail-fast exceptions are bug detectors, not concurrency guarantees.

### 2.7 Ordered Sets and Maps

```java
TreeSet<Integer> values = new TreeSet<>(List.of(2, 5, 9));
int min = values.first();
int max = values.last();
Integer floor = values.floor(6);
Integer ceiling = values.ceiling(6);
Integer lower = values.lower(5);
Integer higher = values.higher(5);

TreeMap<Integer, String> bookings = new TreeMap<>();
bookings.put(10, "meeting");
Map.Entry<Integer, String> previous = bookings.floorEntry(12);
```

`floor` / `ceiling` include equality; `lower` / `higher` are strict. They return null when no neighbour exists. `first` / `last` and `firstKey` / `lastKey` throw on an empty collection. There is no efficient built-in k-th-element lookup in `TreeSet` / `TreeMap`.

### 2.8 Comparators and Priority Queues

```java
record Job(int id, long priority) { }

Comparator<Job> byPriority = Comparator.comparingLong(Job::priority)
    .thenComparingInt(Job::id);
PriorityQueue<Job> pending = new PriorityQueue<>(byPriority);
pending.offer(new Job(1, 30));
pending.offer(new Job(2, 10));
Job next = pending.poll();

PriorityQueue<Integer> maxHeap = new PriorityQueue<>(Comparator.reverseOrder());
```

- `Comparable<T>.compareTo` defines a type's natural ordering; `Comparator<T>` supplies an external ordering. Use `Integer.compare` / `Long.compare`, never subtraction that can overflow.
- Reuse the same comparator for list/object-array sorting and heap ordering. `reversed()` reverses the comparator built so far, including its tie-breakers.
- Heap iteration is **not sorted**. Repeated `poll()` is ordered but consumes the heap; equal-priority elements have no FIFO guarantee unless you add a sequence-number tie-breaker.
- A tree treats comparator result zero as a duplicate key/element. Include an ID tie-breaker when equal priorities must coexist.
- Never mutate fields participating in heap/tree ordering while the element is stored. There is no direct decrease-key API; in Dijkstra, insert the new distance and discard stale entries when polled.

### 2.9 Copies, Conversions, and Mutability

| Operation                                             | Meaning / trap                                                |
| ----------------------------------------------------- | ------------------------------------------------------------- |
| `new ArrayList<>(source)`                             | Mutable shallow copy                                          |
| `new HashSet<>(source)`                               | Mutable deduplicated copy                                     |
| `List.of(1, 2, 3)`                                    | Unmodifiable; rejects null                                    |
| `List.copyOf(source)`                                 | Unmodifiable snapshot of elements; rejects null; shallow      |
| `Collections.unmodifiableList(source)`                | Read-only view; changes through the original list are visible |
| `Arrays.asList(boxedArray)`                           | Fixed-size backed list; `set` works, add/remove do not        |
| `Arrays.asList(intArray)`                             | A one-element `List<int[]>`, not `List<Integer>`              |
| `Arrays.stream(values).boxed().toList()`              | `int[]` to unmodifiable `List<Integer>`                       |
| `list.stream().mapToInt(Integer::intValue).toArray()` | `List<Integer>` to `int[]`; null elements fail                |
| `list.toArray(Integer[]::new)`                        | `List<Integer>` to `Integer[]`                                |

Streams are lazy until a terminal operation and cannot be reused. Prefer loops for mutable algorithm state and early exits. Avoid `parallelStream()` as an automatic optimisation, especially with shared state or blocking work.

## 3. DSA Round Essentials

### 3.1 Graph Representation

Use a list of lists to avoid generic-array warnings. An unweighted graph uses `List<List<Integer>>`; add the reverse edge as well for an undirected graph.

```java
record Edge(int to, int weight) { }

int nodeCount = 4;
List<List<Edge>> graph = new ArrayList<>();
for (int node = 0; node < nodeCount; node++) {
    graph.add(new ArrayList<>());
}
graph.get(0).add(new Edge(1, 7));
for (Edge edge : graph.get(0)) {
    int neighbour = edge.to();
    int weight = edge.weight();
}
```

Adjacency lists take O(V + E) space; a matrix takes O(V^2) but gives O(1) edge lookup. BFS gives shortest paths in unweighted graphs; Dijkstra requires non-negative edge weights. Mark a BFS node visited **when enqueuing**, not when dequeuing.

### 3.2 Binary Search Boundaries

`Arrays.binarySearch` is enough for existence checks. For first occurrence, insertion boundaries, or answer-space search, keep an explicit interval invariant. This lower bound returns the first index with value >= target, or the array length; input must be ascending.

```java
static int lowerBound(int[] values, int target) {
    int low = 0;
    int high = values.length;
    while (low < high) {
        int middle = low + (high - low) / 2;
        if (values[middle] < target) {
            low = middle + 1;
        } else {
            high = middle;
        }
    }
    return low;
}
```

### 3.3 Math, Bits, and Complexity Traps

| Need              | Java / reminder                                                                                       |
| ----------------- | ----------------------------------------------------------------------------------------------------- |
| Min/max and roots | `Math.min`, `Math.max`, `Math.sqrt`; `Math.pow` returns double                                        |
| Set-bit count     | `Integer.bitCount(mask)` / `Long.bitCount(mask)`                                                      |
| Lowest set bit    | `mask & -mask`; clear it with `mask & (mask - 1)`                                                     |
| Test a bit        | `(mask & (1L << bit)) != 0`; keep bit in 0..63                                                        |
| Shifts            | `>>` sign-extends; `>>>` zero-fills; distances are masked to 5 bits for int and 6 for long            |
| Integer GCD       | Euclid's algorithm; Java has no primitive `Math.gcd`                                                  |
| Recursion         | Depth uses stack space and may cause `StackOverflowError`; no guaranteed tail-call optimisation       |
| Backtracking      | Undo each mutation; save paths with `new ArrayList<>(path)`, not the same mutable reference           |
| Memoisation       | Include every state dimension; distinguish an uncomputed state from a valid zero result               |
| Hidden work       | `contains` on a list/heap is O(n); copying/substrings cost O(k); repeated concatenation can be O(n^2) |

Prefer primitive arrays for dense states and frequency tables with small known alphabets. Use maps for sparse or unbounded keys. Boxing and per-node objects add memory overhead beyond the asymptotic complexity.

### 3.4 Problem-Solving Checklist

1. Confirm constraints, input validity, duplicate rules, and whether input mutation is allowed.
2. Name the invariant and choose a matching pattern: two pointers, sliding window, prefix sums, binary search, traversal, heap, greedy, DP, or union-find.
3. State time and space complexity, including sorting, copying, output, and recursion stack.
4. Test empty/singleton inputs, duplicates, all-equal data, negative/extreme values, sorted/reversed data, and disconnected/cyclic graphs where relevant.
5. Check overflow, index boundaries, stale shared fields across repeated calls, and accidental aliasing.

## 4. OOP and Java Object Semantics

### 4.1 The Four Pillars

| Concept       | Meaning                                                   | Interview application                                           |
| ------------- | --------------------------------------------------------- | --------------------------------------------------------------- |
| Encapsulation | Hide state and enforce invariants through operations      | `reserve()` / `cancel()`, not unrestricted status setters       |
| Abstraction   | Expose a contract without exposing implementation details | `FeePolicy` describes pricing, not its algorithm                |
| Inheritance   | Extend a type with an **is-a** relationship               | A subtype must remain usable wherever its base type is expected |
| Polymorphism  | Invoke different implementations through one contract     | A service accepts any implementation of `FeePolicy`             |

### 4.2 Interfaces, Abstract Classes, and Composition

| Construct      | Use when                                                                                                                |
| -------------- | ----------------------------------------------------------------------------------------------------------------------- |
| Interface      | You need a capability/contract and multiple implementations; supports default/static methods but no per-instance fields |
| Abstract class | Closely related subclasses share state, constructors, or partial implementation; only one superclass is allowed         |
| Composition    | An object **has-a** collaborator that can be replaced or tested independently                                           |
| Enum           | A small fixed set of domain values, such as `AVAILABLE`, `RESERVED`, `OCCUPIED`                                         |
| Record         | A compact data carrier with generated accessors, constructor, `equals`, `hashCode`, and `toString`                      |

Prefer composition for interchangeable behaviour. This example combines encapsulation, an interface, overriding, runtime polymorphism, and constructor injection. Amounts are in minor currency units, and hours are assumed to be whole billable hours.

```java
interface FeePolicy {
    long fee(int hours);
}

final class HourlyFee implements FeePolicy {
    private final long rate;

    HourlyFee(long rate) {
        if (rate < 0) throw new IllegalArgumentException("Negative rate");
        this.rate = rate;
    }

    @Override
    public long fee(int hours) {
        if (hours < 0) throw new IllegalArgumentException("Negative hours");
        return Math.multiplyExact(rate, hours);
    }
}

final class ParkingService {
    private final FeePolicy feePolicy;

    ParkingService(FeePolicy feePolicy) {
        this.feePolicy = Objects.requireNonNull(feePolicy);
    }

    long quote(int hours) {
        return feePolicy.fee(hours);
    }
}
```

### 4.3 Access, Modifiers, and Dispatch

| Keyword / concept | Remember                                                                                                  |
| ----------------- | --------------------------------------------------------------------------------------------------------- |
| `private`         | Encapsulated implementation; not accessible by unrelated callers                                          |
| No modifier       | Package-private; accessible in the same package                                                           |
| `protected`       | Same package plus subclasses; cross-package subclass access has receiver restrictions                     |
| `public`          | Public API, subject to enclosing type/module accessibility                                                |
| `static`          | Belongs to the class, not an instance; static methods are hidden, not dynamically overridden              |
| `final`           | Variable assigned once, method not overridable, or class not extendable; does not imply deep immutability |
| `this` / `super`  | Current instance / superclass access or constructor invocation                                            |
| Overloading       | Same name, different parameter lists; selected at compile time; return type alone is insufficient         |
| Overriding        | Subclass replaces an inherited instance method; selected at runtime; use `@Override`                      |

An override cannot reduce visibility or broaden checked exceptions. Constructors are not inherited; call another constructor using `this(...)` or the superclass using `super(...)`. Avoid calling overridable methods from constructors because subclass state may not yet be initialised.

### 4.4 Equality, Hashing, and Immutability

- Primitive `==` compares values; reference `==` compares identity. Never rely on boxed-integer caching for equality. Default `Object.equals` is also identity unless overridden.
- Override `equals` and `hashCode` together: equal objects **must** have equal hashes; a matching hash does not prove equality. Equality must be reflexive, symmetric, transitive, consistent, and false for null.
- Never change fields used by `equals` / `hashCode` while an object is a hash key. Prefer immutable IDs or value objects as keys.
- Records are reference types and only **shallowly immutable**. Their final fields may reference mutable lists/arrays; an array component still uses array identity equality unless customised.
- Use defensive copies on input and output. An unmodifiable collection protects its structure, not mutable elements. `final List<T>` alone does not prevent additions/removals.

```java
record Route(String id, List<String> stops) {
    Route {
        Objects.requireNonNull(id);
        stops = List.copyOf(stops);
    }
}
```

Here the list is protected and its `String` elements are immutable. Domain **entities** usually have identity across state changes; **value objects** are compared by their attributes. Choose equality accordingly rather than making every domain object a record.

### 4.5 Generics and Lambdas

- Generics are invariant: `List<Integer>` is not a `List<Number>`. Use wrappers rather than primitives and avoid raw types.
- **PECS:** producer `? extends T` when reading T values; consumer `? super T` when writing them. You cannot safely add a non-null element to `List<? extends Number>`.
- Type erasure means no `new T()` or `new T[]`; an arbitrary `Object` cannot be tested with `instanceof List<String>` because the element type is erased. `instanceof List<?>` checks only that it is a list. Prefer collection factories or an explicit `Class<T>` where needed.
- A functional interface has one abstract method and can accept a lambda. Know `Predicate<T>`, `Function<T,R>`, `Consumer<T>`, and `Supplier<T>` from `java.util.function`.
- Captured local variables must be final or effectively final. Their referenced objects may still be mutable, which matters when a lambda runs concurrently.

## 5. LLD Design Essentials

### 5.1 From Requirements to Classes

1. Clarify use cases, actors, scale, extensibility needs, and whether calls may be concurrent.
2. Identify entities, value objects, responsibilities, ownership, and invariants before naming patterns.
3. Define operations with inputs, results, and failure cases. Prefer domain commands over generic setters.
4. Model lifecycle transitions explicitly, such as `AVAILABLE -> RESERVED -> OCCUPIED`; decide who can perform each transition and what happens on retries.
5. Separate domain rules from persistence, notifications, and external APIs using small interfaces where substitution is useful.
6. Walk through one complete scenario, including cancellation, errors, and competing requests.
7. Test behaviour and invariants with deterministic dependencies such as a fake repository and an injected `Clock`.

In a class diagram, distinguish **is-a** inheritance, **has-a** composition/aggregation, and temporary dependencies. State cardinality (one-to-one, one-to-many). Composition expresses ownership of a part's lifecycle; aggregation is a shared association.

### 5.2 SOLID in Practice

| Principle             | Practical rule                                                                                     |
| --------------------- | -------------------------------------------------------------------------------------------------- |
| Single responsibility | Keep reasons to change separate; pricing, persistence, and notification need not live in one class |
| Open/closed           | Add a new policy implementation without rewriting the orchestration                                |
| Liskov substitution   | Subtypes preserve the contract; do not strengthen preconditions or weaken guarantees               |
| Interface segregation | Expose small role-specific interfaces rather than forcing irrelevant methods                       |
| Dependency inversion  | High-level rules depend on abstractions; inject concrete adapters at the application boundary      |

Prefer high cohesion and low coupling. Do not create an interface for every class or introduce a pattern without a concrete variation or responsibility to manage.

### 5.3 Patterns Worth Recognising

| Pattern   | Appropriate use                                              | Example                                                                       |
| --------- | ------------------------------------------------------------ | ----------------------------------------------------------------------------- |
| Strategy  | Interchangeable algorithm                                    | Pricing / eviction / allocation policy                                        |
| Factory   | Centralised construction or implementation selection         | Choose a payment adapter by provider                                          |
| Builder   | Many optional construction inputs with final validation      | Immutable request or configuration                                            |
| State     | Behaviour and allowed operations change with lifecycle state | Vending machine / booking; an enum plus guarded methods may suffice initially |
| Observer  | Notify multiple subscribers of an event                      | Order status notifications; define delivery/failure semantics                 |
| Decorator | Add behaviour while preserving an interface                  | Metrics or retries around a client                                            |
| Adapter   | Translate an external API into an internal contract          | Payment gateway wrapper                                                       |
| Singleton | One instance per class loader                                | Know enum-based construction; prefer injection to hidden global mutable state |

### 5.4 Exceptions, Resources, and Useful Java APIs

- **Checked exceptions** must be caught or declared, such as `IOException`. **Unchecked exceptions** extend `RuntimeException`, such as `IllegalArgumentException`; use them for violated API preconditions where appropriate. Do not catch `Exception` and silently continue.
- Expected domain failures, such as "no seat available", can be explicit result values or domain exceptions. Keep their meaning separate from programming bugs and infrastructure failures.
- Use `Optional<T>` for a return value that may be absent; do not return null instead of an `Optional`, and avoid unchecked `.get()`.
- Use try-with-resources for owned `AutoCloseable` resources. Garbage collection reclaims memory, not timely file/socket cleanup.
- Use `Instant` / `Duration` for timestamps and elapsed business intervals, `Clock` for testability, and `System.nanoTime()` for elapsed runtime measurement. Wall clocks can move backwards.
- `BigDecimal.equals` includes scale (`1.0` differs from `1.00`); `compareTo` compares numeric value. Monetary division needs an explicit rounding policy.
- Garbage collection does not prevent leaks from retained references, unbounded caches, listeners, or `ThreadLocal` values in pooled threads.

```java
static String readFirstLine(java.nio.file.Path path) throws java.io.IOException {
    try (java.io.BufferedReader reader = java.nio.file.Files.newBufferedReader(path)) {
        return reader.readLine();
    }
}
```

## 6. Concurrency and Synchronisation

### 6.1 What Must Be Protected?

| Concept        | Meaning / common mistake                                                                     |
| -------------- | -------------------------------------------------------------------------------------------- |
| Atomicity      | An operation appears indivisible; `count++` is a read-modify-write, not one atomic operation |
| Visibility     | One thread observes another's writes; ordinary unsynchronised fields give no such guarantee  |
| Ordering       | Reordering is constrained by happens-before relationships, not by assumptions about timing   |
| Race condition | Correctness depends on interleaving; check-then-act is a frequent source                     |
| Invariant      | A rule across one or more values that must hold at each observable operation boundary        |

Prefer immutable data and thread confinement first. Sharing a concurrent collection does **not** make the objects inside it or a multi-step workflow thread-safe.

### 6.2 Choosing a Primitive

| Mechanism                      | Use for                                                         | Important limitation                                                                                          |
| ------------------------------ | --------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------- |
| `synchronized`                 | Mutual exclusion and visibility for a critical section          | All accesses to the protected invariant must use the same monitor                                             |
| `volatile`                     | Visibility/order for flags or independently replaced references | No mutual exclusion; `volatile count++` is not atomic; referenced object state is not automatically protected |
| `AtomicInteger` / `AtomicLong` | Atomic updates and compare-and-set on one value                 | Several atomic variables do not make a multi-field transaction                                                |
| `ReentrantLock`                | Timed/interruptible acquisition, `tryLock`, or conditions       | Must unlock in `finally`; use only when these capabilities are needed                                         |
| `ReentrantReadWriteLock`       | Concurrent readers with exclusive writers                       | Added complexity; useful only with an appropriate workload                                                    |
| `Semaphore`                    | Limit simultaneous users of a resource                          | A permit is not an ownership-based object lock                                                                |
| `CountDownLatch`               | Wait for a fixed number of completions                          | One-shot; cannot reset                                                                                        |
| `CyclicBarrier`                | Reusable rendezvous for a fixed group                           | Participants must coordinate failures/cancellation                                                            |

### 6.3 Protect a Whole Invariant with synchronized

```java
final class Inventory {
    private int available;

    Inventory(int available) {
        if (available < 0) throw new IllegalArgumentException("Negative stock");
        this.available = available;
    }

    public synchronized boolean reserve(int quantity) {
        if (quantity <= 0) throw new IllegalArgumentException("Non-positive quantity");
        if (available < quantity) return false;
        available -= quantity;
        return true;
    }

    public synchronized int available() {
        return available;
    }
}
```

- The check and decrement form **one** critical section, so competing reservations cannot oversell this instance. The getter uses the same monitor for visibility.
- An instance synchronized method locks `this`; a static one locks the `Class` object. These are different locks. A `synchronized (privateLock)` block can avoid exposing the monitor to callers.
- Monitors are reentrant and released automatically on normal return or exception. Unlocking a monitor happens-before a subsequent successful lock of that same monitor.
- This protects one shared object inside one JVM only. Multiple service instances/processes require coordination at the shared source of truth, such as a database transaction or conditional update.

### 6.4 Explicit Locks and Atomic Variables

For timed acquisition, use a lock and release it only if acquired. This method belongs to a class; it either increments under the lock or returns false on timeout. Other accesses to `completed` must also acquire this lock.

```java
private final ReentrantLock lock = new ReentrantLock();
private int completed;

boolean tryComplete() throws InterruptedException {
    if (!lock.tryLock(100, TimeUnit.MILLISECONDS)) return false;
    try {
        completed++;
        return true;
    } finally {
        lock.unlock();
    }
}
```

For an independent counter, the simpler choice is `AtomicInteger.incrementAndGet()`. `compareAndSet(expected, updated)` succeeds only if the current value matches the expectation; retry logic must recheck the condition. Atomic update functions may be retried, so keep them free of side effects. `LongAdder` suits high-contention metrics, not an exact atomic check against a limit.

### 6.5 Concurrent Collections and Producer-Consumer

| Type / operation               | Use / caveat                                                                                                           |
| ------------------------------ | ---------------------------------------------------------------------------------------------------------------------- |
| `ConcurrentHashMap`            | Concurrent access; no null keys/values; use `putIfAbsent`, `compute`, or `merge` for atomic per-key updates            |
| `ConcurrentHashMap.compute`    | Atomic per-key decision/update; keep callbacks short and avoid updates to other mappings; not a multi-key transaction  |
| `ArrayBlockingQueue`           | Bounded producer-consumer queue with backpressure                                                                      |
| `CopyOnWriteArrayList`         | Frequent reads, rare writes; snapshot iteration; each write copies the backing array                                   |
| `Collections.synchronizedList` | Serialises individual operations; compound actions and iteration still require external synchronisation on the wrapper |

```java
ConcurrentHashMap<String, Integer> counts = new ConcurrentHashMap<>();
counts.merge("completed", 1, Integer::sum);

BlockingQueue<String> jobs = new ArrayBlockingQueue<>(100);
jobs.put("job-1");
String job = jobs.take();
```

`put` waits for capacity; `take` waits for an element; both can throw `InterruptedException`. Timed `offer` / `poll` allow bounded waits. Define shutdown with interruption or a termination-message protocol; never assume that an empty queue means producers are finished.

Prefer `BlockingQueue` to hand-written `wait` / `notify` buffers. When discussing monitors: call `wait` / `notifyAll` while holding the same monitor, check the condition in a **while loop**, and remember `wait` releases that monitor while `sleep` does not. `Condition.await` similarly releases its associated lock and requires a condition loop.

### 6.6 Executors, Futures, and Cancellation

Use an executor for task ownership and lifecycle instead of starting unmanaged threads. `Runnable` returns no value; `Callable<T>` returns a value and may throw checked exceptions.

```java
static int runTask() throws InterruptedException, ExecutionException, TimeoutException {
    ExecutorService executor = Executors.newFixedThreadPool(2);
    Future<Integer> future = executor.submit(() -> 42);
    try {
        return future.get(1, TimeUnit.SECONDS);
    } finally {
        future.cancel(true);
        executor.shutdownNow();
    }
}
```

- `get` waits and exposes task failures through `ExecutionException`; a timeout alone does not cancel work. `cancel(true)` and `shutdownNow()` request interruption, not forced termination. This short-lived example abandons outstanding work; services normally own a longer-lived pool and shut it down deliberately.
- `shutdown()` stops accepting tasks but allows submitted work to finish; `awaitTermination` waits for exit. `shutdownNow()` attempts to interrupt active tasks and returns queued tasks that never started. Close other owned resources as part of shutdown too.
- Interruption is cooperative. Propagate `InterruptedException`; if a task cannot propagate it, restore the status with `Thread.currentThread().interrupt()` and exit/clean up. Never swallow it and keep looping.
- `newFixedThreadPool` has an unbounded queue. Production workloads often need a bounded `ThreadPoolExecutor`, a rejection/backpressure policy, and timeouts. Avoid tasks waiting on other tasks in the same saturated pool.
- `CompletableFuture`: `thenApply` transforms, `thenCompose` chains async work, `thenCombine` combines independent results, and `exceptionally` handles failure. Non-async stages may run on the completing thread; async stages without an executor usually use the common pool. `join()` wraps failures unchecked; cancellation does not generally interrupt the underlying work.
- **Java 21+:** virtual threads via `Executors.newVirtualThreadPerTaskExecutor()` simplify blocking I/O concurrency. They do not make CPU work faster or remove races; still bound access to scarce resources such as database connections.

### 6.7 Deadlocks, Publication, and Concurrency Tests

- Acquire multiple locks in one consistent global order. Keep critical sections short; avoid network calls, callbacks, and unbounded blocking while holding locks.
- Distinguish deadlock (cyclic waiting), starvation (a task repeatedly denied progress), and livelock (threads keep reacting without completing useful work). Fair locks do not guarantee overall application fairness.
- Publish shared objects safely through a lock, a volatile reference, static initialisation, or a concurrent handoff. Do not let `this` escape during construction. Visibility guarantees do not replace invariant-level locking.
- Define the atomic operation or **linearisation point**: successful reservation, per-key update, CAS, or database commit. A concurrent map alone cannot make a booking-plus-payment workflow atomic.
- Use latches/barriers to coordinate competing test threads and timeouts to detect hangs, not `Thread.sleep` as a correctness mechanism. Assert invariants such as non-negative stock, one successful reservation, and no lost updates; a passing stress test does not prove race freedom.

## 7. Final Revision Checklist

| DSA round                                       | LLD round                                                              |
| ----------------------------------------------- | ---------------------------------------------------------------------- |
| Correct collection and operation costs          | Clear responsibilities, invariants, and API contracts                  |
| Overflow-safe arithmetic and comparisons        | Encapsulation, composition, and justified extension points             |
| `equals` vs `==`, immutable hash keys           | Entity identity vs value equality; defensive copies                    |
| Empty/null cases, range boundaries, duplicates  | Lifecycle transitions, error handling, retries, and resource ownership |
| Recursion depth, input mutation, copied results | Explicit thread-safety scope, lock ownership, and atomic operations    |
| Complexity explanation and edge-case tests      | Deterministic tests, cancellation, timeouts, and cleanup               |
