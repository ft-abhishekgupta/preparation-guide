# Patterns

> Space complexity is auxiliary space unless the algorithm explicitly builds a result structure.

## Arrays

### Prefix Sum

```
Running sum of values in array
- For range sum
- For subset sum max
```

```cs
// Build: Time O(n), Space O(n)
// prefixSum[i] = sum of values till i
var prefixSum = new int[len];
prefixSum[0] = num[0];
for (int i = 1; i < len; i++)
    prefixSum[i] = num[i] + prefixSum[i-1];

// Range query [i...j]: Time O(1), Space O(1)
if(i == 0)
    sum = prefixSum[j];
else
    sum = prefixSum[j] - prefixSum[i-1];
```

### Difference Array

```
- For range operations in O(1)
```

```cs
// u updates + final build: Time O(u + n), Space O(n)
// Range update [l..r] += val in O(1); build final array with prefix sum
var diff = new int[len + 1];
foreach (var (l, r, val) in updates)
{
    diff[l] += val;
    diff[r + 1] -= val;   // cancel after r
}
// Accumulate to get final values
var res = new int[len];
int running = 0;
for (int i = 0; i < len; i++)
{
    running += diff[i];
    res[i] = running;
}
return res;
```

### Quickselect - kth smallest, avg O(n)

```cs
// Average: Time O(n), Space O(log n); Worst: Time O(n^2), Space O(n)
int Select(int lo, int hi, int k) // k is target index in sorted order
{
    int pivot = num[hi], i = lo;
    for (int j = lo; j < hi; j++)
        if (num[j] < pivot)
            (num[i], num[j]) = (num[j], num[i++]);
    (num[i], num[hi]) = (num[hi], num[i]);
    if (i == k)
        return num[i];
    return i < k ? Select(i + 1, hi, k) : Select(lo, i - 1, k);
}
```

## Hashing

### Membership Check

```cs
// Time O(n) average, Space O(n)
var set = new HashSet<int>();
foreach (int x in num)
    if (!set.Add(x)) {
        /* <duplicate found> */
    }
```

### Frequency count

```cs
// Time O(n) average, Space O(n)
var freq = new Dictionary<int, int>();
foreach (int x in num)
    freq[x] = freq.GetValueOrDefault(x) + 1;
```

## Two Pointers

```cs
// Same Direction
// Time O(n), Space O(1)
int slow = 0;
for (int fast = 0; fast < num.Length; fast++)
{
    if (/* <condition to keep num[fast]> */)
        num[slow++] = num[fast];
}
return slow;

// Opposite Direction
// Time O(n), Space O(1)
int left = 0, right = num.Length - 1;
while (left < right)
{
    if (/* <condition> */)
        left++;
    else
        right--;
    // <update answer>
}
```

## Sliding Window

> `count(exactly K) = count(at most K) - count(at most K-1)`

```cs
// Fixed Size k
// Time O(n), Space O(1)
int sum = 0;
for (int i = 0; i < num.Length; i++)
{
    sum += num[i];
    if (i >= k)
        sum -= num[i - k];
    if (i >= k - 1)
    {
        /* <update answer> */
    }
}

// Variable Size - Minimum
// Time O(n), Space O(1) plus any window state
int left = 0, right = 0, best = int.MaxValue;
while (right < num.Length)
{
    // <add num[right]>
    while (/* <Condition Met> */)
    {
        // Update ans
        best = Math.Min(best, right - left + 1);
        // <remove num[left]>
        left++;
    }
    right++;
}
return best == int.MaxValue ? 0 : best;

// Variable Size - Maximum
// Time O(n), Space O(1) plus any window state
int left = 0, right = 0, best = 0;
while (right < num.Length)
{
    // <add num[right]>
    while (/* <Condition Broken> */)
    {
        // <remove num[left]>
        left++;
    }
    // Update ans
    best = Math.Max(best, right - left + 1);
    right++;
}
return best;
```

## Stack

### Monotonic Stack

```
- Next greater or smaller element
- Stack stores only indices
```

```cs
// Next greater element
// Time O(n), Space O(n)
// Stack contains elements in increasing order
var stack = new Stack<int>();
var res = new int[num.Length];
for (int i = 0; i < num.Length; i++){
    while(stack.Count > 0 && num[stack.Peek()] < num[i]){
        res[stack.Pop()] = num[i];
    }
    stack.Push(i);
}
```

