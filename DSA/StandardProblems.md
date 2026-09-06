## Arrays

### Maximum Subarray Sum

**Time:** `O(n)` | **Space:** `O(1)`

```
Iterate and calculate the running sum
Reset whenever running sum < current element
```

### Best time to buy and sell one stock

**Time:** `O(n)` | **Space:** `O(1)`

```
Reverse iterate and calculate the max of (suffix max - current element)
```

### Majority Element - Element occuring more than n/2

**Time:** `O(n)` | **Space:** `O(1)`

```
candidate = null
count = 0

for each num:
    if count == 0:
        candidate = num

    if num == candidate:
        count++
    else:
        count--
return candidate
```

### Product of Array Except Self

**Time:** `O(n)` | **Space:** `O(n)` for the prefix and suffix arrays

```
Create prefix product and suffix product array
Iterate and calculate prefix into suffix
```

### Max Product Subarray

**Time:** `O(n)` | **Space:** `O(1)`

```
Calculate prefix and suffix product. Reset to 1 whenever 0 encountered
Calculate max
```

### Find all duplicates in array of 1 to n

**Time:** `O(n)` | **Space:** `O(1)` excluding the result

```
Negate the index of element, if again negative found, mark duplicate
```

### Find duplicate number in O(1) space

**Time:** `O(n)` | **Space:** `O(1)`

```
Floyd Cycle Detection - Element Value Pointing to Index
slow = nums[slow]
fast = nums[nums[fast]]
```

### Sort Colors / Dutch National Flag - 3 Way Partition

**Time:** `O(n)` | **Space:** `O(1)`

```
lo = 0, mid = 0, hi = A.Length - 1;
while (mid <= hi)
    if (A[mid] == 0)      { Swap(A, lo++, mid++); }
    else if (A[mid] == 1) { mid++; }
    else                  { Swap(A, mid, hi--); }
```

### Rotate Array by k

**Time:** `O(n)` | **Space:** `O(1)`

```
k %= A.Length;
Reverse whole array;
Reverse till k - 1
Reverse from k to end
```

### Next Permutation

**Time:** `O(n)` | **Space:** `O(1)`

```
Start from end, find index position where num[i] < num[i+1]
Find rightmost element greater than index, swap
Reverse index+1 to end
```

### Spiral Matrix

**Time:** `O(rows * cols)` | **Space:** `O(1)` excluding the result

```
Keep top, left, bottom, right limits
while top <= bottom and left <= right
    j: left to right, top++
    i: top to bottom, right--
    if top <= bottom
        j: right to left, bottom--
    if left <= right
        i: bottom to top, left++
```

### Set matrix row col 0 if any cell 0

**Time:** `O(rows * cols)` | **Space:** `O(1)`

```
Mark 1st row and 1st column with 0
```

### Rotate 2D Array by 90 degree right

**Time:** `O(n^2)` | **Space:** `O(1)`

```
for i in 0 to len
    for j in i to len
        swap arr[i][j], arr[j][i]
reverse each row
```

## Strings

### Longest common prefix in all strings

**Time:** `O(C)` | **Space:** `O(1)` excluding the result, where `C` is the total number of characters examined

```
Scan vertically for each position
```

### Encode and Decode List of Strings

**Time:** `O(C)` | **Space:** `O(C)` for the encoded or decoded result, where `C` is the total character count

```
Convert to (Length + "#" + String)
```

## Hashing

### Check array contains duplicate

**Time:** `O(n)` average | **Space:** `O(n)`

```
Iterate and put elements in hashset
```

### Check 2 strings anagram

**Time:** `O(n + m)` | **Space:** `O(k)`, where `k` is the character-set size

```
Create frequency map and check both equal
```

### Two Sum

**Time:** `O(n)` average | **Space:** `O(n)`

```
Put elements in hashset, check Target-num[i] in hashset
```

### Group anagrams

**Time:** `O(n * k log k)` | **Space:** `O(n * k)`, where `k` is the maximum string length

```
Create map of string, list
Put sorted string as key, and add all belonging string to that list
```

### Subarray Sum Equal K

**Time:** `O(n)` average | **Space:** `O(n)`

```
Iterate and keep count of prefix sum
Count all prefixSumCount(currentSum - k)

prefixCount[0] = 1
foreach int n in nums
    sum += n
    count += prefixCount[sum-k]
    prefixCount[sum]++
```

### Longest consecutive sequence in array

**Time:** `O(n)` average | **Space:** `O(n)`

```
Put all elements in hashset
Iterate
    if element-1 not in hashset then then start counting from this element and calculate max
```

### Three Sum

