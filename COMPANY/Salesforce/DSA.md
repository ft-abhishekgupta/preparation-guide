# DSA

## 146. LRU Cache

<https://leetcode.com/problems/lru-cache/description/>

```
Use Doubly Linked List for Priority Tracking
Store link list node ref in dictionary for quick access
```

```cs
public class LRUCache {
    Dictionary<int, LinkedListNode<(int key, int val)>> map;
    LinkedList<(int key, int val)> ll;
    int cap;

    public LRUCache(int capacity) {
        cap = capacity;
        map = new();
        ll = new();
    }

    public int Get(int key) {
        if(!map.TryGetValue(key, out var val)){
            return -1;
        }
        ll.Remove(val);
        ll.AddFirst(val);
        return val.Value.val;
    }

    public void Put(int key, int value) {
        var node = new LinkedListNode<(int key, int val)>((key, value));
        if(!map.TryGetValue(key, out var val)){

            ll.AddFirst(node);
            map.Add(key, node);

            if(map.Count > cap){
                var toRemove = ll.Last;
                map.Remove(toRemove.Value.key);
                ll.RemoveLast();
            }
        }
        else {
            map[key] = node;
            ll.Remove(val);
            ll.AddFirst(node);
        }
    }
}
```

## 460. LFU Cache

<https://leetcode.com/problems/lfu-cache/description/>

```
use minF, map of freq to LinkedList of Nodes, map of key to LinkedListNode
```

```cs
using System.Collections.Generic;

public class LFUCache {
    public class Node {
        public int Key;
        public int Val;
        public int Freq;

        public Node(int key, int val) {
            Key = key;
            Val = val;
            Freq = 1;
        }
    }

    private readonly int cap;
    private int minFreq;
    private readonly Dictionary<int, LinkedListNode<Node>> nodeMap;
    private readonly Dictionary<int, LinkedList<Node>> freqMap;

    public LFUCache(int capacity) {
        cap = capacity;
        minFreq = 0;
        nodeMap = new();
        freqMap = new();
    }

    public int Get(int key) {
        if (!nodeMap.TryGetValue(key, out var node)) return -1;

        Promote(node);
        return node.Value.Val;
    }

    public void Put(int key, int value) {
        if (cap <= 0) return;

        if (nodeMap.TryGetValue(key, out var node)) {
            node.Value.Val = value;
            Promote(node);
            return;
        }

        // Evict LRU node from minFreq before adding the new key
        if (nodeMap.Count >= cap) Evict();

        var newNode = new Node(key, value);
        if (!freqMap.TryGetValue(1, out var list)) {
            list = new LinkedList<Node>();
            freqMap[1] = list;
        }

        nodeMap[key] = list.AddFirst(newNode);
        minFreq = 1;
    }

    private void Promote(LinkedListNode<Node> node) {
        int oldFreq = node.Value.Freq;
        var oldList = freqMap[oldFreq];
        oldList.Remove(node);

        if (oldFreq == minFreq && oldList.Count == 0) {
            minFreq++;
        }

        node.Value.Freq++;
        int nextFreq = node.Value.Freq;

        if (!freqMap.TryGetValue(nextFreq, out var nextList)) {
            nextList = new LinkedList<Node>();
            freqMap[nextFreq] = nextList;
        }

        // Reuse the same LinkedListNode wrapper to keep nodeMap in sync with O(1)
        nextList.AddFirst(node);
    }

    private void Evict() {
        var minList = freqMap[minFreq];
        var lru = minList.Last!;
        minList.RemoveLast();
        nodeMap.Remove(lru.Value.Key);
    }
}
```

## 56. Merge Intervals

<https://leetcode.com/problems/merge-intervals/description/>

```
Sort by start time
for each (s,e) in list
    if ans empty or ans end does not overlap with current
        add
    else
        update ans end to max of (ans.end, e)
```

```cs
public class Solution {
    public int[][] Merge(int[][] intervals) {
        var len = intervals.Length;
        var ans = new List<int[]>();
        var list = new List<(int s, int e)>();
        foreach(var i in intervals) list.Add((i[0], i[1]));
        list.Sort((a, b) => {
            if(a.s != b.s) return a.s-b.s;
            else return a.e-b.e;
        });
        for(int i = 0; i < len; i++){
            if(ans.Count == 0 || ans[^1][1] < list[i].s)
                ans.Add([list[i].s, list[i].e]);
            else
                ans[^1][1] = Math.Max(ans[^1][1], list[i].e);
        }
        return ans.ToArray();
    }
}
```