## Binary Search

```cs
// Exact Value
// Time O(log n), Space O(1)
int lo = 0, hi = num.Length - 1;
while (lo <= hi)
{
    int mid = lo + (hi - lo) / 2;
    if (num[mid] == target)
        return mid;
    if (num[mid] < target)
        lo = mid + 1;
    else
        hi = mid - 1;
}
return -1;

// Binary Search on Answer
// Time O(log(maxAns - minAns) * F), Space O(S), where Feasible costs O(F) time and O(S) space
int lo = minAns, hi = maxAns;
while (lo < hi)
{
    int mid = lo + (hi - lo) / 2;
    if (!Feasible(mid))   // <feasibility check>
        lo = mid + 1;
    else
        hi = mid;
}
return lo;

// Lower Bound - first i where num[i] >= target
// Time O(log n), Space O(1)
int lo = 0, hi = num.Length;
while (lo < hi)
{
    int mid = lo + (hi - lo) / 2;
    if (num[mid] < target)
        lo = mid + 1;
    else
        hi = mid;
}
return lo;

// Upper Bound - first i where num[i] > target
// Time O(log n), Space O(1)
int lo = 0, hi = num.Length;
while (lo < hi)
{
    int mid = lo + (hi - lo) / 2;
    if (num[mid] <= target)
        lo = mid + 1;
    else
        hi = mid;
}
return lo;
```

## Range Query Data Structures

### Range Sum Query - Mutable

Design a structure over an integer array supporting `update(index, value)` and `sumRange(left, right)`.

**Example:** `nums = [1, 3, 5]; sumRange(0, 2)` → `9`; `update(1, 2), sumRange(0, 2)` → `8`

```text
PLAIN ARRAY | O(1) update, O(N) query | O(1)
PREFIX SUM   | O(N) update, O(1) query | O(N)

Use a prefix-sum array only when the data is immutable

-----------------------------------------------------------------------------

FENWICK TREE | O(log N) update, O(log N) query | O(N)

sumRange(left, right) = QUERY(right + 1) - QUERY(left)
Simplest to write, but only supports invertible operations such as sum and xor

-----------------------------------------------------------------------------

SEGMENT TREE | O(log N) update, O(log N) query | O(N)

BUILD(node, left, right):
    if left == right:
        tree[node] = nums[left]
        return
    mid = left + (right - left) / 2
    BUILD(2 * node, left, mid)
    BUILD(2 * node + 1, mid + 1, right)
    tree[node] = tree[2 * node] + tree[2 * node + 1]

UPDATE(node, left, right, index, value):
    if left == right:
        tree[node] = value
        return
    mid = left + (right - left) / 2
    if index <= mid:
        UPDATE(2 * node, left, mid, index, value)
    else:
        UPDATE(2 * node + 1, mid + 1, right, index, value)
    tree[node] = tree[2 * node] + tree[2 * node + 1]

QUERY(node, left, right, queryLeft, queryRight):
    if queryRight < left OR right < queryLeft:
        return 0
    if queryLeft <= left AND right <= queryRight:
        return tree[node]
    mid = left + (right - left) / 2
    return QUERY(2 * node, left, mid, queryLeft, queryRight)
           + QUERY(2 * node + 1, mid + 1, right, queryLeft, queryRight)
```

> Range updates need lazy propagation: store a pending delta on each affected node and push it down only when that subtree is visited.

## Linked List

```cs
// Reverse
// Time O(n), Space O(1)
ListNode prev = null, cur = head;
while (cur != null)
{
    ListNode next = cur.next;
    cur.next = prev;
    prev = cur;
    cur = next;
}
return prev;

// Fast & Slow - cycle detection
// Time O(n), Space O(1)
ListNode slow = head, fast = head;
while (fast != null && fast.next != null)
{
    slow = slow.next;
    fast = fast.next.next;
    if (slow == fast)   // Cycle Detected
        slow = head;    // Get Entry Point
        while (slow != fast){
            slow = slow.next;
            fast = fast.next;
        }
        return true;
}
return false;

// Middle node (slow ends at middle)
// Time O(n), Space O(1)
ListNode slow = head, fast = head;
while (fast != null && fast.next != null)
{
    slow = slow.next;
    fast = fast.next.next;
}
return slow;

// Dummy head - simplifies insert/delete at front
// Time O(1) for front insert/delete, Space O(1)
var dummy = new ListNode(0) { next = head };
ListNode cur = dummy;
// ... rewire via cur.next ...
return dummy.next;
```

