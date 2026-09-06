## Arrays

## Car Pooling

Given trips `[passengers, start, end]` and a vehicle capacity, determine whether all trips can be completed without exceeding the capacity.

**Example:** `trips = [[2,1,5],[3,3,7]], capacity = 4` → `false`

````text
BRUTE FORCE | O(N * range) | O(range)

For every trip, add its passengers to each stop in [start, end)

-----------------------------------------------------------------------------

DIFFERENCE ARRAY | O(N + range) | O(range)

// Mark only the boundaries, then a prefix sum reconstructs every value
// Turns range updates into O(1) each

for each [passengers, start, end]:
    diff[start] += passengers
    diff[end] -= passengers

current = 0
for location = 0 to range - 1:
    current += diff[location]
    if current > capacity:
        return false
return true

-----------------------------------------------------------------------------

SWEEP LINE | O(N log N) | O(N)

Sort the boundary events and scan them; use this when coordinates are large or unbounded
Process drop-offs before pick-ups at the same coordinate
```s
````

## DP

### Burst Balloons

You are given n balloons, indexed from 0 to n - 1. Each balloon is painted with a number on it represented by an array nums. You are asked to burst all the balloons. If you burst balloon i you will get nums[left] _nums[i]_ nums[right] coins. Here left and right are adjacent indices of i. After the burst, the left and right then become adjacent. Find maximum coins you can collect by bursting the balloons wisely. You may imagine nums[-1] = nums[n] = 1. They are not real therefore you cannot burst them.

**Example:** `nums = [3, 1, 5, 8]` → `167`

```text
INTERVAL DP | O(N^3) | O(N^2)

// dp[left][right] = max coins from bursting every balloon in [left, right]
// Pick i as the LAST balloon burst in the range, so its neighbours are then nums[left - 1] and nums[right + 1] (1 outside the array)
// dp[left][right] = max over i in [left, right] of
//     dp[left][i - 1] + dp[i + 1][right] + nums[left - 1] × nums[i] × nums[right + 1]
// Choosing the last burst (not the first) is what makes the subproblems independent

BURST_BALLOONS(nums):
    n = nums.length
    dp = 2D array of n x n
    for length = 1 to n:
        for left = 0 to n - length:
            right = left + length - 1
            for i = left to right:
                coins = nums[i]
                if left > 0:
                    coins *= nums[left - 1]
                if right < n - 1:
                    coins *= nums[right + 1]
                if i > left:
                    coins += dp[left][i - 1]
                if i < right:
                    coins += dp[i + 1][right]
                dp[left][right] = max(dp[left][right], coins)
    return dp[0][n - 1]
```

### Regular Expression Matching

Given an input string s and a pattern p supporting `.` (any single character) and `*` (zero or more of the preceding element), determine whether the pattern matches the entire string.

**Example:** `s = "aab", p = "c*a*b"` → `true`

```text
RECURSION | O(2^(M + N)) | O(M + N)

MATCH(i, j):
    if j == p.length:
        return i == s.length
    firstMatches = i < s.length
                   AND (p[j] == s[i] OR p[j] == '.')
    if j + 1 < p.length AND p[j + 1] == '*':
        // Zero occurrences, or consume one character and stay on the same pattern
        return MATCH(i, j + 2)
               OR (firstMatches AND MATCH(i + 1, j))
    return firstMatches AND MATCH(i + 1, j + 1)

------------------------------------------------------------------------------

2D DP | O(M * N) | O(M * N)

// dp[i][j] = does the first i characters of s match the first j characters of p

dp[0][0] = true
for j = 1 to n:
    if p[j - 1] == '*':
        dp[0][j] = dp[0][j - 2]      // "x*" matches the empty string

for i = 1 to m:
    for j = 1 to n:
        if p[j - 1] == '*':
            dp[i][j] = dp[i][j - 2]                       // Zero occurrences
            if p[j - 2] == s[i - 1] OR p[j - 2] == '.':
                dp[i][j] = dp[i][j] OR dp[i - 1][j]        // One more occurrence
        else if p[j - 1] == s[i - 1] OR p[j - 1] == '.':
            dp[i][j] = dp[i - 1][j - 1]
        else:
            dp[i][j] = false