## 994. Rotting Oranges

<https://leetcode.com/problems/rotting-oranges/>

```
Multi-source BFS
Add all the source in queue
Only run the level for the initial level size
Increment minutes each level
```

```cs
public class Solution {
    public int OrangesRotting(int[][] grid) {
        var fresh = 0;
        var min = 0;
        var row = grid.Length;
        var col = grid[0].Length;
        var queue = new Queue<(int r, int c)>();
        for(int i = 0; i < row; i++){
            for(int j = 0; j < col; j++){
                if(grid[i][j] == 2) queue.Enqueue((i, j));
                if(grid[i][j] == 1) fresh++;
            }
        }
        while(queue.Count > 0 && fresh > 0){
            int size = queue.Count;
            for(int i = 0; i < size; i++){
                (var r,var c) = queue.Dequeue();
                if(r+1 < row && grid[r+1][c] == 1) {
                    grid[r+1][c] = 2; fresh--;
                    queue.Enqueue((r+1, c));
                }
                if(r-1 >= 0 && grid[r-1][c] == 1) {
                    grid[r-1][c] = 2; fresh--;
                    queue.Enqueue((r-1, c));
                }
                if(c+1 < col && grid[r][c+1] == 1) {
                    grid[r][c+1] = 2; fresh--;
                    queue.Enqueue((r, c+1));
                }
                if(c-1 >= 0 && grid[r][c-1] == 1) {
                    grid[r][c-1] = 2; fresh--;
                    queue.Enqueue((r, c-1));
                }
            }
            min++;
        }
        return fresh == 0 ? min : -1;
    }
}
```

## 23. Merge k Sorted Lists

<https://leetcode.com/problems/merge-k-sorted-lists/>

```
Use dummy node for ease
Put all start nodes in min heap
Pop, attach to ans and push next node
```

```cs
public class Solution {
    public ListNode MergeKLists(ListNode[] lists) {
        var pq = new PriorityQueue<ListNode, int>();
        var dummy = new ListNode(0);
        var curr = dummy;
        var len = lists.Length;
        for(int i = 0; i < len; i++){
            if(lists[i] != null)
                pq.Enqueue(lists[i],lists[i].val);
        }
        while(pq.Count > 0){
            var min = pq.Dequeue();

            curr.next = min;
            curr = curr.next;

            if(min.next != null)
                pq.Enqueue(min.next, min.next.val);
        }
        return dummy.next;
    }
}
```

## 560. Subarray Sum Equals K

<https://leetcode.com/problems/subarray-sum-equals-k/description/>

```
Count number of previous prefix sums that gets equal to [currentPrefixSum - target], add them to answer
Add map[0] = 1, for all the subarrays startint at 0
```

```cs
public class Solution {
    public int SubarraySum(int[] nums, int k) {
        var map = new Dictionary<int, int>();
        var curr = 0;
        var ans = 0;
        map.Add(0,1);
        foreach(var n in nums){
            curr += n;
            ans += map.GetValueOrDefault(curr - k);
            map[curr] = map.GetValueOrDefault(curr)+1;
        }
        return ans;
    }
}
```

## 128. Longest Consecutive Sequence

<https://leetcode.com/problems/longest-consecutive-sequence/description/>

```
Push all elements to set
Iterate and if (n-1) is not in set, then consider it the starting point and check max length achieved
```

```cs
public class Solution {
    public int LongestConsecutive(int[] nums) {
        var set = new HashSet<int>();
        var max = 0;
        foreach(var n in nums) set.Add(n);
        foreach(var n in nums){
            if(!set.Contains(n-1)){
                var index = n;
                var len = 0;
                while(set.Contains(index++)) len++;
                max = Math.Max(max, len);
            }
        }
        return max;
    }
}
```

## 210. Course Schedule II

<https://leetcode.com/problems/course-schedule-ii/description/>

```
TOPO SORT
Calculate indegree for all nodes
Add 0 indegree nodes to q
While q empty
    add q.pop to ans
    decrement indegree for all adj[q]
    if indegree of adj become 0 then add to q
```