**Time:** `O(n^2)` | **Space:** `O(n)` with a hash-based Two Sum, excluding the result

```
Iterate over array and fix one element
    Then run 2 sum on remaining array
```

## 2 Pointers

### Is Palindrome

**Time:** `O(n)` | **Space:** `O(1)`

```
Create 2 pointers, 0 and len-1
While pointers not equal
    skip non alpha characters
    if char at l and r equal then continue else return false
```

### Remove duplicates from sorted array

**Time:** `O(n)` | **Space:** `O(1)`

```
Slow and Fast Pointer at 0, replace fast elements with slow, skip duplicates
Same technique can be used to move 0s to end
```

### Two sum on sorted array

**Time:** `O(n)` | **Space:** `O(1)`

```
Two pointer at 0, len-1
Move the pointer to move closer to target
```

### Container with most water

**Time:** `O(n)` | **Space:** `O(1)`

```
Create 2 pointers, 0 and len-1
While (l<r)
    Calculate area and max
    Step the smaller length
```

### Trapping Rain Water

**Two pointers:** Time `O(n)`, Space `O(1)` | **Prefix/suffix arrays:** Time `O(n)`, Space `O(n)`

```
2 Pointers at 0, len-1
Running prefix and suffix max, Calculate outside max water from current height
Advance lower height, water = max - current height

while l < r
    if h[l] < h[r]
        maxL = Max(maxL, h[l])
        water += maxL - h[l++]
    else
        maxR = Max(maxR, h[r])
        water += maxR - h[r--]

or
Create prefixMax and suffixMax array
Calculate water at each index: water += min(pMax[i],sMax[i]) - h[i]
```

### Palindromic Substrings - Number of Palindromic Substring

**Expand centers:** Time `O(n^2)`, Space `O(1)` | **DP:** Time `O(n^2)`, Space `O(n^2)`

```
Expand around each character as center using 2 pointers
2 Cases - Odd Length, Even Length

OR
dp[][]
```

## Sliding Window

### Longest substring without repeating character

**Time:** `O(n)` | **Space:** `O(k)`, where `k` is the character-set size

```
Sliding window with variable width. Keep characters in hashset

Optimized - Keep last occurrence, and skip to last occurrence
```

### Longest substring with same character if replacement of k characters possible

**Time:** `O(n)` | **Space:** `O(k)`, where `k` is the character-set size

```
Create 2 pointers at 0
Valid window condition = window size - max frequent character <= k
```

### Minimum window / substring containing all character of t

**Time:** `O(|s| + |t|)` | **Space:** `O(k)`, where `k` is the character-set size

```
Keep map of needed string and map of window
Keep track of formed and required distinct characters to optimize map equality
Sliding window technique
```
### Count of subarray with products less than k given elements >= 1
```
Expand window till product k, calculate count of array starting at l
```
```cs
int len = nums.Length;
int l = 0, r = 0;
int curr = 1;
int count = 0;
while(l < len && r < len){
    curr *= nums[r];
    while(curr >= k && l < r){
        curr /= nums[l++];
    }
    count += (curr < k) ? r-l+1 : 0;
    r++;
}
return count;
```
## Stack and Queue

### Valid Paranthesis

**Time:** `O(n)` | **Space:** `O(n)`

```
If opening bracket, push closing bracket to stack
If closing bracket, match and pop
Stack should be empty at the end
```

### Evaluate Postfix

**Time:** `O(n)` | **Space:** `O(n)`

```
Push operand to stack
Pop 2 when operation, then push ans
```

### Min Stack

**Time:** `O(1)` per operation | **Space:** `O(n)`

```
Create 2 stack, one for element and one for min seen till now.
Push and Pop together
```

### Queue with 2 Stack

**Time:** Enqueue `O(1)`, dequeue `O(1)` amortized | **Space:** `O(n)`

```
2 Stacks - in and out
Enqueue
    Push to in
Dequeu
    if out is empty, pop everything from in to out
    pop out
```

### Stack with 1 Queue

**Time:** Push `O(n)`, pop and peek `O(1)` | **Space:** `O(n)`

```
Push
    q.Enqueu
    for all elements in q
        q.Enqueue(q.Dequeue) // Rotate
Pop
    q.Dequeu
Peek
    q.Peek
```

### Sliding Window Maximum - Calculate all max element for all window of size k

**Deque:** Time `O(n)`, Space `O(k)` | **Max heap:** Time `O(n log n)`, Space `O(n)`

```
Create dequeue to put indices of monotonic decreasing elements
Iterate
    Remove expired element from front
    Remove smaller than current elements from back
    Add current index to back
    front is max of current window

or
Use Max Heap, but O(NLogN)
```