## Heap

### K Largest Elements

```
Use min heap of size K
For smallest elements, use max heap
```

```cs
// Time O(n log k), Space O(k)
var pq = new PriorityQueue<int, int>();
foreach (int x in num)
{
    pq.Enqueue(x, x);
    if (pq.Count > k)
        pq.Dequeue(); // keep k largest
}
pq.Peek(); // Kth Largest
```

### Repeated Operations on Maximum or Minimum Elements

**Time:** `O(n + p log n)` for heap construction and `p` operations | **Space:** `O(n)`

```text
Create a heap from the elements
Repeatedly pop, perform the operation, and push the result
```

## Tree

### Traversal

```cs
// BFS - level order
// Time O(n), Space O(w), where w is the maximum tree width
var res = new List<IList<int>>();
if (root == null)
    return res;
var queue = new Queue<TreeNode>();
queue.Enqueue(root);
while (queue.Count > 0)
{
    int size = queue.Count;
    var level = new List<int>();
    for (int i = 0; i < size; i++)
    {
        var node = queue.Dequeue();
        level.Add(node.val);
        if (node.left != null)
            queue.Enqueue(node.left);
        if (node.right != null)
            queue.Enqueue(node.right);
    }
    res.Add(level);
}
return res;

// DFS - pre / in / post by placement
// Time O(n), Space O(h), where h is the tree height
void Dfs(TreeNode node)
{
    if (node == null)
        return;
    // <preorder>
    Dfs(node.left);
    // <inorder>
    Dfs(node.right);
    // <postorder>
}
Dfs(root);
```

### Height / Diameter

```cs
// Returns height; tracks longest path (edges) through any node
// Time O(n), Space O(h), where h is the tree height
int diameter = 0;
int Height(TreeNode node)
{
    if (node == null)
        return 0;
    int l = Height(node.left), r = Height(node.right);
    diameter = Math.Max(diameter, l + r);
    return 1 + Math.Max(l, r);
}
```

## Graph

### Traversal

```
BFS - Level by level
DFS - Depth first
```

```cs
// BFS
// Time O(V + E), Space O(V)
var queue = new Queue<int>();
var visited = new bool[n];
queue.Enqueue(start);
visited[start] = true;
while (queue.Count > 0)
{
    int node = queue.Dequeue();
    // <process node>
    foreach (int next in adj[node])
    {
        if (visited[next])
            continue;
        visited[next] = true;
        queue.Enqueue(next);
    }
}

// DFS
// Time O(V + E), Space O(V)
var visited = new bool[n];
void Dfs(int node)
{
    visited[node] = true;
    // <process node>
    foreach (int next in adj[node])
        if (!visited[next])
            Dfs(next);
}
Dfs(start);
```

### Connected Components / Path Compression

```cs
// Union-Find
// Initialization + m operations: Time O(V + m * alpha(V)), Space O(V)
var parent = new int[n];
var rank = new int[n];

for (int i = 0; i < n; i++)
    parent[i] = i;

int Find(int x) => parent[x] == x ? x : parent[x] = Find(parent[x]);

bool Union(int a, int b)
{
    int pa = Find(a), pb = Find(b);
    if (pa == pb)   // Same Component
        return false;
    if (rank[pa] < rank[pb])
        (pa, pb) = (pb, pa);
    parent[pb] = pa;
    if (rank[pa] == rank[pb])
        rank[pa]++;
    return true;
}

// DFS / BFS
Number of BFS and DFS on non visited nodes
```

### Cycle Detection

```
DFS or Union-Find - Undirected Graph
3 State DFS or Topological Sort - Directed Graph
```

