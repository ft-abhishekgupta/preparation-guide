Q1. Count Rotations With Exactly K Equal Adjacent Pairs
Solved
Easy
3 pt.
You are given a string s of length n and an integer k.

A cyclic rotation of s is obtained by choosing a prefix of s whose length is between 0 and n - 1 (inclusive), and moving it to the end of the string while preserving the order of all characters.

For every cyclic rotation of s, let its score be the number of indices i such that 0 <= i < n - 1 and the characters at positions i and i + 1 are equal.

Return the number of cyclic rotations of s whose score equals k.

Example 1:

Input: s = "aab", k = 1

Output: 2

Explanation:

The cyclic rotations of s are:

"aab": The characters at positions 0 and 1 are equal, so score = 1.
"aba": No two adjacent characters are equal, so score = 0.
"baa": The characters at positions 1 and 2 are equal, so score = 1.
Since score equals k for 2 cyclic rotations of s, the answer is 2.

Example 2:

Input: s = "abca", k = 0

Output: 1

Explanation:

The cyclic rotations of s are:

"abca": No two adjacent characters are equal, so score = 0.
"bcaa": The characters at positions 2 and 3 are equal, so score = 1.
"caab": The characters at positions 1 and 2 are equal, so score = 1.
"aabc": The characters at positions 0 and 1 are equal, so score = 1.
Since score equals k for only 1 cyclic rotation of s, the answer is 1.

Constraints:

2 <= n == s.length <= 100
s only consists of lowercase English letters.
0 <= k <= n - 1

```cs
class Solution
{
    public int CountRotations(string s, int k)
    {
        int n = s.Length;
        int equal = 0;

        // Count equal pairs in the circular string
        for (int i = 0; i < n; i++)
        {
            if (s[i] == s[(i + 1) % n])
                equal++;
        }

        int ans = 0;

        // Removed pair was equal
        if (k == equal - 1)
            ans += equal;

        // Removed pair was not equal
        if (k == equal)
            ans += n - equal;

        return ans;
    }
}
```

---

Q2. Count Good Cyclic Rotations
Solved
Medium
4 pt.
You are given an integer array nums of even length n.

A cyclic rotation of nums is obtained by choosing a prefix of nums whose length is between 0 and n - 1 (inclusive), and moving it to the end of the array while preserving the order of all elements.

A cyclic rotation is good if the sum of its first n / 2 elements is strictly greater than the sum of its last n / 2 elements.

Return the number of cyclic rotations of nums that are good.

Example 1:

Input: nums = [1,2,3,4,5,6]

Output: 3

Explanation:

The cyclic rotations of nums are:

Cyclic rotation Sum of first n / 2 elements Sum of last n / 2 elements
[1, 2, 3, 4, 5, 6] 1 + 2 + 3 = 6 4 + 5 + 6 = 15
[2, 3, 4, 5, 6, 1] 2 + 3 + 4 = 9 5 + 6 + 1 = 12
[3, 4, 5, 6, 1, 2] 3 + 4 + 5 = 12 6 + 1 + 2 = 9
[4, 5, 6, 1, 2, 3] 4 + 5 + 6 = 15 1 + 2 + 3 = 6
[5, 6, 1, 2, 3, 4] 5 + 6 + 1 = 12 2 + 3 + 4 = 9
[6, 1, 2, 3, 4, 5] 6 + 1 + 2 = 9 3 + 4 + 5 = 12
The first half has a greater sum than the second half for 3 rotations. Thus, the answer is 3.

Example 2:

Input: nums = [1,2,1,2]

Output: 0

Explanation:

The cyclic rotations of nums are:

Cyclic rotation Sum of first n / 2 elements Sum of last n / 2 elements
[1, 2, 1, 2] 1 + 2 = 3 1 + 2 = 3
[2, 1, 2, 1] 2 + 1 = 3 2 + 1 = 3
[1, 2, 1, 2] 1 + 2 = 3 1 + 2 = 3
[2, 1, 2, 1] 2 + 1 = 3 2 + 1 = 3
No cyclic rotation is good because the two sums are equal for every rotation. Thus, the answer is 0.

Constraints:

