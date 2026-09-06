# Advanced Patterns

---

## Core Concepts

- **Segment tree** — binary tree over array intervals; each node stores aggregate of its range. O(log n) query and update.
- **Fenwick tree (BIT)** — implicit tree using bit manipulation; limited to prefix-sum-style queries but simpler and faster constant.
- **Lazy propagation** — defer range updates in segment tree; propagate lazily on demand.
- **Bloom filter** — probabilistic set; no false negatives; false positive rate tunable by size/hash count.
- **Count-Min Sketch** — probabilistic frequency counter; underestimates never, overestimates bounded.
- **HyperLogLog** — estimates distinct element count in O(1) space; ~1–2% error.

---

## Segment Tree — Build / Query / Point Update

```csharp
class SegmentTree
{
    private int[] _tree;
    private int _n;

    public SegmentTree(int[] arr)
    {
        _n = arr.Length;
        _tree = new int[4 * _n];
        Build(arr, 0, 0, _n - 1);
    }

    private void Build(int[] arr, int node, int start, int end)
    {
        if (start == end) { _tree[node] = arr[start]; return; }
        int mid = (start + end) / 2;
        Build(arr, 2*node+1, start, mid);
        Build(arr, 2*node+2, mid+1, end);
        _tree[node] = _tree[2*node+1] + _tree[2*node+2]; // sum; replace for min/max
    }

    // Range sum query [l, r]
    public int Query(int l, int r) => Query(0, 0, _n-1, l, r);
    private int Query(int node, int start, int end, int l, int r)
    {
        if (r < start || end < l) return 0;              // out of range
        if (l <= start && end <= r) return _tree[node];  // fully covered
        int mid = (start + end) / 2;
        return Query(2*node+1, start, mid, l, r) + Query(2*node+2, mid+1, end, l, r);
    }

    // Point update: set arr[idx] = val
    public void Update(int idx, int val) => Update(0, 0, _n-1, idx, val);
    private void Update(int node, int start, int end, int idx, int val)
    {
        if (start == end) { _tree[node] = val; return; }
        int mid = (start + end) / 2;
        if (idx <= mid) Update(2*node+1, start, mid, idx, val);
        else Update(2*node+2, mid+1, end, idx, val);
        _tree[node] = _tree[2*node+1] + _tree[2*node+2];
    }
}
```

Build: O(n). Query: O(log n). Update: O(log n). Space: O(4n).

---

## Lazy Propagation (Conceptual)

Range updates (add delta to all elements in [l,r]) without lazy: O(n) per update. With lazy:

- Store `lazy[node]` = pending delta not yet pushed to children.
- On query/update touching a node's range, **push down** the lazy value to children first.
- O(log n) range update + range query.

**Useful for:** range add + range sum, range set + range min, etc.

---

## Fenwick Tree (Binary Indexed Tree)

```csharp
class FenwickTree
{
    private int[] _bit;
    private int _n;

    public FenwickTree(int n) { _n = n; _bit = new int[n + 1]; }

    // Add delta to index i (1-indexed)
    public void Update(int i, int delta)
    {
        for (; i <= _n; i += i & -i) _bit[i] += delta;
    }

    // Prefix sum [1..i]
    public int Query(int i)
    {
        int sum = 0;
        for (; i > 0; i -= i & -i) sum += _bit[i];
        return sum;
    }

    // Range sum [l..r]
    public int RangeQuery(int l, int r) => Query(r) - Query(l - 1);
}
```

Build: O(n log n) via updates or O(n) direct construction. Query/Update: O(log n). Space: O(n).

---

## Segment Tree vs Fenwick vs Prefix Sum

| Aspect               | Prefix Sum                   | Fenwick Tree                      | Segment Tree                 |
| -------------------- | ---------------------------- | --------------------------------- | ---------------------------- |
| Build                | O(n)                         | O(n log n)                        | O(n)                         |
| Point update         | O(n) rebuild                 | **O(log n)**                      | **O(log n)**                 |
| Range query          | **O(1)**                     | O(log n)                          | O(log n)                     |
| Range update         | O(1) with diff array         | O(log n)                          | O(log n) lazy                |
| Supported aggregates | Sum only (or with diff arr)  | Sum, XOR                          | **Any (min, max, GCD, sum)** |
| Code complexity      | Simple                       | Medium                            | Complex                      |
| Space                | O(n)                         | O(n)                              | O(4n)                        |
| Best for             | Static array, range sum only | Dynamic point update + prefix sum | Any dynamic range aggregate  |

---

## DSU Recap (Union-Find)

See [Graphs](10-Graphs.md) for full C# implementation. Key applications:

| Application                | How DSU is used                     |
| -------------------------- | ----------------------------------- |
| Connected components       | Union nodes, count roots            |
| Cycle detection            | Union edge; false = cycle           |
| Kruskal's MST              | Union edge; false = skip            |
| Accounts merge             | Union same emails → same account    |
| Redundant connection       | First `Union` returning false       |
| Smallest string with swaps | Same component = can permute freely |

