# Searching and Sorting

## Binary Search

- Data is sorted
- Divide and Conquer approach
- Time Complexity: O(log n)
- Space Complexity: O(1) for iterative, O(log n) for recursive

```cs
// Binary Search (Iterative)
BINARY_SEARCH(arr, target):
    left = 0
    right = length(arr) - 1
    while left <= right:
        mid = left + (right - left) / 2
        if arr[mid] == target:
            return mid
        else if arr[mid] < target:
            left = mid + 1
        else:
            right = mid - 1
    return -1

// Binary Search (Recursive)
BINARY_SEARCH_RECURSIVE(arr, target, left, right):
    if left > right:
        return -1
    mid = left + (right - left) / 2
    if arr[mid] == target:
        return mid
    else if arr[mid] < target:
        return BINARY_SEARCH_RECURSIVE(arr, target, mid + 1, right)
    else:
        return BINARY_SEARCH_RECURSIVE(arr, target, left, mid - 1)

// Lower Bound (First Occurrence)
LOWER_BOUND(arr, target):
    left = 0
    right = length(arr)
    while left < right:
        mid = left + (right - left) / 2
        if arr[mid] < target:
            left = mid + 1
        else:
            right = mid
    return left

// Upper Bound (First Element Greater Than Target)
UPPER_BOUND(arr, target):
    left = 0
    right = length(arr)
    while left < right:
        mid = left + (right - left) / 2
        if arr[mid] <= target:
            left = mid + 1
        else:
            right = mid
    return left

// Binary Search on Answer Space
BINARY_SEARCH_ANSWER_SPACE(low, high, condition):
    while low < high:
        mid = low + (high - low) / 2
        if condition(mid):
            high = mid
        else:
            low = mid + 1
    return low

// Binary Search on Rotated Sorted Array
FIND_MIN_ROTATED_SORTED_ARRAY(arr):
    left = 0
    right = length(arr) - 1
    while left < right:
        mid = left + (right - left) / 2
        if arr[mid] > arr[right]:
            left = mid + 1
        else:
            right = mid
    return arr[left]

// Binary Search to Find Peak Element
FIND_PEAK_ELEMENT(arr):
    left = 0
    right = length(arr) - 1
    while left < right:
        mid = left + (right - left) / 2
        if arr[mid] < arr[mid + 1]:
            left = mid + 1
        else:
            right = mid
    return arr[left]
```

## Sorting Algorithms

```cs
// Bubble Sort
// After each pass, the largest element "bubbles" to the end of the array
for i from 0 to n-1:
    for j from 0 to n-i-2:
        if arr[j] > arr[j+1]:
            swap(arr[j], arr[j+1])

// Selection Sort
// After each pass, the smallest element is placed at the beginning of the unsorted portion
for i from 0 to n-1:
    minIndex = i
    for j from i+1 to n-1:
        if arr[j] < arr[minIndex]:
            minIndex = j
    swap(arr[i], arr[minIndex])

// Insertion Sort
// Builds the sorted array one element at a time by inserting each new element into its correct position
for i from 1 to n-1:
    key = arr[i]
    j = i - 1
    while j >= 0 and arr[j] > key:
        arr[j + 1] = arr[j]
        j = j - 1
    arr[j + 1] = key

// Merge Sort
// Divides the array into halves, sorts each half, and merges them back together
MERGE_SORT(arr, left, right):
    if left >= right:
        return
    mid = (left + right) / 2
    MERGE_SORT(arr, left, mid)
    MERGE_SORT(arr, mid + 1, right)
    MERGE(arr, left, mid, right)

MERGE(arr, left, mid, right):
    create temporary array temp
    i = left
    j = mid + 1
    while i <= mid AND j <= right:
        if arr[i] <= arr[j]:
            add arr[i] to temp
            i++
        else:
            add arr[j] to temp
            j++
    while i <= mid:
        add arr[i] to temp
        i++
    while j <= right:
        add arr[j] to temp
        j++
    copy temp back into arr[left...right]

// Quick Sort
// Selects a "pivot" element and partitions the array into elements less than and greater than the pivot, then recursively sorts the partitions
QUICK_SORT(arr, low, high):
    if low >= high:
        return
    pivotIndex = PARTITION(arr, low, high)
    QUICK_SORT(arr, low, pivotIndex - 1)
    QUICK_SORT(arr, pivotIndex + 1, high)

PARTITION(arr, low, high):
    pivot = arr[high] // Or choose a random pivot for better performance
    i = low
    for j = low to high - 1:
        if arr[j] <= pivot:
            swap arr[i] and arr[j]
            i++
    swap arr[i] and arr[high]
    return i

// Heap Sort
// Builds a max heap and repeatedly extracts the maximum element to sort the array
HEAP_SORT(arr):
    n = length(arr)
    // Build max heap
    for i = n/2 - 1 down to 0:
        HEAPIFY(arr, n, i)
    // Move max to the end
    for end = n - 1 down to 1:
        swap arr[0] and arr[end]
        HEAPIFY(arr, end, 0)

HEAPIFY(arr, n, i):
    largest = i
    left  = 2*i + 1
    right = 2*i + 2
    if left < n AND arr[left] > arr[largest]:
        largest = left
    if right < n AND arr[right] > arr[largest]:
        largest = right
    if largest != i:
        swap arr[i] and arr[largest]
        HEAPIFY(arr, n, largest)

// Counting Sort
// Counts the occurrences of each unique element and calculates their positions in the sorted array
   max = maximum(arr)
   count = array of size max + 1, filled with 0
   // Count occurrences
   for x in arr:
       count[x]++
   // Rebuild sorted array
   index = 0
   for value = 0 to max:
       while count[value] > 0:
           arr[index] = value
           index++
           count[value]--

// Bucket Sort
// Distributes elements into buckets, sorts each bucket, and concatenates them
BUCKET_SORT(arr, bucketSize):
    if length(arr) == 0:
        return arr
    minValue = minimum(arr)
    maxValue = maximum(arr)
    bucketCount = (maxValue - minValue) / bucketSize + 1
    buckets = array of empty lists of size bucketCount
    // Distribute elements into buckets
    for x in arr:
        index = (x - minValue) / bucketSize
        add x to buckets[index]
    // Sort each bucket and concatenate
    sortedArray = empty list
    for bucket in buckets:
        SORT(bucket) // Use any sorting algorithm, e.g., insertion sort
        append bucket to sortedArray
    return sortedArray

// Radix Sort
// Sorts numbers digit by digit, starting from the least significant digit to the most significant digit
RADIX_SORT(arr):
    max = maximum(arr)
    exp = 1
    while max / exp > 0:
        COUNTING_SORT_BY_DIGIT(arr, exp)
        exp = exp * 10

COUNTING_SORT_BY_DIGIT(arr, exp):
    count[0..9] = all 0
    output = array of same size
    // Count digits
    for x in arr:
        digit = (x / exp) % 10
        count[digit]++
    // Prefix sums
    for i = 1 to 9:
        count[i] += count[i - 1]
    // Build output (right → left for stability)
    for i = length(arr) - 1 down to 0:
        digit = (arr[i] / exp) % 10
        output[count[digit] - 1] = arr[i]
        count[digit]--
    copy output into arr
```