2 <= n == nums.length <= 105
1 <= nums[i] <= 109
n is even.

```cs
public class Solution {
    public int CountGoodRotations(int[] nums) {
        var len = nums.Length;
        long sum = 0;
        long preSum = 0;
        int ans = 0;
        foreach(var n in nums) sum += n;
        for(int i = 0; i < len/2; i++) preSum += nums[i];
        for(int i = len/2; i < len+(len/2); i++){
            if(preSum > (sum/2)){
                ans++;
            }
            preSum += nums[i%len];
            preSum -= nums[(i-(len/2)+len)%len];
        }
        return ans;
    }
}
```

---

Q3. Count Robot Groups
Solved
Medium
5 pt.
You are given a strictly increasing integer array position, where position[i] is the initial position of the ith robot at time t = 0.

You are also given an integer array speed, where speed[i] is the constant speed of the ith robot in units per second, and an integer distance.

Time is continuous and measured in seconds. A robot or group with speed v moves v \* t units to the right over any interval of t seconds.

Whenever the distance between two robots or groups becomes at most distance, they merge into a single group.

If multiple robots or groups satisfy the merging condition at the same time, all merges happen simultaneously. In particular, every connected collection of robots or groups whose consecutive positions differ by at most distance merges into one group.

After a merge, the resulting group takes the current position and speed of the rightmost robot in that group. Once merged, robots never separate.

Return the number of groups remaining after all possible merges have occurred.

Example 1:

Input: position = [1,5,6,20], speed = [4,3,2,3], distance = 1

Output: 2

Explanation:

Initially, the groups are {R1}, {R2}, {R3}, and {R​​​​​​​4}.
At t = 0, the robots R2 and R3 at positions 5 and 6, respectively, merge because they are 1 unit apart. The resulting group moves with the position and speed of the rightmost robot R3. The groups are now {R1}, {R2, R3}, and {R​4}.
Later at t = 2, the robot R1 catches up to the group {R2, R3} and merges with it. The groups are now {R1, R2, R3} and {R​4}.
Thus, the answer is 2.

Example 2:

Input: position = [1,5,9], speed = [3,2,2], distance = 2

Output: 2

Explanation:

Initially, the groups are {R1}, {R2}, and {R3}.
At t = 2, the robot R1 catches up to the robot R2 and merges with it. The resulting group moves with the position and speed of the rightmost robot R2. The groups are now {R1, R2} and {R3}.
Thus, the answer is 2.

Example 3:

Input: position = [9], speed = [8], distance = 5

Output: 1

Explanation:

Initially, there is only one group. Therefore, the answer is 1.

Constraints:

1 <= position.length == speed.length <= 105
1 <= position[i], speed[i], distance <= 109
position is strictly increasing.

```cs
public class Solution {
    public int CountGroups(int[] pos, int[] sp, int dis) {
        var list = new List<(int p, int s)>();
        int len = pos.Length;
        for(int i = 0; i < len; i++){
            if(list.Count == 0)
                list.Add((pos[i],sp[i]));
            else {
                var last = list[list.Count - 1];
                if(pos[i] - last.p <= dis){
                    list.RemoveAt(list.Count - 1);
                }
                list.Add((pos[i],sp[i]));
            }
        }
        var l = list.Count;
        var nextSmaller = new int[l];
        Array.Fill(nextSmaller, -1);
        var st = new Stack<int>();
        for(int i = 0; i < l; i++){
            while(st.Count > 0 && list[st.Peek()].s > list[i].s){
                nextSmaller[st.Pop()] = i;
            }
            st.Push(i);
        }
        var ans = 0;
        for(int i = 0; i < l;){
            if(nextSmaller[i] == -1){
                ans++; i++;
            }
            else{
                i = nextSmaller[i];
            }
        }
        return ans;
    }
}
```

---

Q4. Minimum Cost Path With At Most K Turns
Hard
6 pt.
You are given a 2D integer array grid of size m x n, where grid[i][j] represents the cost of visiting cell (i, j), and an integer k.

You start at the top-left cell (0, 0) and want to reach the bottom-right cell (m - 1, n - 1).

From each cell, you may move one step in any of the four directions: up, down, left, or right.