return dp[m][n]
```

> Wildcard matching (`?` and `*` where `*` matches any sequence) uses the same table with `dp[i][j] = dp[i - 1][j] OR dp[i][j - 1]` for `*`.

### Partition to K Equal Sum Subsets

Given an integer array `nums` and an integer `k`, determine whether it is possible to divide the array into `k` non-empty subsets with equal sums.

**Example:** `nums = [4, 3, 2, 3, 5, 2, 1], k = 4` → `true` (`[5], [1,4], [2,3], [2,3]`)

```text
BACKTRACKING WITH PRUNING | O(K * 2^N) | O(N)

sort descending and place large numbers first
skip a bucket that already failed with the same remaining capacity
fail fast when total % k != 0 or max(nums) > target

-----------------------------------------------------------------------------

BITMASK DP | O(2^N * N) | O(2^N)

// mask = which elements have been used
// Because elements are always placed in order, sum(mask) determines both the number
// of completed buckets and how full the current bucket is
// remainder[mask] = space already used in the current bucket

target = total / k
reachable[0] = true
remainder[0] = 0

for mask = 0 to 2^n - 1:
    if not reachable[mask]:
        continue
    for i = 0 to n - 1:
        if i is already in mask:
            continue
        if remainder[mask] + nums[i] > target:
            continue
        next = mask | (1 << i)
        reachable[next] = true
        remainder[next] = (remainder[mask] + nums[i]) % target

return reachable[(1 << n) - 1]
```

> Travelling Salesman uses the same shape with an extra dimension: `dp[mask][last]` = cheapest route visiting `mask` and ending at `last`, in `O(2^N * N^2)`.

## Graphs

### Cheapest Flights Within K Stops

Given n cities and flights `[from, to, price]`, return the cheapest price from src to dst using at most k stops. If there is no such route, return -1.

**Example:** `n = 4, flights = [[0,1,100],[1,2,100],[2,0,100],[1,3,600],[2,3,200]], src = 0, dst = 3, k = 1` → `700`

```text
PLAIN DIJKSTRA | INCORRECT

Dijkstra minimises cost only, so it can discard a costlier path that uses fewer stops
The state must include the number of stops used

------------------------------------------------------------------------------

BELLMAN-FORD (K + 1 ROUNDS) | O(K * E) | O(V)

// Relaxing all edges i times finds the cheapest path using at most i edges
// The snapshot is required so that one round cannot use edges relaxed in the same round

distance = array filled with infinity
distance[src] = 0

repeat k + 1 times:
    previous = copy of distance
    for each (from, to, price) in flights:
        if previous[from] == infinity:
            continue
        distance[to] = min(distance[to], previous[from] + price)

if distance[dst] == infinity:
    return -1
return distance[dst]

------------------------------------------------------------------------------

BFS / DIJKSTRA ON (CITY, STOPS) | O(E * K log(E * K)) | O(V * K)

Push (cost, city, stopsUsed) into a min heap
Skip a state when stopsUsed > k or when the city was already reached with fewer stops and lower cost
```

### Critical Connections in a Network

Given an undirected connected graph, return all bridges: edges whose removal disconnects the graph.

**Example:** `n = 4, connections = [[0,1],[1,2],[2,0],[1,3]]` → `[[1,3]]`

```text
BRUTE FORCE | O(E * (V + E)) | O(V + E)

Remove each edge and check whether the graph is still connected

------------------------------------------------------------------------------

TARJAN'S BRIDGE ALGORITHM | O(V + E) | O(V + E)

// discovery[node] = when the node was first visited
// low[node] = earliest discovery time reachable from the node's subtree using at most one back edge
// If a child cannot reach the current node or higher, the connecting edge is a bridge

timer = 0

DFS(node, parent):
    discovery[node] = low[node] = timer++
    for neighbor in graph[node]:
        if neighbor == parent:
            continue                     // Skip only one occurrence for parallel edges
        if neighbor is visited:
            low[node] = min(low[node], discovery[neighbor])
        else:
            DFS(neighbor, node)
            low[node] = min(low[node], low[neighbor])
            if low[neighbor] > discovery[node]:
                add (node, neighbor) to bridges
```

> - Articulation point: `low[child] >= discovery[node]`; the DFS root is one only when it has two or more children.
> - Strongly connected components in a directed graph use Tarjan (one DFS with a stack) or Kosaraju (two DFS passes on the graph and its reverse).

### Reconstruct Itinerary

Given a list of airline tickets `[from, to]`, reconstruct the itinerary that starts at `"JFK"` and uses every ticket exactly once. If several are valid, return the lexicographically smallest one.

**Example:** `tickets = [["MUC","LHR"],["JFK","MUC"],["SFO","SJC"],["LHR","SFO"]]` → `["JFK","MUC","LHR","SFO","SJC"]`

```text
BACKTRACKING | O(E!) | O(E)