```cs
// DFS
DFS(node, parent):
    visited[node] = true
    for neighbor:
        if neighbor not visited:
            if DFS(neighbor, node):
                return true
        else if neighbor != parent:
            return true
    return false

// 3 State DFS
DFS(node):
    state[node] = CURRENT_PATH
    for neighbor:
        if state[neighbor] == CURRENT_PATH:
            cycle found
        if state[neighbor] == NOT_VISITED:
            if DFS(neighbor):
                cycle found
    state[node] = COMPLETED
    return false
```

### Topological Sort

```
> Calculate indegree of all nodes = Number of edges towards it
> Put nodes with indegree 0 in queue
> BFS traverse, decreasing the indegree for all adjacent nodes and pushing the 0 indegree node in queue
```

```cs
// Kahn's - BFS on indegree
// Time O(V + E), Space O(V)
var indeg = new int[n];
foreach (var e in edges) indeg[e[1]]++;
var queue = new Queue<int>();
for (int i = 0; i < n; i++)
    if (indeg[i] == 0)
        queue.Enqueue(i);
var order = new List<int>();
while (queue.Count > 0)
{
    int node = queue.Dequeue();
    order.Add(node);
    foreach (int next in adj[node])
        if (--indeg[next] == 0)
            queue.Enqueue(next);
}
return order.Count == n ? order : new List<int>(); // empty = cycle
```

### Shortest Path

```
BFS - Unweighted
> Traverse and increase the length at each level

Dijkstra - Non negative weights
> Use min heap of distance
> BFS traversal with adjacent nodes distance relaxation

Bellman Ford - Negative weights
> Relax each edge n-1 times
```

```cs
// BFS - unweighted
// Time O(V + E), Space O(V)
var dist = new int[n];
Array.Fill(dist, -1);
var queue = new Queue<int>();
queue.Enqueue(start);
dist[start] = 0;
while (queue.Count > 0)
{
    int node = queue.Dequeue();
    foreach (int next in adj[node])
        if (dist[next] == -1)
        {
            dist[next] = dist[node] + 1;
            queue.Enqueue(next);
        }
}

// Dijkstra - non-negative weights
// Time O((V + E) log V), Space O(V + E) including the priority queue
var dist = new int[n];
Array.Fill(dist, int.MaxValue);
dist[start] = 0;
var pq = new PriorityQueue<int, int>(); // <node, distance>
pq.Enqueue(start, 0);
while (pq.Count > 0)
{
    pq.TryDequeue(out int node, out int d);
    if (d > dist[node])
        continue;
    foreach (var (next, w) in adj[node])
        if (dist[node] + w < dist[next])
        {
            dist[next] = dist[node] + w;
            pq.Enqueue(next, dist[next]);
        }
}

// Bellman Ford - negative edges + cycle detection
// Time O(V * E), Space O(V)
var dist = new int[n];
Array.Fill(dist, int.MaxValue);
dist[start] = 0;
for (int i = 0; i < n - 1; i++)          // relax n-1 times
    foreach (var e in edges)             // e = (u, v, w)
        if (dist[e[0]] != int.MaxValue
        && dist[e[0]] + e[2] < dist[e[1]])
            dist[e[1]] = dist[e[0]] + e[2];

foreach (var e in edges)   // extra pass => negative cycle
    if (dist[e[0]] != int.MaxValue
    && dist[e[0]] + e[2] < dist[e[1]])
        return null;
```

### MST

```cs
// MST - Kruskals (uses Find/Union above)
// Time O(E log E), Space O(V + log E) for DSU and in-place sort stack
Array.Sort(edges, (a, b) => a[2] - b[2]);
int total = 0, used = 0;
foreach (var e in edges) // e = (u, v, w)
    if (Union(e[0], e[1]))
    {
        total += e[2];
        if (++used == n - 1)
            break;
    }
return total;

// MST - Prims
// Add minimum weight edge to current MST till n-1 edges
public int Prim(int n, List<(int to, int weight)>[] graph)
{
    var pq = new PriorityQueue<(int node, int weight), int>();
    var visited = new bool[n];
    int mstCost = 0;
    int count = 0;
    // Start from node 0
    pq.Enqueue((0, 0), 0);

    while (pq.Count > 0 && count < n)
    {
        var (node, weight) = pq.Dequeue();

        if (visited[node])
            continue;

        visited[node] = true;
        count++;
        mstCost += weight;

        foreach (var (next, edgeWeight) in graph[node])
        {
            if (!visited[next])
            {
                pq.Enqueue((next, edgeWeight), edgeWeight);
            }
        }
    }
    // Graph is disconnected → MST doesn't exist
    if (count != n)
        return -1;

    return mstCost;
}
```