```cs
public class Solution {
    public int[] FindOrder(int num, int[][] pre) {
        var ind = new int[num];
        var adj = new List<int>[num];
        var ans = new List<int>();
        for(int i = 0; i < num; i++) adj[i] = new List<int>();
        foreach(var p in pre){
            ind[p[0]]++;
            adj[p[1]].Add(p[0]);
        }
        var q = new Queue<int>();
        for(int i = 0; i < num; i++)
            if(ind[i] == 0) q.Enqueue(i);
        while(q.Count > 0){
            var f = q.Dequeue();
            ans.Add(f);
            foreach(var v in adj[f]){
                if(ind[v] > 0) ind[v]--;
                if(ind[v] == 0) q.Enqueue(v);
            }
        }
        return (ans.Count == num) ? ans.ToArray() : [];
    }
}
```

## 443. String Compression

<https://leetcode.com/problems/string-compression/description/>

```
Alternate approach
- current index, writer index
- Count with current index, update char with writer index
```

```cs
public class Solution {
    public int Compress(char[] chars) {
        var sb = new StringBuilder();
        int len = chars.Length;
        for(int i = 0; i < len; i++){
            var count = 1;
            var ch = chars[i];
            while(i+count < len && ch == chars[i+count]) count++;
            sb.Append(ch.ToString());
            if(count > 1) sb.Append(count.ToString());
            i = i+count-1;
        }
        var s = sb.ToString();
        var l = s.Length;
        for(int i = 0; i < l; i++) chars[i] = s[i];
        return l;
    }
}
```

## 22. Generate Parentheses

<https://leetcode.com/problems/generate-parentheses/description/>

```
At any instance closing brackets cant be more than opening, when iterating left to right
```

```cs
public class Solution {
    List<string> ans = new();
    public IList<string> GenerateParenthesis(int n) {
        var s = new char[n*2];
        Generate(0, n*2, s, n, n);
        return ans;
    }
    public void Generate(int curr, int len, char[] s, int o, int c){
        if(curr == len){
            ans.Add(new string(s));
            return;
        }
        if(c > 0 && c > o){
            s[curr] = ')';
            Generate(curr+1, len, s, o, c-1);
        }
        if(o > 0){
            s[curr] = '(';
            Generate(curr+1, len, s, o-1, c);
        }
    }
}
```

## 1209. Remove All Adjacent Duplicates in String II

<https://leetcode.com/problems/remove-all-adjacent-duplicates-in-string-ii/description/>

```
Use stack to track char and frequency. Pop and merge if same char

BETTER
Process one char at a time

for char c in s
    if stack not empty and stack top is c
        var t = st.Pop()
        if t.count+1 < k
            push c, t.count+1
    else
        push c, 1
```

```cs
public class Solution {
    public string RemoveDuplicates(string s, int k) {
        var st = new Stack<(char c, int f)>();
        int len = s.Length;
        int i = 0;
        while(i < len){
            var curr = s[i];
            var count = 0;
            while(i < len && s[i] == curr){ count++; i++; }
            if(st.Count == 0 || st.Peek().c != curr){
                count = count%k;
                if(count > 0) st.Push((curr, count%k));
            }
            else{
                while(st.Count > 0 && st.Peek().c == curr){
                    var top = st.Peek();
                    st.Pop();
                    count = (count+top.f)%k;
                }
                if(count > 0) st.Push((curr, count));
            }
        }
        Console.WriteLine(st.Count());
        StringBuilder sb = new();
        while(st.Count() > 0){
            (char c, int n) = st.Pop();
            while(n-- > 0) sb.Append(c.ToString());
        }
        var ans = sb.ToString().ToCharArray();
        Array.Reverse(ans);
        return new string(ans);
    }
}
```

## 735. Asteroid Collision

<https://leetcode.com/problems/asteroid-collision/description/>

```
stack = empty

for asteroid in asteroids:

    while stack is not empty
          AND stack.top > 0
          AND asteroid < 0:

        if stack.top < abs(asteroid):
            pop stack
            continue

        else if stack.top == abs(asteroid):
            pop stack
            asteroid = 0
            break

        else:
            asteroid = 0
            break

    if asteroid != 0:
        push asteroid into stack

return stack
```