Try every unused ticket in lexicographic order and undo on failure

------------------------------------------------------------------------------

HIERHOLZER'S ALGORITHM (EULERIAN PATH) | O(E log E) | O(E)

// Greedily walking forward can get stuck at a dead end before all edges are used
// Post-order emission fixes this: a stuck node is appended first and ends up last

build adjacency lists and sort each one (or use a min heap per node)
route = empty list

DFS(node):
    while graph[node] is not empty:
        next = remove the smallest destination from graph[node]
        DFS(next)
    route.add(node)              // Post-order

DFS("JFK")
reverse route
```

> An Eulerian path exists when the graph is connected and at most one vertex has `outdegree - indegree == 1` (the start) and at most one has `indegree - outdegree == 1` (the end).

## Greedy

### Partition Labels

You are given a string s. You need to partition the string into as many parts as possible such that: Each character appears in at most one partition. After partitioning, return the sizes of all partitions.
Example - s = "ababcbacadefegdehijhklij" > "ababcbaca" | "defegde" | "hijhklij" > [9, 7, 8]

**Example:** `s = "ababcbacadefegdehijhklij"` → `[9, 7, 8]`

```text
BRUTE FORCE | O(N^2) | O(1)

Search for the last occurrence of each character in the string and create partitions accordingly.

-----------------------------------------------------------------------------

GREEDY | O(N) | O(1)

// Pre calculate the last occurrence of each character, then iterate through the string to create partitions based on the last occurrences. If all characters in the current partition have their last occurrence within the partition, we can finalize the partition.

function partitionLabels(s):
    last = array/map
    // Find last occurrence of every character
    n = length(s)
    for i from 0 to n - 1:
        last[s[i]] = i
    result = []
    start = 0
    end = 0
    for i from 0 to n - 1:
        end = max(end, last[s[i]])
        if i == end:
            result.add(i - start + 1)
            start = i + 1
    return result
```

## Backtracking

### Letter Combinations of a Phone Number

Given a string containing digits from 2-9 inclusive, return all possible letter combinations that the number could represent. Return the answer in any order. A mapping of digit to letters (just like on the telephone buttons) is given below. Note that 1 does not map to any letters.

**Example:** `digits = "23"` → `["ad","ae","af","bd","be","bf","cd","ce","cf"]`

```text
BACKTRACKING | O(4^N) | O(N)
Where N is the length of the input digits. Each digit can map to at most 4 letters, leading to a maximum of 4^N combinations.

function letterCombinations(digits):
    mapping = ["","","abc","def","ghi","jkl","mno","pqrs","tuv","wxyz"]
    if digits is empty:
        return []
    result = []
    current = ""
    backtrack(index):
        if index == length(digits):
            result.add(current)
            return
        letters = mapping[digits[index]]
        for letter in letters:
            current += letter
            backtrack(index + 1)
            current remove last character
    backtrack(0)
    return result
```

## Palindrome Partitioning

Given a string s, partition it such that every substring of the partition is a palindrome. Return all possible partitions.

**Example:** `s = "aab"` → `[["a","a","b"],["aa","b"]]`

```text
BACKTRACKING | O(N * 2^N) | O(N)

// Every position is either a cut point or not, so there are 2^(N-1) partitions

function partition(s):
    result = []
    current = []
    backtrack(start):
        if start == length(s):
            result.add(copy(current))
            return
        for end from start to n - 1:
            if s[start...end] is not a palindrome:
                continue
            // Choose
            current.add(s[start...end])
            // Explore
            backtrack(end + 1)
            // Undo
            current.removeLast()
    backtrack(0)
    return result

-----------------------------------------------------------------------------

BACKTRACKING + DP PRECOMPUTE | O(N * 2^N) | O(N^2)

// Precompute isPalindrome[i][j] so each check is O(1) instead of O(N)
// isPalindrome[i][j] = s[i] == s[j] AND (j - i < 2 OR isPalindrome[i + 1][j - 1])

for length = 1 to n:
    for i = 0 to n - length:
        j = i + length - 1
        isPalindrome[i][j] =
            s[i] == s[j]
            AND (length < 3 OR isPalindrome[i + 1][j - 1])
