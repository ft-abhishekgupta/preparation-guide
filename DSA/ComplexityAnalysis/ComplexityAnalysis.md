# Complexity Analysis

## Time and Space Complexity

Complexity is a measure of the amount of resources (time and space) that an algorithm uses as a function of the size of the input.
It is measured to evaluate efficiency, compare algorithms, and predict performance on scale.

### Best, Average, and Worst Case Complexity

- Best Case: Ω(n) - Minimum Time
- Average Case: Θ(n) - Average Time
- Worst Case: O(n) - Maximum Time
- Amortized - Average cost per opertaion

**Big O Notation: O(f(n))**
= There exists a constant c > 0 and n0 > 0 such that T(n) ≤ c \* f(n) for all n ≥ n0.
![alt text](image.png)

> Big O Notation = Upper Bound of the algorithm's running time = USUALLY CONSIDERED AS TIME COMPLEXITY

### Space Complexity

Space complexity is the amount of memory space required by an algorithm in its life cycle

- **Input Space:** The space required to store the input data.
- **Auxiliary Space:** The space required by the algorithm to perform its operations, excluding the input space.
- **Output Space:** The space required to store the output data.

> Auxiliary Space = Total Space - Input Space - Output Space = USUALLY CONSIDERED AS SPACE COMPLEXITY

### Time Complexity

```cs
// Linear Loops
for (int i = 0; i < n; i++) { ... } // O(n)
// Nested Loops
for (int i = 0; i < n; i++) {
    for (int j = 0; j < n; j++) { ... } // O(n^2)
}
// Logarithmic Loops
for (int i = 1; i < n; i *= 2) { ... } // O(log n)
// Recursive Calls
void RecursiveFunction(int n) {
    if (n <= 1) return;
    RecursiveFunction(n - 1); // O(n)
    RecursiveFunction(n / x); // O(log n base x)
    RecursiveFunction(n / 2); for (int i = 0; i < n; i++) { ... } // O(n Log n)
    RecursiveFunction(n - 1); RecursiveFunction(n - 1); // O(2^n)
}
```

#### Calculating Time Complexity

1. Ignore constant factors and lower order terms.
2. Focus on the dominant term that grows the fastest as n increases.
3. If Else statements: Take the maximum time complexity of the branches.
4. Loops: Multiply the time complexity of the loop body by the number of iterations.
5. Recursion: Use recurrence relations or the Master Theorem to analyze recursive algorithms.

| Complexity | Name         | Example                                     |
| ---------- | ------------ | ------------------------------------------- |
| O(1)       | Constant     | Accessing an array element by index         |
| O(log n)   | Logarithmic  | Binary Search                               |
| O(n)       | Linear       | Traversing an array or linked list          |
| O(n log n) | Linearithmic | Merge Sort, Quick Sort                      |
| O(n^2)     | Quadratic    | Bubble Sort, Insertion Sort, Selection Sort |
| O(2^n)     | Exponential  | Recursive Fibonacci, Subset generation      |
| O(n!)      | Factorial    | Traveling Salesman Problem (TSP)            |

## Master Theorem

For `T(n) = a·T(n/b) + f(n)`:

| Case | Condition                                  | Result                          |
| ---- | ------------------------------------------ | ------------------------------- |
| 1    | `f(n) = O(n^(log_b a − ε))`                | `T(n) = Θ(n^(log_b a))`         |
| 2    | `f(n) = Θ(n^(log_b a))`                    | `T(n) = Θ(n^(log_b a) · log n)` |
| 3    | `f(n) = Ω(n^(log_b a + ε))` and regularity | `T(n) = Θ(f(n))`                |

**Quick examples:**

- Merge sort: a=2, b=2, f=Θ(n) → Case 2 → **Θ(n log n)**
- Binary search: a=1, b=2, f=Θ(1) → Case 2 → **Θ(log n)**
- Strassen: a=7, b=2, f=Θ(n²) → Case 1 → **Θ(n^2.81)**

## Constraint Size → Expected Algorithm

| Input size n | Target complexity | Algorithm class                                           |
| ------------ | ----------------- | --------------------------------------------------------- |
| n ≤ 20       | O(2^n) or O(n!)   | Backtracking, bitmask DP, meet-in-middle                  |
| n ≤ 400      | O(n³)             | Triple-loop DP, Floyd-Warshall                            |
| n ≤ 3,000    | O(n²)             | Two nested loops, O(n²) DP                                |
| n ≤ 10⁵      | O(n log n)        | Sort, binary search, segment tree, heap                   |
| n ≤ 10⁶      | O(n)              | Two pointers, sliding window, prefix sum                  |
| n ≤ 10⁷      | O(n) tight        | Linear scan only, avoid constant-heavy O(n)               |
| n ≤ 10¹⁸     | O(log n)          | Binary search, fast exponentiation, matrix exponentiation |

> Rule of thumb: CPUs do ~10⁸ simple ops/sec. Budget 1–2 seconds → budget 10⁸ ops.

## Edge Cases & Invariants Checklist

**Input edge cases:**

- Empty collection / null input
- Single element
- All elements identical
- Already sorted (ascending/descending)
- Integer overflow (use `long` if n up to 10⁹ and multiplied)
- Negative numbers where algorithm assumes positives

**Loop invariant checklist:**

- What is true at the start of each iteration?
- Is the invariant maintained after each step?
- Does it hold at termination and give the correct result?

**Common bugs:**

- Off-by-one in binary search (`lo <= hi` vs `lo < hi`; return `lo` vs `lo-1`)
- Modifying a collection while iterating it
- Uninitialized `best`/`max` (use `int.MinValue`, not `0`, when values can be negative)
- Stack overflow from O(n) recursion depth on n = 10⁵