### Next Greater Element II - Circular array, Find next greater element

**Time:** `O(n)` | **Space:** `O(n)`

```
Assume array of twice length and process whole
```

### Remove k digits from number to make it smallest

**Time:** `O(n)` | **Space:** `O(n)`

```
> If any number has some greater number on the left, remove it

Iterate digits left to right
Whenever the current digit is smaller than the previous kept digit in stack, remove the previous larger digit
```

### Largest Rectangle in Histogram

**Time:** `O(n)` | **Space:** `O(n)`

```
Calculate 2 array using monotonic stack for leftSmaller, rightSmaller
for each element calculate area = height * width of left to right smaller
```

## Linked List

### Merge 2 sorted link lists

**Time:** `O(n + m)` | **Space:** `O(1)`

```
Take 2 pointers at heads of 2 list, p1 and p2
Take another pointer to mark new head and curr pointer
while p1 and p2 not null
    Iterate and update the curr and p1 and p2
Attach the non empty pointer to curr
```

### Remove Nth Node from End

**Time:** `O(n)` | **Space:** `O(1)`

```
Calculate length
Go to len-N node and detach
```

### Check Link List Palindrome

**Time:** `O(n)` | **Space:** `O(1)`

```
Find Middle
Reverse Second Half
Compare
```

### Reorder Link List

**Time:** `O(n)` | **Space:** `O(1)`

l[0]-l[len-1]-l[1]-l[len-2]...

```
Find middle
Break
Reverse 2nd half
Merge 2 lists one by one
```

### Copy List with Random Pointer

**Dictionary:** Time `O(n)`, Space `O(n)` | **Interleaving:** Time `O(n)`, Space `O(1)`

```text
Dictionary approach:
    First pass creates a copy for every original node
    Second pass assigns copy.next and copy.random through the map

Interleaving approach:
    Insert each copy directly after its original node
    copy.random = original.random.next
    Separate the original and copied lists
```

### Merge K Sorted Link List

**Time:** `O(N log k)` | **Space:** `O(k)`, where `N` is the total node count

```
Create start and curr List Node
Put all heads into min heap
Iterate over heap
    pop head and attach to curr
    push head->next
```

### LRU Cache

**Time:** `O(1)` average for `Get` and `Put` | **Space:** `O(capacity)`

```text
Use a dictionary from key to node and a doubly linked list
The head side is most recently used; the tail side is least recently used

Get: look up the node and move it to the front
Put: update or insert at the front
If over capacity, remove the tail node from both the list and dictionary
```

## Binary Search

### Find min in rotated sorted array

**Time:** `O(log n)` | **Space:** `O(1)`

```
One half sorted.
while(l < r)
    if n[m] > n[r]
        l = m+1
    else
        r = m
return n[r]
```

### Find in rotated sorted array

**Time:** `O(log n)` | **Space:** `O(1)`

```
Check the sorted half
Then check if element in sorted half or not
```

### Koko Eating Bananas

Koko has `n` piles and `h` hours. Find the minimum integer eating speed that finishes every pile within `h` hours.

**Example:** `piles = [3, 6, 7, 11], h = 8` → `4`

```text
BRUTE FORCE | O(N * max(piles)) | O(1)

Try every speed from 1 through max(piles).

-----------------------------------------------------------------------------

BINARY SEARCH ON ANSWER | O(N log(max(piles))) | O(1)

left = 1
right = max(piles)
while left <= right:
    speed = left + (right - left) / 2
    hours = 0
    for pile in piles:
        hours += ceil(pile / speed)
    if hours <= h:
        right = speed - 1
    else:
        left = speed + 1
return left
```

## Heap

### Top K Frequent Elements

**Min heap:** Time `O(n log k)`, Space `O(n)` | **Buckets:** Time `O(n)`, Space `O(n)`

```
Create frequency map
Create min heap of size k, and push all the elements in map

OR

Create frequency map
Create buckets of size len
Push element to correct bucket
Iterate over bucket lists
```

### Running Median

**Time:** `O(log n)` per insertion and `O(1)` per median lookup | **Space:** `O(n)`

```
Create 2 heaps, min and max heap for upper and lower half of data
Push to max heap if x <= top
Rebalance to keep max heap - min heap <= 1

```

### Task Scheduler

**Heap simulation:** Time `O(T log k)`, Space `O(k)` | **Counting formula:** Time `O(T + k)`, Space `O(k)`

```text
Count each task frequency
Always run the available task with the highest remaining frequency
Keep cooling tasks in a queue with their next available time

OR

maxFreq = maximum frequency
countMax = number of tasks with maxFreq
answer = max(T, (maxFreq - 1) * (cooldown + 1) + countMax)
```