```

> Minimum-cuts variant is pure DP: `cuts[i] = min(cuts[j - 1] + 1)` for every `j` where `s[j..i]` is a palindrome.

## Heap

### Meeting Rooms III

You are given: n meeting rooms numbered 0 to n - 1 and a list of meetings [start, end]

Rules:

A meeting should use the unused room with the smallest room number.
If no room is available when a meeting starts, the meeting is delayed until a room becomes available.
When delayed, the meeting keeps the same duration.
Among rooms that become free at the same time, choose the smallest room number.
Return the room that hosted the most meetings.
If multiple rooms have the same count, return the smallest room number.

**Example:** `n = 2, meetings = [[0,10],[1,5],[2,7],[3,4]]` → `0`

```text
HEAP | O(N log N) | O(N)

// Maintain 2 heaps: one for available rooms (min-heap by room number) and one for occupied rooms (min-heap by end time and Room Number). Process meetings in order of start time, assigning rooms according to the rules.

sort meetings by start time
create minHeap availableRooms
create minHeap busyRooms

for room = 0 to n - 1
    push room into availableRooms

create count[n] initialized to 0

for each [start, end] in meetings
    duration = end - start

    while busyRooms is not empty
          and busyRooms.min.endTime <= start
        busy = remove minimum from busyRooms
        push busy.roomNumber
        into availableRooms
    if availableRooms is not empty
        room = remove minimum from availableRooms
        finishTime = end
    else
        busy = remove minimum from busyRooms
        room = busy.roomNumber
        finishTime =
            busy.endTime + duration
    count[room]++
    push (finishTime, room)
    into busyRooms
bestRoom = 0
for room = 1 to n - 1
    if count[room] > count[bestRoom]
        bestRoom = room

return bestRoom
```

## Linked List

### LFU Cache Design

**Concept:** Evict the least frequently used key. On frequency tie, evict LRU among them.

**Time:** `O(1)` average for `Get` and `Put` | **Space:** `O(capacity)`

**Data structures:**

- `Dictionary<int, (int val, int freq)>` — key → (value, frequency).
- `Dictionary<int, LinkedList<int>>` — freq → doubly linked list of keys (insertion order = LRU order within frequency).
- `int minFreq` — track current minimum frequency.

**Operations (all O(1)):**

- `Get(key)`: look up value, increment freq, move key from `freqMap[freq]` to `freqMap[freq+1]`. Update `minFreq` if `freqMap[minFreq]` is now empty.
- `Put(key, val)`: if exists, same as get + update val. If new: evict `freqMap[minFreq]`'s tail if at capacity; insert key into `freqMap[1]`; set `minFreq = 1`.

## Binary Search

## Median of Two Sorted Arrays

Given two sorted arrays nums1 and nums2 of size m and n respectively, return the median of the two sorted arrays.

**Example:** `nums1 = [1, 3], nums2 = [2]` → `2.0`

```text
BRUTE FORCE | O((m+n) log(m+n)) | O(m+n)
Merge the two arrays and sort them, then find the median of the merged array.

-----------------------------------------------------------------------------

TWO POINTERS | O(m+n) | O(1)

Use two pointers to merge the two arrays until reaching the median position, then return the median value.

total = m + n
target = total / 2
i = 0
j = 0
previous = 0
current = 0
for count from 0 to target:
    previous = current
    if i < m AND
       (j >= n OR nums1[i] <= nums2[j]):
        current = nums1[i]
        i++
    else:
        current = nums2[j]
        j++
if total is odd:
    return current
return (previous + current) / 2

-----------------------------------------------------------------------------
BINARY SEARCH | O(log(min(m,n))) | O(1)

// Use binary search on the smaller array to find the correct partition that divides the combined array into two halves with equal length (or off by one).
// Divide the arrays into left and right halves such that all elements in the left halves are less than or equal to all elements in the right halves. The median is then calculated based on the maximum of the left halves and the minimum of the right halves.

      LEFT        |       RIGHT

nums1: A A A A    |    B B B
nums2: C C C      |    D D D D

Need:
A <= D
C <= B

if length(nums1) > length(nums2):
    swap(nums1, nums2)