The cost of a path is the sum of the values of all visited cells, including the starting and ending cells. If a cell is visited more than once, its value is included each time it is visited.

Return the minimum possible path cost to reach (m - 1, n - 1) using at most k turns. If no such path exists, return -1.

A turn occurs when the direction changes between two consecutive moves. For example, moving right and then down counts as one turn, while moving right and then right does not.

Example 1:

Input: grid = [[2,7,3],[1,4,5]], k = 1

Output: 12

Explanation:

An optimal path is (0, 0) → (1, 0) → (1, 1) → (1, 2). The moves are down, right, right.
The direction changes from down to right once, so the path uses exactly k = 1 turn.
The total path cost is 2 + 1 + 4 + 5 = 12.
Example 2:

Input: grid = [[4,1,9],[3,2,5],[4,8,6]], k = 2

Output: 20

Explanation:​​​​​​​

An optimal path is (0, 0) → (1, 0) → (1, 1) → (1, 2) → (2, 2). The moves are down, right, right, down.
The direction changes from down to right and from right to down, so the path uses exactly k = 2 turns.
The total path cost is 4 + 3 + 2 + 5 + 6 = 20.
Example 3:

Input: grid = [[1,9],[3,4]], k = 0

Output: -1

Explanation:

It is impossible to reach (1, 1) using k = 0 turns. Thus, the answer is -1.

Constraints:

1 <= m == grid.length <= 75
1 <= n == grid[i].length <= 75
0 <= grid[i][j] <= 1000
0 <= k < min(m, n)

```csharp
using System;
using System.Collections.Generic;

public class Solution {
    public int MinimumCost(int[][] grid, int k) {
        int m = grid.Length;
        int n = grid[0].Length;

        // Edge case: grid is 1x1
        if (m == 1 && n == 1) return grid[0][0];

        // Direction vectors: Right, Down, Left, Up
        int[] dr = { 0, 1, 0, -1 };
        int[] dc = { 1, 0, -1, 0 };

        // 4D distance array: [row, col, direction, turns]
        int[,,,] dist = new int[m, n, 4, k + 1];

        // Initialize distances to infinity
        for (int i = 0; i < m; i++) {
            for (int j = 0; j < n; j++) {
                for (int d = 0; d < 4; d++) {
                    for (int t = 0; t <= k; t++) {
                        dist[i, j, d, t] = int.MaxValue;
                    }
                }
            }
        }

        // PriorityQueue stores tuples of (row, col, direction, turns)
        // and uses the path cost as the priority (min-heap)
        PriorityQueue<(int r, int c, int d, int t), int> pq = new();

        // Make the initial move from (0,0) in all valid directions
        for (int d = 0; d < 4; ++d) {
            int nr = dr[d];
            int nc = dc[d];

            if (nr >= 0 && nr < m && nc >= 0 && nc < n) {
                int initialCost = grid[0][0] + grid[nr][nc];
                dist[nr, nc, d, 0] = initialCost;
                pq.Enqueue((nr, nc, d, 0), initialCost);
            }
        }

        while (pq.Count > 0) {
            pq.TryDequeue(out var state, out int cost);
            var (r, c, d, t) = state;

            // Ignore outdated states (lazy deletion)
            if (cost > dist[r, c, d, t]) continue;

            // Reached destination
            if (r == m - 1 && c == n - 1) return cost;

            // Explore all 4 adjacent cells
            for (int nd = 0; nd < 4; ++nd) {
                int nt = t + (nd != d ? 1 : 0);

                // If it exceeds allowed turns, skip
                if (nt > k) continue;

                int nr = r + dr[nd];
                int nc = c + dc[nd];

                // If within grid bounds
                if (nr >= 0 && nr < m && nc >= 0 && nc < n) {
                    int ncost = cost + grid[nr][nc];

                    // Relaxation step
                    if (ncost < dist[nr, nc, nd, nt]) {
                        dist[nr, nc, nd, nt] = ncost;
                        pq.Enqueue((nr, nc, nd, nt), ncost);
                    }
                }
            }
        }

        // Target is unreachable within k turns
        return -1;
    }
}
```