## Tree

### Is Same Tree

**Time:** `O(n)` | **Space:** `O(h)` recursion stack, where `n` is the number of compared nodes

```
IsSame(p,q)
    check p, q same or null
    return IsSame(p.left, q.left) AND IsSame(p.right, q.right)
```

### Invert Binary Tree

**Time:** `O(n)` | **Space:** `O(h)` recursion stack

```
Invert(root)
    (root.left, root.right) = (root.right, root.left)
    Invert(root.left)
    Invert(root.right)
```

### Path Sum - Get Path root to leaf is target

**Time:** `O(n)` | **Space:** `O(h)` recursion stack

```
DFS and check sum
```

### Valid BST

**Time:** `O(n)` | **Space:** `O(h)` recursion stack

```
Check root divides left and right subtree values and no duplicates

IsValid(root)
    check any null, return true
    return IsValidRange(root, intMin, intMax)

IsValidRange(root, min, max)
    if null, return true
    if root <= min or root >= max return false
    return IsValidRange(root.left, min, root.val) AND IsValidRange(root.right, root.val, max)
```

### Kth Smallest value in BST

**Time:** `O(h + k)` | **Space:** `O(h)`

```
In order traversal gives sorted order, So traverse and stop at k
```

### LCA of BST

**Time:** `O(h)` | **Space:** `O(h)` recursive or `O(1)` iterative

```
LCA of tree should be dividing both p and q as left and right subtree

LCA(root, p, q)
    check any null, return null
    if root less than both, check right
    if root more than both, check left
    return root
```

### LCA of Binary Tree

**Time:** `O(n)` | **Space:** `O(h)` recursion stack

```
LCA should divide the p and q in different subtree

LCA(root, p, q)
    if root is null, p, q
        return root
    left = LCA root.left
    right = LCA root.right
    if left and right, then return root
    return left ?? right
```

### Subtree of another tree

**Time:** `O(N * M)` worst case | **Space:** `O(H + h)` recursion stack

```
IsSubTree(r, sr)
    check both null, return true
    check either null, return false
    return IsSame(r, sr) || IsSubTree(r.left, sr) || IsSubTree(r.right, sr)
```

### Construct tree from Preorder and Inorder

**Time:** `O(n)` | **Space:** `O(n)` for the index map and recursion stack

```
// Use a hashmap to store the indices of the inorder values for O(1) lookups. Recursively build the tree using the preorder array to determine the root nodes.

BUILD(preorder, inorder):
    inorderIndex = hashmap()
    for i from 0 to inorder.length - 1:
        inorderIndex[inorder[i]] = i
    preorderIndex = 0
    BUILD_TREE(left, right):
        if left > right:
            return null
        rootValue = preorder[preorderIndex]
        preorderIndex++
        root = new Node(rootValue)
        mid = inorderIndex[rootValue]
        root.left = BUILD_TREE(left, mid - 1)
        root.right = BUILD_TREE(mid + 1, right)
        return root
    return BUILD_TREE(0, inorder.length - 1)
```

### Serialize and Deserialize Tree

**Time:** `O(n)` | **Space:** `O(n)` for serialized data and deserialization tokens, plus `O(h)` recursion stack

```
Preorder
// Serialize
DFS(root)
    if root is null
        s += "null,"
        return
    s += root.val + ","
    DFS root.left
    DFS root.right

// Deserialize
q = Queue(s.split",")
Build()
    v = q.Dequeue
    if v is null
        return null
    return new TreeNode(int(v), Build(), Build())
```

### Max Path Sum through any node

**Time:** `O(n)` | **Space:** `O(h)` recursion stack

```
int _maxPath = int.MinValue;
int MaxGain(TreeNode node)
{
    if (node == null) return 0;
    int l = Math.Max(0, MaxGain(node.Left));   // ignore negative branches
    int r = Math.Max(0, MaxGain(node.Right));
    _maxPath = Math.Max(_maxPath, node.Val + l + r);
    return node.Val + Math.Max(l, r);           // return single-branch gain
}
```

## Graph

### Number of Island in Grid of 0s and 1s

**Time:** `O(rows * cols)` | **Space:** `O(rows * cols)` worst case

```
DFS on all non visited 1s and in all 4 direction
Number of DFS calls is number of components
```

### Clone Graph

**Time:** `O(V + E)` | **Space:** `O(V)` excluding the cloned graph

```
Create hashmap to track visited node
Do DFS on graph, track visited and add cloned neighbour to clone nodes
```