m = length(nums1)
n = length(nums2)
left = 0
right = m
leftSize = (m + n + 1) / 2
while left <= right:
    i = left + (right - left) / 2
    j = leftSize - i
    nums1Left  = value at i - 1, or -infinity
    nums1Right = value at i,     or +infinity
    nums2Left  = value at j - 1, or -infinity
    nums2Right = value at j,     or +infinity
    if nums1Left <= nums2Right
       AND nums2Left <= nums1Right:
        if total length is odd:
            return max(nums1Left, nums2Left)
        else:
            leftMax = max(nums1Left, nums2Left)
            rightMin = min(nums1Right, nums2Right)
            return (leftMax + rightMin) / 2
    else if nums1Left > nums2Right:
        right = i - 1
    else:
        left = i + 1
```

## Split Array Largest Sum

Given an array of non-negative integers nums and an integer m, split the array into m non-empty continuous subarrays. Minimize the largest sum among these m subarrays.

**Example:** `nums = [7, 2, 5, 10, 8], m = 2` → `18`

```text
BRUTE FORCE | O(N^m) | O(1)
Use recursion to explore all possible ways to split the array into m subarrays and calculate the largest sum for each split. Return the minimum of these largest sums.

-----------------------------------------------------------------------------

DP | O(N^2 * m) | O(N * m)

// dp[i][j] = minimum largest sum for splitting first i elements into j subarrays
// dp[i][j] = min over p < i max(dp[p][j-1],prefixSum[i] - prefixSum[p])

-----------------------------------------------------------------------------

BINARY SEARCH ON ANSWER | O(N log(sum(nums))) | O(1)

// Min = max(nums), Max = sum(nums)
// Check if we can split the array into at most m subarrays with largest sum <= mid

function splitArray(nums, k):
    left = max(nums)
    right = sum(nums)
    while left <= right:
        maxSum = left + (right - left) / 2
        groups = countGroups(nums, maxSum)
        if groups <= k:
            // maxSum works.
            // Try a smaller maximum.
            right = maxSum - 1
        else:
            // Need too many groups.
            // Maximum allowed sum is too small.
            left = maxSum + 1
    return left

function countGroups(nums, maxSum):
    groups = 1
    currentSum = 0
    for num in nums:
        if currentSum + num > maxSum:
            groups++
            currentSum = 0
        currentSum += num
    return groups
```

## Count of Smaller Numbers After Self

Given an integer array `nums`, return an array `counts` where `counts[i]` is the number of elements to the right of `nums[i]` that are smaller than it. The same technique counts inversions.

**Example:** `nums = [5, 2, 6, 1]` → `[2, 1, 1, 0]`

```text
BRUTE FORCE | O(N^2) | O(1)

For each element, scan everything to its right and count the smaller values

-----------------------------------------------------------------------------

MERGE SORT | O(N log N) | O(N)

// While merging, taking an element from the right half means it is smaller than
// every element still remaining in the left half

sort indices instead of values so each count can be attributed to its original position
rightWritten = number of right-half elements merged so far

when a left-half element is written:
    counts[itsIndex] += rightWritten
when a right-half element is written:
    rightWritten++

-----------------------------------------------------------------------------

FENWICK TREE (BINARY INDEXED TREE) | O(N log N) | O(N)

// Coordinate compress the values, scan right to left, and ask
// "how many values smaller than this one have I already seen?"

for i = n - 1 down to 0:
    counts[i] = QUERY(rank(nums[i]) - 1)
    UPDATE(rank(nums[i]), 1)

UPDATE(i, delta):
    while i <= n:
        tree[i] += delta
        i += i & (-i)          // Move to the next node covering i

QUERY(i):                      // Prefix sum over [1 ... i]
    sum = 0
    while i > 0:
        sum += tree[i]
        i -= i & (-i)          // Strip the lowest set bit
    return sum
```

> A Fenwick tree is 1-indexed; index 0 would loop forever because `0 & -0 == 0`.

## Tree

### Path Sum III

Given the root of a binary tree and an integer targetSum, return the number of downward paths (not necessarily starting at the root or ending at a leaf) whose values sum to targetSum.

**Example:** `root = [10,5,-3,3,2,null,11,3,-2,null,1], targetSum = 8` → `3`

```text
BRUTE FORCE | O(N^2) | O(H)

For every node, walk down all paths starting at that node and count matches

------------------------------------------------------------------------------

PREFIX SUM + HASH MAP | O(N) | O(H)

// Same idea as Subarray Sum Equals K, applied along the root-to-node path
// Remove the current prefix on the way back up so sibling branches are unaffected