## Quickselect

Average O(n); uses partition from quicksort. Finds kth smallest without full sort.

```csharp
int QuickSelect(int[] nums, int lo, int hi, int k)
{
    if (lo == hi) return nums[lo];
    int pivot = nums[hi], p = lo;
    for (int i = lo; i < hi; i++)
        if (nums[i] <= pivot) { (nums[i], nums[p]) = (nums[p], nums[i]); p++; }
    (nums[p], nums[hi]) = (nums[hi], nums[p]);
    if (p == k) return nums[p];
    return p > k ? QuickSelect(nums, lo, p - 1, k) : QuickSelect(nums, p + 1, hi, k);
}
```

| Sorting Algorithm | Time Complexity       | Space Complexity | Stable? | In-place? | When to Use                                     |
| ----------------- | --------------------- | ---------------- | ------- | --------- | ----------------------------------------------- |
| Bubble Sort       | O(n^2)                | O(1)             | ✅      | ✅        | Small datasets, educational purposes            |
| Selection Sort    | O(n^2)                | O(1)             | ❌      | ✅        | Small datasets, minimal writes                  |
| Insertion Sort    | O(n^2)                | O(1)             | ✅      | ✅        | Small datasets, nearly sorted data              |
| Merge Sort        | O(n log n)            | O(n)             | ✅      | ❌        | Large datasets, stable sort needed              |
| Quick Sort        | O(n log n) - (O(n^2)) | O(log n)         | ❌      | ✅        | Large datasets, average case performance        |
| Heap Sort         | O(n log n)            | O(1)             | ❌      | ✅        | Large datasets, in-place sorting                |
| Counting Sort     | O(n + k)              | O(k)             | ✅      | ❌        | Small range of integers, non-comparison sort    |
| Bucket Sort       | O(n + k)              | O(n + k)         | ✅      | ❌        | Uniformly distributed data, non-comparison sort |
| Radix Sort        | O(nk)                 | O(n + k)         | ✅      | ❌        | Large datasets of integers, non-comparison sort |

## C# Sort Internals

- **`Array.Sort`** — introspective sort (quicksort → heapsort fallback at depth limit). **Not stable**. O(n log n).
- **`List<T>.Sort`** — delegates to `Array.Sort`. **Not stable**.
- **`OrderBy` (LINQ)** — merge sort internally. **Stable**. Returns new sequence.
- **`Array.Sort` with `IComparer<T>`** — custom comparison, still not stable.
- For stable in-place sort: use `OrderBy(...).ToArray()` or implement stable sort.