### Rotten Oranges, Determine minimum time in which all fresh orange becomes rotten

**Time:** `O(rows * cols)` | **Space:** `O(rows * cols)` worst case

```
Multi Source BFS
Add all rotten orranges in queue
At each level, visit all nodes in that level and make adjacent rotten
```

### Bipartite Graph - 0-1 Coloring

**Time:** `O(V + E)` | **Space:** `O(V)`

```text
for every uncolored node:
    color[node] = 0
    BFS(node)
        for neighbor:
            if neighbor uncolored:
                color[neighbor] = opposite color
            else if color[neighbor] == color[node]:
                return false
return true
```

### Graph Valid Tree

**Time:** `O(V + E)` | **Space:** `O(V)`

```
Number of edges = N-1
DFS/BFS if no cycle
```

### Redundant Connection - Find edge creating a cycle

**Time:** `O(E * alpha(V))` | **Space:** `O(V)` for Union-Find

```
Run Union Find on graph edges, is union returns false, that edge creates a cycle
```

### Pacific Atlantic Ocean - Number of cells that can reach left-top or right-bottom margin

**Time:** `O(rows * cols)` | **Space:** `O(rows * cols)`

```
DFS from each cell on border - Left-Top and Right-Bottom in opposite direction
Intersection gives results
```

### Word Ladder - Given source and destination word, minimum traversal in list of words to reach

**Time:** `O(N * L^2)` | **Space:** `O(N * L)`, where `N` is the word count and `L` is the word length

```
Unweighted shortest path problem - BFS
Words with one letter difference adjacent
```

### Network Delay Time

**Dijkstra:** Time `O((V + E) log V)` | **Space:** `O(V + E)`

```text
Build a weighted adjacency list
Set distance[source] = 0 and push it into a min heap
Repeatedly pop the closest unvisited node and relax its outgoing edges
Return the maximum finite distance, or -1 if any node is unreachable
```

### Alien Dictionary - Words given in order, find order of letters

**Time:** `O(C + V + E)` | **Space:** `O(V + E)`, where `C` is the total input character count

```
Create edges and Topological Sorting
```

## Backtracking

### Subsets

Given an integer array nums of unique elements, return all possible subsets (the power set).

**Example:** `nums = [1, 2, 3]` → `[[],[1],[2],[3],[1,2],[1,3],[2,3],[1,2,3]]`

```cs
BACKTRACKING | O(N * 2^N) | O(N)

At each element, choose to include it in the current subset or not. Recursively build all subsets.

function subsets(nums):
    result = []
    current = []
    backtrack(index):
        if index == length(nums):
            result.add(copy(current))
            return
        // Choice 1: include nums[index]
        current.add(nums[index])
        backtrack(index + 1)
        current.removeLast()
        // Choice 2: don't include nums[index]
        backtrack(index + 1)
    backtrack(0)
    return result
```

### Subsets II

Given an integer array nums that may contain duplicates, return all possible subsets (the power set) without duplicate subsets.

**Example:** `nums = [1, 2, 2]` → `[[],[1],[2],[1,2],[2,2],[1,2,2]]`

```cs
BACKTRACKING | O(N * 2^N) | O(N)

Sort the array to handle duplicates. At each element, choose to include it in the current subset or not, skipping duplicates.

function subsetsWithDup(nums):
    sort(nums)
    result = []
    current = []
    backtrack(start):
        result.add(copy(current))
        for i from start to n - 1:
            // Skip duplicate choices
            // at the same recursion level.
            if i > start AND nums[i] == nums[i - 1]:
                continue
            // Choose
            current.add(nums[i])
            // Explore
            backtrack(i + 1)
            // Undo
            current.removeLast()
    backtrack(0)
    return result
```

### Permutations

Given an integer array nums of unique elements, return all possible permutations.

**Example:** `nums = [1, 2, 3]` → `[[1,2,3],[1,3,2],[2,1,3],[2,3,1],[3,1,2],[3,2,1]]`

```cs
BACKTRACKING | O(N * N!) | O(N)

For each position in the permutation, choose an unused number from nums and recursively build the permutation.

function permutations(nums):
    result = []
    current = []
    used = array of false
    backtrack():
        if current.size == nums.length:
            result.add(copy(current))
            return
        for i from 0 to nums.length - 1:
            if used[i]:
                continue
            // Choose
            used[i] = true
            current.add(nums[i])
            // Explore
            backtrack()
            // Undo
            current.removeLast()
            used[i] = false
    backtrack()
    return result


-----------------------------------------------------------------------------

SWAP | O(N * N!) | O(N)

// At each recursion level, swap the current index with each of the remaining indices to generate permutations.

function permutations(nums):
    result = []
    backtrack(start):
        if start == nums.length:
            result.add(copy(nums))
            return
        for i from start to nums.length - 1:
            swap(nums[start], nums[i])
            backtrack(start + 1)
            swap(nums[start], nums[i])  // undo
    backtrack(0)
    return result
```