PATH_SUM(root, targetSum):
    prefixCount = empty map
    prefixCount[0] = 1
    count = 0
    DFS(node, currentSum):
        if node is null:
            return
        currentSum += node.value
        count += prefixCount[currentSum - targetSum]
        prefixCount[currentSum]++
        DFS(node.left, currentSum)
        DFS(node.right, currentSum)
        prefixCount[currentSum]--     // Backtrack
    DFS(root, 0)
    return count
```

## Stack

## Asteroid Collision

You are given an array asteroids. Each asteroid has:

- Absolute value = size
- Sign = direction
- positive → moving right
- negative → moving left

All asteroids move at the same speed. When two asteroids collide:

- Smaller asteroid is destroyed.
- Larger asteroid survives.
- If both have the same size, both are destroyed.
- Asteroids moving in the same direction never collide.

Return the state of the asteroids after all collisions.

**Example:** `asteroids = [5, 10, -5]` → `[5, 10]`

```text
BRUTE FORCE | O(N^2) | O(1)
Repeatedly scan the array for collisions and resolve them until no more collisions occur

-----------------------------------------------------------------------------

STACK | O(N) | O(N)

// Use a stack to keep track of asteroids moving to the right. When a left-moving asteroid is encountered, resolve collisions with the stack.

create empty stack
for each asteroid x
    alive = true
    while alive
          and x < 0
          and stack is not empty
          and stack.top() > 0
        top = stack.top()
        if top < abs(x)
            stack.pop()
        else if top == abs(x)
            stack.pop()
            alive = false
        else
            alive = false
    if alive
        stack.push(x)
return stack
```

## Car Fleet

There are n cars traveling toward the same destination. You are given:
target — destination position
position[i] — starting position of car i
speed[i] — speed of car i

All cars:
Move in the same direction.
Start at different positions.
Cannot pass another car.
If a faster car catches a slower car, it slows down and becomes part of the same car fleet.

Return the number of car fleets that will arrive at the destination.

**Example:** `target = 12, position = [10, 8, 0, 5, 3], speed = [2, 4, 1, 1, 3]` → `3`

```text
BRUTE FORCE | O(N^3) | O(1)

For each car, simulate its movement and check for collisions with other cars to determine fleets. Complex and inefficient for large n.

-----------------------------------------------------------------------------

STACK | O(N log N) | O(N)

// Pair each car's position with its time to reach the target, sort by position from closest to farthest from target, and use a stack to determine fleets based on arrival times.

create list of cars
for each i: car = (position[i], speed[i])
sort cars by position descending
create empty stack of arrival times

for each car in sorted order
    time = (target - car.position) / car.speed

    if stack is empty or time > stack.top()
        stack.push(time)

return stack.count

-----------------------------------------------------------------------------
WITHOUT STACK | O(N log N) | O(1)

sort cars by position descending
fleetCount = 0
lastFleetTime = 0
for each car
    time = (target - position) / speed

    if fleetCount == 0 or time > lastFleetTime
        fleetCount++
        lastFleetTime = time

return fleetCount
```

## Calculator

Given a string representing a mathematical expression, evaluate it and return its integer result.
The expression can contain: digits, +, -, (, ), spaces. Example - `1 + (2 - (3 + 4))`

**Example:** `s = "1 + (2 - (3 + 4))"` → `-4`

```text
BRUTE FORCE | O(N^2) | O(1)
Repeatedly scan the string for parentheses and evaluate the innermost expressions first, replacing them with their results until no parentheses remain.

-----------------------------------------------------------------------------

STACK | O(N) | O(N)

// Use a stack to keep track of the current result and sign. When encountering '(', push the current result and sign onto the stack, and reset them. When encountering ')', pop from the stack and combine with the current result.

result = 0
sign = +1
create empty stack
i = 0

while i < length(expression)
    if expression[i] is space
        i++
    else if expression[i] is digit
        number = 0
        while i < length
              and expression[i] is digit
            number =  number × 10 + digit value
            i++
        result += sign × number
        continue
    else if expression[i] == '+'
        sign = +1
    else if expression[i] == '-'
        sign = -1
    else if expression[i] == '('
        stack.push(result)
        stack.push(sign)
        result = 0
        sign = +1
    else if expression[i] == ')'
        previousSign = stack.pop()
        previousResult = stack.pop()
        result = previousResult + previousSign × result
    i++
return result
```