DSU with path compression + union by rank: O(α(n)) ≈ O(1) amortized.

---

## LRU / LFU

See [Linked Lists and Caches](../99-Cheatsheets) — these are standalone design problems. Summary:

- **LRU**: `Dictionary<key,Node>` + doubly-linked list. O(1) all ops.
- **LFU**: Two hashmaps + frequency-bucketed linked lists. O(1) all ops.

---

## Bloom Filter

**Concept:** m-bit array + k hash functions. On insert: set k bits. On query: check all k bits — if any 0 → definitely absent; all 1 → probably present (false positive possible).

**False positive rate:** `p ≈ (1 - e^(-kn/m))^k` where n = inserted items, m = bits, k = hash functions.

**Optimal k:** `k = (m/n) · ln 2 ≈ 0.693 · m/n`.

**Sizing formula:** `m = -n·ln(p) / (ln 2)²` bits for desired false-positive rate p.

**Example:** 1M items, 1% FP rate → `m = -1M·ln(0.01)/(0.693²) ≈ 9.6M bits ≈ 1.2 MB`.

```mermaid
flowchart LR
    I["Insert(x)"] --> H1["h1(x) → bit 42"]
    I --> H2["h2(x) → bit 1701"]
    I --> H3["h3(x) → bit 99"]
    Q["Query(x)"] --> C{"All k bits set?"}
    C -- "No" --> N["Definitely NOT in set"]
    C -- "Yes" --> Y["Probably in set (FP possible)"]
```

**Where used in production:**

- **Databases** (e.g., Cassandra, RocksDB): skip SSTable disk reads for non-existent keys.
- **CDNs**: check if content is in cache before expensive lookup.
- **Distributed caches**: Redis uses Bloom filters to protect against cache miss stampede.
- **Web crawlers**: already-visited URL tracking.

See also: [../05-System-Design-HLD/00-README.md](../05-System-Design-HLD/00-README.md) for system-level cache and CDN patterns.

---

## Count-Min Sketch

**Concept:** d×w integer array + d independent hash functions. On insert: increment `table[i][hi(x)]` for each row i. On query frequency: return `min(table[i][hi(x)])` across rows.

- **No false negatives** for frequency (never underestimates).
- **Overestimates** by at most ε·N with probability 1-δ, where `w = ⌈e/ε⌉`, `d = ⌈ln(1/δ)⌉`.
- Space: O(d·w) = O(ε^-1 · log(1/δ)).
- **Use cases:** network flow analysis, top-k heavy hitters (combine with min-heap), event counting at scale (Flink, Redis).

---

## HyperLogLog

**Concept:** Estimates distinct elements (cardinality) using O(1.5 KB) regardless of data size. Uses the leftmost-zero-bit position of hashed values as a probabilistic estimator. Multiple registers reduce variance.

- **Error rate:** ≈ 1.04/√m where m = number of registers. With m=16384: ~0.8% error.
- **Space:** m · 5-6 bits per register ≈ 12 KB for 0.8% error.
- **Use cases:** unique visitor counting, distinct query counting in analytics, cardinality estimation in Redis (`PFADD`/`PFCOUNT`).

---

## Skip List

- Probabilistic linked list with express lanes (additional forward pointers). Expected O(log n) search, insert, delete.
- Used in: Redis sorted sets (`ZSET`), LevelDB MemTable.
- Simpler to implement than balanced BST; no rotations needed.
- Space: O(n) expected; each node has O(log n) pointers in expectation.

---

## B-Tree / B+Tree vs LSM Tree

| Aspect              | B+Tree                                           | LSM Tree                                              |
| ------------------- | ------------------------------------------------ | ----------------------------------------------------- |
| Structure           | Balanced tree, all data in leaves                | Log-structured: in-memory buffer + immutable SSTables |
| Write performance   | O(log n), in-place update                        | **O(1) amortized** (append-only)                      |
| Read performance    | **O(log n)**                                     | O(log n) amortized + compaction overhead              |
| Write amplification | Low                                              | **High** (compaction rewrites data multiple times)    |
| Read amplification  | Low                                              | Higher (may check multiple SSTables)                  |
| Space amplification | Low                                              | Higher (old data retained until compaction)           |
| Best for            | Read-heavy workloads (SQL DBs: InnoDB, Postgres) | Write-heavy workloads (Cassandra, RocksDB, LevelDB)   |
| Crash recovery      | WAL + page-level recovery                        | WAL + compaction replay                               |

---

## Trade-offs & When to Use

- **Segment tree**: when you need non-prefix aggregates (min, max, GCD) with updates.
- **Fenwick tree**: when prefix sums with point updates suffice — simpler, faster constant.
- **Bloom filter**: when false negatives are unacceptable but false positives are OK; saves expensive lookups. Can't delete (use Counting Bloom Filter for deletions).
- **Count-Min Sketch**: frequency estimation over a stream where exact counts are infeasible.
- **HyperLogLog**: distinct count at massive scale (Redis: 100M elements → 12 KB).
- **LSM tree**: write-heavy storage (Cosmos DB internally, Cassandra, RocksDB).