### All Pair Shortest Path

```cs
FLOYD-WARSHALL | O(V^3) | O(V^2)

// distance[i][j] using only the first k vertices as intermediates
// k MUST be the outermost loop, otherwise the recurrence is wrong

initialise distance[i][i] = 0
initialise distance[u][v] = weight(u, v), otherwise infinity

for k = 0 to n - 1:
    for i = 0 to n - 1:
        for j = 0 to n - 1:
            if distance[i][k] + distance[k][j] < distance[i][j]:
                distance[i][j] = distance[i][k] + distance[k][j]

> - Negative edges are allowed; `distance[i][i] < 0` means a negative cycle exists.
> - Replacing `min`/`+` with `OR`/`AND` gives the transitive closure (reachability).
```

## Backtracking

```cs
CHECK > MARK > EXPLORE > UNMARK
// Permutation-style loop shown: Time O(n * n!), Space O(n) recursion depth
// Exact complexity depends on the branching factor and pruning used by the problem.
void Backtrack(int index, List<int> num){
    if(index == num.Length){
        // Append to global ans
        return;
    }
    for(int i = index; i < num.Length; i++){
        if (/* <skip invalid/duplicate> */)
            continue;
        // Include / Swap / Add
        Backtrack(i + 1, num);
        // Undo the above inclusion / swapping
    }
}
Backtrack(0,num);
```

## Dynamic Programming

```cs
// 1D DP
// Time O(n * t), Space O(n), where t is transitions checked per state
dp[i] = f(dp[i-1], dp[i-2], ...)

// Take Leave
// Time O(n), Space O(n); often reducible to O(1) space
dp[i] = max or min (take item, leave item)

// 0/1 Knapsack
// Time O(n * W), Space O(n * W); reducible to O(W) space
dp[i][w] = Max till i with weight w
    max(dp[i-1][w],
        dp[i-1][w-weight] + value)

// Unbounded Knapsack
// Time O(n * W), Space O(W)
dp[w] =
    max(dp[w],
        dp[w-weight] + value)

// Subset Sum
// Time O(n * target), Space O(n * target); reducible to O(target) space
dp[i][sum] =
    dp[i-1][sum] ||
    dp[i-1][sum-num]

// Coin Change - Minimum
// Time O(n * amount), Space O(amount)
dp[0] = 0
everything else = INF
dp[a] = min(dp[a],
            dp[a-coin] + 1)

// Coin Change - Ways
// Time O(n * amount), Space O(amount)
dp[0] = 1
dp[a] += dp[a-coin]

// LIS
// Time O(n^2), Space O(n)
dp[i] = LIS ending at i
dp[x] = 1 // Fill
if(j < i && nums[j] < nums[i])
    dp[i] = max(dp[i],
                dp[j] + 1)
ans is max in dp array

// LCS
// Time O(m * n), Space O(m * n); reducible to O(min(m, n)) space
if (a[i] == b[j])
    dp[i][j] = dp[i-1][j-1] + 1;
else
    dp[i][j] = max(dp[i-1][j],
                   dp[i][j-1]);

// Grid
// Time O(rows * cols), Space O(rows * cols); often reducible to O(cols) space
dp[i][j] =
    f(top, left, diagonal)

// Palindrome
// Time O(n^2), Space O(n^2)
// l : len-1 to 0, r : 0 to l
dp[l][r] =
    s[l] == s[r] &&
    dp[l+1][r-1]
```

## Bit Manipulation

All operations below take `O(1)` time and `O(1)` space for fixed-width integers.

| Operation                 | Expression                        |
| ------------------------- | --------------------------------- |
| Is odd                    | `x & 1`                           |
| Divide by 2               | `x >> 1`                          |
| Set bit k                 | `x \| (1 << k)`                   |
| Clear bit k               | `x & ~(1 << k)`                   |
| Toggle bit k              | `x ^ (1 << k)`                    |
| Check bit k               | `(x >> k) & 1`                    |
| Lowest set bit            | `x & -x`                          |
| Clear lowest set bit      | `x & (x - 1)`                     |
| XOR values                | `x ^ y`                           |
| Is power of two           | `x > 0 && (x & (x - 1)) == 0`     |
| Count set bits (popcount) | `BitOperations.PopCount((uint)x)` |
| Sign of integer           | `x >> 31`                         |
| Swap without temp         | `a ^= b; b ^= a; a ^= b`          |