### Combination Sum

Given an array of distinct integers candidates and a target integer target, return a list of all unique combinations of candidates where the chosen numbers sum to target. You may return the combinations in any order. The same number may be chosen from candidates an unlimited number of times.

**Example:** `candidates = [2, 3, 6, 7], target = 7` → `[[2,2,3],[7]]`

```text
BACKTRACKING | O(N^(T/m)) | O(T/m)
Where T is the target and m is the minimum value in candidates. The maximum depth of the recursion tree is T/m, and at each level, we have N choices (the number of candidates).

function combinationSum(candidates, target):
    sort(candidates)
    result = []
    current = []
    backtrack(start, remaining):
        if remaining == 0:
            result.add(copy(current))
            return
        for i from start to n - 1:
            if candidates[i] > remaining:
                break
            current.add(candidates[i])
            // i, not i+1
            backtrack(i, remaining - candidates[i])
            current.removeLast()
    backtrack(0, target)
    return result
```

### Combination Sum II

Given a collection of candidate numbers (candidates) and a target number (target), find all unique combinations in candidates where the candidate numbers sum to target. Each number in candidates may only be used once in the combination. Input may contain duplicates.

**Example:** `candidates = [10,1,2,7,6,1,5], target = 8` → `[[1,1,6],[1,2,5],[1,7],[2,6]]`

```text
BACKTRACKING | O(N * 2^N) | O(N)
Every element is either taken or skipped, and copying each valid combination costs O(N).

// Sort the candidates
// Do not reuse the same element in the same recursion level
// Skip duplicates in the same recursion level

function combinationSum2(candidates, target):
    sort(candidates)
    result = []
    current = []
    backtrack(start, remaining):
        if remaining == 0:
            result.add(copy(current))
            return
        for i from start to n - 1:
            // Skip duplicate choices
            // at the same recursion level.
            if i > start AND
               candidates[i] == candidates[i - 1]:
                continue
            // Since sorted, nothing after this can fit.
            if candidates[i] > remaining:
                break
            // Choose
            current.add(candidates[i])
            // Cannot reuse this element.
            backtrack(i + 1, remaining - candidates[i])
            // Undo
            current.removeLast()
    backtrack(0, target)
    return result
```

### Search word in 2D Matrix

**Time:** `O(rows * cols * 3^L)` worst case | **Space:** `O(L)` recursion stack

```cs
Backtrack(board[][], r, c, word, index)
    // Base case
    if(index == word.Length)
        return true
    if(b[r][c] == word[index])
        // Mark
        b[r][c] = '#'
        // Backtrack in all 4 direction
        ...
        // Undo
        b[r][c] = word[index]
```

### Generate Parentheses

Given n pairs of parentheses, write a function to generate all combinations of well-formed parentheses.

**Example:** `n = 3` → `["((()))","(()())","(())()","()(())","()()()"]`

```text
BACKTRACKING | O(4^N / sqrt(N)) | O(N)
The number of valid sequences is the Nth Catalan number, and each one costs O(N) to build.

function generateParenthesis(n):
    result = []
    current = ""
    backtrack(open, close):
        if length(current) == 2 * n:
            result.add(current)
            return
        if open < n:
            current += "("
            backtrack(open + 1, close)
            current remove last character
        if close < open:
            current += ")"
            backtrack(open, close + 1)
            current remove last character
    backtrack(0, 0)
    return result
```

### N-Queens

Place n queens on an n × n chessboard so that no two queens attack each other. Return all distinct solutions.

**Example:** `n = 4` → `[[".Q..","...Q","Q...","..Q."],["..Q.","Q...","...Q",".Q.."]]`

```text
BRUTE FORCE | O(N^N) | O(N)

Try every column for every row and validate the full board at the end

-----------------------------------------------------------------------------

BACKTRACKING + CONFLICT SETS | O(N!) | O(N)

// Place one queen per row, so only columns and the two diagonals can conflict
// Anti-diagonal is constant along row + col
// Main diagonal is constant along row - col

function solveNQueens(n):
    result = []
    columns = empty set
    diagonal = empty set          // row - col
    antiDiagonal = empty set      // row + col
    position = array of size n
    backtrack(row):
        if row == n:
            result.add(board built from position)
            return
        for col from 0 to n - 1:
            if col in columns
               OR (row - col) in diagonal
               OR (row + col) in antiDiagonal:
                continue
            // Choose
            add col, row - col, row + col to the sets
            position[row] = col
            // Explore
            backtrack(row + 1)
            // Undo
            remove col, row - col, row + col from the sets
    backtrack(0)
    return result
```

