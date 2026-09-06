# Heaps and Priority Queues

- Complete Binary Tree
- Max Heap: Parent node >= Child nodes
- Min Heap: Parent node <= Child nodes
- Applications: Priority Queue, Heap Sort, Graph Algorithms (Dijkstra's, Prim's)

![alt text](image-5.png)

**Array representation of Heap:**

- For node at index i:
  - Left child index = 2 \* i + 1
  - Right child index = 2 \* i + 2
  - Parent index = (i - 1) / 2
  - Root node is at index 0
  - Leaf nodes are at indices n/2 to n-1 (0-based indexing)

**Operations on Heap:**

- Insert
  - Insert at end > Heapify up (compare with parent and swap if necessary)
- Delete
  - Replace root with last element > Heapify down (compare with children and swap if necessary)
- Build Heap
  - Assume all elements are in the array, leaf nodes are already heaps
  - Operate on all non-leaf nodes from bottom to top and heapify each node

```cs
INSERT_HEAP(arr, key):
    arr.append(key)
    i = length(arr) - 1
    while i != 0 and arr[PARENT(i)] < arr[i]:
        swap(arr[i], arr[PARENT(i)])
        i = PARENT(i)

DELETE_HEAP(arr, key):
    index = FIND_INDEX(arr, key)
    if index == -1:
        return
    arr[index] = arr[length(arr) - 1]
    arr.pop()
    HEAPIFY(arr, length(arr), index)

HEAPIFY(arr, n, i):
    largest = i
    left = 2 * i + 1
    right = 2 * i + 2

    if left < n and arr[left] > arr[largest]:
        largest = left

    if right < n and arr[right] > arr[largest]:
        largest = right

    if largest != i:
        swap(arr[i], arr[largest])
        HEAPIFY(arr, n, largest)
```