## Math

### All primes up to n

```
For each prime number 2 to sqrt of n, cancel all multiples
```

```cs
// Sieve of Eratosthenes - primes up to n
// Time O(n log log n), Space O(n)
var isPrime = new bool[n + 1];
Array.Fill(isPrime, true);
isPrime[0] = isPrime[1] = false;
for (int p = 2; (long)p * p <= n; p++)
    if (isPrime[p])
        for (int m = p * p; m <= n; m += p)
            isPrime[m] = false;
```

### GCD / LCM

```cs
// GCD: Time O(log(min(a, b))), Space O(log(min(a, b))) recursion stack
int Gcd(int a, int b) => b == 0 ? a : Gcd(b, a % b);

// LCM: Time O(log(min(a, b))), Space O(log(min(a, b))) through GCD
long Lcm(int a, int b) => (long)a / Gcd(a, b) * b;
```

### Fast power - a^b mod m, O(log b)

```cs
// Time O(log b), Space O(1)
long Pow(long a, long b, long mod)
{
    long res = 1; a %= mod;
    while (b > 0)
    {
        if ((b & 1) == 1)
            res = res * a % mod;
        a = a * a % mod;
        b >>= 1;
    }
    return res;
}
```

### Math Formulas & Concepts

Sums & Series

- Sum `1..n` = `n(n+1)/2`
- Sum of squares `1²..n²` = `n(n+1)(2n+1)/6`
- Sum of first `n` odds = `n²`; first `n` evens = `n(n+1)`
- Arithmetic series = `count * (first + last) / 2`
- Geometric series `a + ar + ... + ar^(n-1)` = `a(rⁿ - 1)/(r - 1)`
- Powers of 2: `1 + 2 + 4 + ... + 2^(n-1) = 2ⁿ - 1`

Combinatorics

- Permutations `nPr` = `n! / (n-r)!`
- Combinations `nCr` = `n! / (r!(n-r)!)`; `nCr = nC(n-r)`
- Pascal: `nCr = (n-1)C(r-1) + (n-1)Cr`
- Subsets of `n` items = `2ⁿ`; with `k` chosen = `nCk`
- Catalan `Cₙ` = `(2n)! / ((n+1)! n!)` — valid parens, BST shapes, triangulations
- Sum of all `nCr` for r=0..n = `2ⁿ`

Divisibility & Modular

- `(a + b) % m = ((a%m) + (b%m)) % m` (same for `*`)
- `(a - b) % m = ((a%m) - (b%m) + m) % m`
- Use `1_000_000_007` (prime) as common mod
- `n` is prime if no divisor in `2..√n`

Number Properties

- `gcd(a,b) * lcm(a,b) = a * b`
- Digit count of `n` (base 10) = `floor(log10(n)) + 1`

Powers of Two / Bits

- Check power of two: `n > 0 && (n & (n-1)) == 0`
- `2ⁿ` values: 2^10 ≈ 10³, 2^20 ≈ 10⁶, 2^30 ≈ 10⁹
- Max `int` ≈ 2.1×10⁹ (2³¹−1); use `long` past that

Geometry

- Manhattan distance = `|x1-x2| + |y1-y2|`
- Euclidean distance² = `(x1-x2)² + (y1-y2)²` (compare squares, skip sqrt)
- Triangle area (shoelace) = `½ |x1(y2-y3) + x2(y3-y1) + x3(y1-y2)|`

Complexity Cheatsheet (n that fits in ~1s, ~10⁸ ops)

- `O(n!)` → n ≤ 11 · `O(2ⁿ)` → n ≤ 22
- `O(n³)` → n ≤ 500 · `O(n²)` → n ≤ 5,000
- `O(n log n)` → n ≤ 10⁶ · `O(n)` → n ≤ 10⁸
- `O(log n)` / `O(1)` → effectively unbounded