```cs
public class Solution {
    public int[] AsteroidCollision(int[] ast) {
        var st = new Stack<int>();
        var len = ast.Length;
        var i = 0;
        while(i < len){
            if(st.Count() == 0 || st.Peek() * ast[i] > 0 || (st.Peek() < 0 && ast[i] > 0)){
                st.Push(ast[i++]);
            }
            else {
                var curr = ast[i];
                while(i < len && st.Count() > 0 && st.Peek() > 0 && curr < 0) {
                    var top = st.Pop();
                    if(Math.Abs(top) > Math.Abs(curr)) curr = top;
                    else if(Math.Abs(top) < Math.Abs(curr)) curr = ast[i];
                    else curr = 0;
                }
                i++;
                if(curr != 0) st.Push(curr);
            }
        }
        var ans = new int[st.Count()];
        while(st.Count() > 0)
            ans[st.Count()-1] = st.Pop();
        return ans;
    }
}
```

## 628. Maximum Product of Three Numbers

<https://leetcode.com/problems/maximum-product-of-three-numbers/description/>

```
O(N) Solution
Find max 3 and min 2 numbers using linear scan

largest = -∞
secondLargest = -∞
thirdLargest = -∞

smallest = +∞
secondSmallest = +∞

for each num:

    if num >= largest:
        thirdLargest = secondLargest
        secondLargest = largest
        largest = num

    else if num >= secondLargest:
        thirdLargest = secondLargest
        secondLargest = num

    else if num >= thirdLargest:
        thirdLargest = num


    if num <= smallest:
        secondSmallest = smallest
        smallest = num

    else if num <= secondSmallest:
        secondSmallest = num


return max(
    largest * secondLargest * thirdLargest,
    largest * smallest * secondSmallest
)
```

```cs
public class Solution {
    public int MaximumProduct(int[] nums) {
        Array.Sort(nums);
        int max = nums[0]*nums[1]*nums[2];
        max = Math.Max(max, nums[0]*nums[1]*nums[^1]);
        max = Math.Max(max, nums[0]*nums[^2]*nums[^1]);
        max = Math.Max(max, nums[^3]*nums[^2]*nums[^1]);
        return max;
    }
}
```

## 987. Vertical Order Traversal of a Binary Tree

```
Traverse and store both row and col
Same node can occur in same row and col, so sort them by value
```

```cs
public class Solution
{
    public IList<IList<int>> VerticalTraversal(TreeNode root)
    {
        var nodes = new List<(int col, int row, int val)>();

        void DFS(TreeNode node, int row, int col)
        {
            if (node == null) return;

            nodes.Add((col, row, node.val));

            DFS(node.left, row + 1, col - 1);
            DFS(node.right, row + 1, col + 1);
        }

        DFS(root, 0, 0);

        // Sort: column → row → value
        nodes.Sort((a, b) =>
        {
            if (a.col != b.col)
                return a.col.CompareTo(b.col);

            if (a.row != b.row)
                return a.row.CompareTo(b.row);

            return a.val.CompareTo(b.val);
        });

        var ans = new List<IList<int>>();
        int prevCol = int.MinValue;

        foreach (var node in nodes)
        {
            if (node.col != prevCol)
            {
                ans.Add(new List<int>());
                prevCol = node.col;
            }

            ans[^1].Add(node.val);
        }

        return ans;
    }
}
```

## 904. Fruit Into Baskets

<https://leetcode.com/problems/fruit-into-baskets/description/>

```
Max window with at max 2 unique elements
```

```cs
public class Solution {
    public int TotalFruit(int[] fruits) {
        var len = fruits.Length;
        var left = 0;
        var right = 0;
        var max = 0;
        var map = new Dictionary<int, int>();
        while(right < len){
            map[fruits[right]] = map.GetValueOrDefault(fruits[right],0) + 1;
            while(map.Count > 2){
                map[fruits[left]]--;
                if(map[fruits[left]] == 0)
                    map.Remove(fruits[left]);
                left++;
            }
            max = Math.Max(max, right-left+1);
            right++;
        }
        return max;
    }
}
```

## 131. Palindrome Partitioning

<https://leetcode.com/problems/palindrome-partitioning/description/>

```
Backtrack, decide till now if palindrome, add to curr list
```