> - N-Queens II only needs the count, so the board never has to be materialised.
> - Bitmask version stores the three sets in integers and uses `available = ~(cols | diag | anti)`.

## Greedy and Intervals

### Meeting Rooms I - Check No Overlap

**Time:** `O(n log n)` | **Space:** `O(log n)` for the in-place sort stack

```
Sort by start time, check adjacent end and start time
```

### Merge Intervals

**Time:** `O(n log n)` | **Space:** `O(n)` for the heap and result

```
Insert all in Max Heap on end time, Sort in decreasing order of end time
Pop, if still intersect with top, pop again and push merged
```

### Insert Interval

**Time:** `O(n)` | **Space:** `O(n)` for the result

```
3 Cases
1. Interval totally before, append
2. Interval in between, skip and calculate min and max for start and end, insert
3. Interval totally after, append
```

### Max number of non overlapping intervals

**Time:** `O(n log n)` | **Space:** `O(log n)` for the in-place sort stack

```
Sort by end time
Iterate and Greedily pick non overlapping interval
```

### Minimum intervals to remove to make others non overlapping

**Time:** `O(n log n)` | **Space:** `O(log n)` for the in-place sort stack

```
Sort by END ascending
↓
Keep earliest-ending interval
↓
If next.start < currentEnd → remove it
↓
Otherwise → keep it and update currentEnd
```

### Meeting Rooms II - Number of Meeting Rooms Required

**Time:** `O(n log n)` | **Space:** `O(n)`

```
Put +1 with start time, -1 with end time
Put in same array and sort
Iterate and keep the max count
```

### Minimum number of arrows to burst balloons

**Time:** `O(n log n)` | **Space:** `O(log n)` for the in-place sort stack

```
Sort the balloons by their end points
shoot arrows at the end of each balloon, skipping any balloons that overlap with the last shot arrow.
```

### Jump Game - Check if can reach the end if max jump size at each step given

**Time:** `O(n)` | **Space:** `O(1)`

```
Iterate and calculate max reach possible
max = num[0]
for i in 1 to len-1
    if max < i
        break
    max = Max(max, i + num[i])
return (maxReach >= len-1)
```

### Minimum Jumps required to reach end

**Time:** `O(n)` | **Space:** `O(1)`

```
Keep track of the current range of reachable indices and the farthest index reachable in the next jump.

jumps = 0                   // Number of jumps made so far
currentEnd = 0              // The farthest index reachable with the current number of jumps
farthest = 0                // The farthest index reachable with one more jump
n = length(nums)
for i from 0 to n - 2:
    farthest = max(
        farthest,
        i + nums[i]
    )
    if i == currentEnd:
        jumps++
        currentEnd = farthest
return jumps
```

### Check which gas station can reach all station in circle

**Time:** `O(n)` | **Space:** `O(1)`

```
If total gas ≥ total cost, a solution always exists. Start from any point where running sum drops below 0 → reset start to next index.
If starting at start causes the tank to become negative at station i, then none of the stations between start and i can be a valid starting point either.
```

## DP

### Climbing Stairs, Number of ways to reach end if 1 or 2 steps

**Time:** `O(n)` | **Space:** `O(n)`, reducible to `O(1)`

```
dp[0] = 0, dp[1] = 1, dp[2] = 2
dp[i] = dp[i-1] + dp[i-2]
```

### Number of paths from top left to bottom right in a grid with down or right movement

**Time:** `O(rows * cols)` | **Space:** `O(rows * cols)`, reducible to `O(cols)`

```
dp[i][j] = dp[i-1][j] + dp[i][j-1];
```

### Minimum sum of path from top left to bottom right in a grid with down or right movement

**Time:** `O(rows * cols)` | **Space:** `O(rows * cols)`, reducible to `O(cols)`

```
dp[r][c] = minimum cost to reach (r,c)
dp[r][c] = grid[r][c] + min(dp[r-1][c], dp[r][c-1])
```

### House Robber - Max amount that can be robbed without adjacent house robbed

**Time:** `O(n)` | **Space:** `O(n)`, reducible to `O(1)`

```
dp[i] = Math.Max
        (nums[i] + dp[i-2], // Rob Current House
        dp[i-1])            // Skip Current House
```

### House Robber 2 - House arranged in circle

**Time:** `O(n)` | **Space:** `O(n)` as shown, reducible to `O(1)`