```cs
public class Solution {
    public IList<IList<string>> Partition(string s) {
        int len = s.Length;
        var dp = new bool[len][];
        for(int i = 0; i < len; i++) dp[i] = new bool[len];
        for(int i = len-1; i >= 0; i--){
            for(int j = i; j < len; j++){
                if(j-i == 0) dp[i][j] = true;
                else if(j-i == 1 && s[i]==s[j]) dp[i][j] = true;
                else if(s[i]==s[j]) dp[i][j] = dp[i+1][j-1];
            }
        }
        var ans = new List<IList<string>>();
        void Backtrack(int ssLen, List<string> ss, int currI){
            if(ssLen == len){
                ans.Add(new List<string>(ss));
                return;
            }
            if(currI == len) return;
            if(dp[ssLen][currI]){
                var newS = s.Substring(ssLen,currI-ssLen+1);
                ss.Add(newS);
                Backtrack(ssLen+newS.Length,ss,currI+1);
                ss.RemoveAt(ss.Count -1);
            }
            Backtrack(ssLen,ss,currI+1);
        }
        var temp = new List<string>();
        Backtrack(0, temp, 0);
        return ans;
    }
}
```

## 1344. Angle Between Hands of a Clock

<https://leetcode.com/problems/angle-between-hands-of-a-clock/description/>

```cs
public class Solution {
    public double AngleClock(int hour, int minutes) {
        double hourDegreePerHour = 360.0 / 12;
        double hourDegreePerMinute = hourDegreePerHour / 60;
        double minDegreePerMinute = 360.0 / 60;
        double currHourDegree = hourDegreePerHour*(hour%12) + hourDegreePerMinute*minutes;
        double currMinDegree = minDegreePerMinute*minutes;
        var ans = Math.Abs(currHourDegree - currMinDegree);
        return ans <= 180.0 ? ans : 360.0-ans;
    }
}
```

## 355. Design Twitter

<https://leetcode.com/problems/design-twitter/description/>

```

```

```cs
public class Twitter {
    Dictionary<int, LinkedList<(int id, int time)>> userTweetMap;
    Dictionary<int, HashSet<int>> userFollowingMap;
    int max;
    public Twitter() {
        userTweetMap = new();
        userFollowingMap = new();
        max = 0;
    }

    public void PostTweet(int userId, int tweetId) {
        if(!userTweetMap.ContainsKey(userId)){
            userTweetMap[userId] = new LinkedList<(int id, int time)>();
        }
        if(!userFollowingMap.ContainsKey(userId)){
            userFollowingMap[userId] = new HashSet<int>();
            userFollowingMap[userId].Add(userId);
        }
        userTweetMap[userId].AddLast((tweetId,max++));
        if(userTweetMap[userId].Count > 10)
            userTweetMap[userId].RemoveFirst();
    }

    public IList<int> GetNewsFeed(int userId) {
        var ans = new List<int>();
        if(!userFollowingMap.TryGetValue(userId, out var set))
            return ans;
        var pointers = new List<LinkedListNode<(int id, int time)>>();
        foreach(var s in set){
            if(userTweetMap.TryGetValue(s,out var ll)){
                var lastNode = ll.Last!;
                if(lastNode != null)
                    pointers.Add(lastNode);
            }
        }
        while(ans.Count < 10){
            var mx = int.MinValue;
            var mi = -1;
            LinkedListNode<(int id, int time)> maxNode = null;
            for(int i = 0; i < pointers.Count; i++){
                if(pointers[i] != null){
                    if(pointers[i].Value.time > mx){
                        mx = pointers[i].Value.time;
                        mi = i;
                        maxNode = pointers[i];
                    }
                }
            }
            if(maxNode == null) break;

            ans.Add(maxNode.Value.id);
            pointers[mi] = maxNode.Previous;
        }
        return ans;
    }

    public void Follow(int followerId, int followeeId) {
        if(followerId == followeeId) return;
        if(!userFollowingMap.ContainsKey(followerId)){
            userFollowingMap[followerId] = new HashSet<int>();
            userFollowingMap[followerId].Add(followerId);
        }
        userFollowingMap[followerId].Add(followeeId);
    }

    public void Unfollow(int followerId, int followeeId) {
        if(followerId == followeeId) return;
        if(!userFollowingMap.TryGetValue(followerId, out var set))
            return;
        if(set.Contains(followeeId))
            userFollowingMap[followerId].Remove(followeeId);
    }
}
```