```
2 DPs : Rob House 1, Skip House 1

dp[0] = nums[0];
dp[1] = Math.Max(nums[0],nums[1]);
dp2[0] = 0;
dp2[1] = nums[1];
for(int i = 2; i < len; i++){
    dp[i] = Math.Max(nums[i]+dp[i-2], dp[i-1]);
    dp2[i] = Math.Max(nums[i]+dp2[i-2], dp2[i-1]);
}
return Math.Max(dp[len-2], dp2[len-1]);
```

### House Robber 3 - House arranged as tree

```
DP | O(N) | O(H)

// For each node calculate rob[node] and notRob[node].
// rob = node.value + left.notRob + right.notRob
// notRob = max(left.rob, left.notRob) + max(right.rob, right.notRob)

HOUSE_ROBBER(root):
    DFS(node):
        if node is null:
            return (0, 0)
        left = DFS(node.left)
        right = DFS(node.right)
        rob = node.value + left.notRob + right.notRob
        notRob = max(left.rob, left.notRob) + max(right.rob, right.notRob)
        return (rob, notRob)
    result = DFS(root)
    return max(result.rob, result.notRob)
```

### Decode Ways - Number String to Letters

**Time:** `O(n)` | **Space:** `O(n)`, reducible to `O(1)`

```
dp[i] = number of ways to decode first i characters
if One Digit: dp[i] += dp[i - 1]
if Two Digits: dp[i] += dp[i - 2]
```

### Word Break - Check string can be formed using words in dictionary

**Time:** `O(n^2)` dictionary checks | **Space:** `O(n)` DP, excluding substring copies

```
bool dp[i] = whether first i characters can be segmented

dp[0] = true
for i = 1 to n:
    for j = 0 to i - 1:
        if dp[j] == true:
            word = s[j...i]
            if word in dictionary:
                dp[i] = true
                break
return dp[n]
```

### Partition array into subset of equal sum

**Time:** `O(n * target)` | **Space:** `O(target)`, where `target = total / 2`

```
0/1 Knapsack with sum = total / 2
```

### Convert one string to another by insert, delete or replace

**Time:** `O(m * n)` | **Space:** `O(m * n)`, reducible to `O(min(m, n))`

```
dp[i][j] = minimum edit distance between first i characters of s1 and first j characters of s2

for i = 0 to m:
    dp[i][0] = i
for j = 0 to n:
    dp[0][j] = j
for i = 1 to m:
    for j = 1 to n:
        if word1[i - 1] == word2[j - 1]:
            dp[i][j] = dp[i - 1][j - 1]  // No operation needed
        else:
            dp[i][j] = 1 + min(
                    dp[i - 1][j],     // delete
                    dp[i][j - 1],     // insert
                    dp[i - 1][j - 1])  // replace
return dp[m][n]
```

### Best Time to Buy and Sell Stock with Cooldown of 1 day after selling

**Time:** `O(n)` | **Space:** `O(1)`

```
STATE MACHINE DP

// Each day, we can be in one of three states. We track the maximum profit for each state.
// hold = max profit if we are holding a stock
// sold = max profit if we just sold a stock
// rest = max profit if we are in cooldown or just waiting

STOCK_WITH_COOLDOWN(prices):
    hold = -infinity
    sold = 0
    rest = 0
    for price in prices:
        previousHold = hold
        previousSold = sold
        previousRest = rest
        hold = max(previousHold, previousRest - price)
        sold = previousHold + price
        rest = max(previousRest, previousSold)
    return max(sold, rest)
```

### Longest increasing path in matrix

**Time:** `O(rows * cols)` | **Space:** `O(rows * cols)` for memoization and recursion

```
For every cell, do dp
dp[r][c] = Length of longest path starting from r, c
Do DFS with memo
For each direction: best = max(best, 1 + DFS(nr, nc))
```

## Bit

### Every number occurs twice except one

**Time:** `O(n)` | **Space:** `O(1)`

```
Xor all elements
```

### Find missing number 1 to n

**Time:** `O(n)` | **Space:** `O(1)`

```
xor all numbers 1 to n, then xor with array. Remaining is the missing
A ^ A = 0, A ^ 0 = A
```

### Count number of bits

**Time:** `O(p)` | **Space:** `O(1)`, where `p` is the number of set bits

```
n & (n - 1) removes last set bit, count till n becomes 0
```

### Sum 2 integers

**Time:** `O(w)` | **Space:** `O(1)`, where `w` is the integer bit width

```
while b != 0:
    carry = (a AND b) << 1
    a = a XOR b
    b = carry
return a
```
