## Core Concepts

- **Trie (prefix tree)** — each node represents one character; root = empty; path from root = prefix.
- **Radix tree / compressed trie** — merge chains of single-child nodes into one edge labeled with a substring. Reduces node count; used in routing tables and `ConcurrentDictionary` internals.
- **Suffix trie** — all suffixes of a string; enables O(m) substring search after O(n²) build. Suffix array + LCP array is the practical version.
- **Aho-Corasick** — trie + failure links (like KMP on a trie); multi-pattern search in O(n + Σ|patterns| + matches).

---

## Trie Node Design and Operations (C#)

```csharp
class TrieNode
{
    public TrieNode?[] Children = new TrieNode[26];
    public bool IsEnd;
}

class Trie
{
    private readonly TrieNode _root = new();

    public void Insert(string word)
    {
        var cur = _root;
        foreach (var c in word)
        {
            int i = c - 'a';
            cur.Children[i] ??= new TrieNode();
            cur = cur.Children[i]!;
        }
        cur.IsEnd = true;
    }

    public bool Search(string word)
    {
        var node = Find(word);
        return node?.IsEnd == true;
    }

    public bool StartsWith(string prefix)
    {
        return Find(prefix) != null;
    }

    private TrieNode? Find(string s)
    {
        var cur = _root;
        foreach (var c in s)
        {
            cur = cur.Children[c - 'a'];
            if (cur == null) return null;
        }
        return cur;
    }
}
```

- Time: O(L) insert, search, prefix. Space: O(ALPHABET × nodes) — 26 per node worst case.
- For unicode/large alphabets, use `Dictionary<char, TrieNode>` instead of fixed array.

---

## KMP — Knuth-Morris-Pratt

**Goal:** Find all occurrences of pattern `p` in text `t` in O(n + m), no backtracking.

### LPS (Longest Proper Prefix which is also Suffix) Array

```
p = "ABABC"
lps[0]=0  (A)
lps[1]=0  (AB — no proper prefix=suffix)
lps[2]=1  (ABA — "A" is lps)
lps[3]=2  (ABAB — "AB" is lps)
lps[4]=0  (ABABC — no match)
```

```csharp
int[] BuildLPS(string p)
{
    int m = p.Length;
    var lps = new int[m];
    int len = 0, i = 1;
    while (i < m)
    {
        if (p[i] == p[len]) { lps[i++] = ++len; }
        else if (len > 0)   { len = lps[len - 1]; } // fall back, don't advance i
        else                { lps[i++] = 0; }
    }
    return lps;
}

List<int> KMPSearch(string text, string pattern)
{
    var lps = BuildLPS(pattern);
    var result = new List<int>();
    int i = 0, j = 0;
    while (i < text.Length)
    {
        if (text[i] == pattern[j]) { i++; j++; }
        if (j == pattern.Length)   { result.Add(i - j); j = lps[j - 1]; }
        else if (i < text.Length && text[i] != pattern[j])
            j = j > 0 ? lps[j - 1] : 0; // fall back or advance i
        if (j == 0 && (i >= text.Length || text[i] != pattern[0])) i++;
    }
    return result;
}
```

**LPS construction:** O(m). **Search:** O(n). Total: O(n + m). No extra space beyond lps array.

---

## Z-Algorithm

`Z[i]` = length of longest substring starting at `i` that is also a prefix of the string.

```csharp
int[] ZArray(string s)
{
    int n = s.Length;
    var z = new int[n];
    int l = 0, r = 0;
    for (int i = 1; i < n; i++)
    {
        if (i < r) z[i] = Math.Min(r - i, z[i - l]);
        while (i + z[i] < n && s[z[i]] == s[i + z[i]]) z[i]++;
        if (i + z[i] > r) { l = i; r = i + z[i]; }
    }
    return z;
}
// Pattern search: concat pattern + '$' + text; any Z[i] == pattern.Length is a match.
```

O(n) time. Useful for: "find pattern in text", "count distinct substrings".

---

## Rabin-Karp Rolling Hash

```csharp
bool RabinKarp(string text, string pattern)
{
    const int Base = 31, Mod = 1_000_000_007;
    int m = pattern.Length, n = text.Length;
    if (m > n) return false;
    long ph = 0, th = 0, power = 1;
    for (int i = 0; i < m - 1; i++) power = power * Base % Mod;
    for (int i = 0; i < m; i++)
    {
        ph = (ph * Base + pattern[i]) % Mod;
        th = (th * Base + text[i]) % Mod;
    }
    for (int i = 0; i <= n - m; i++)
    {
        if (ph == th && text.Substring(i, m) == pattern) return true; // verify on hash match
        if (i < n - m)
            th = (th - text[i] * power % Mod + Mod) * Base % Mod + text[i + m];
        th %= Mod;
    }
    return false;
}
```

- O(n+m) average, O(nm) worst (hash collisions). Best for **multi-pattern** search or **duplicate substring** problems.

---

## Manacher's Algorithm (Conceptual)

Finds all palindromic substrings in O(n) by exploiting mirror symmetry. Transform string to `#a#b#a#` to handle even/odd uniformly. Maintains `center` and `right` boundary of the rightmost palindrome. For interviews: know it exists and its use case; implement only if specifically asked.

---

## String Matching Comparison

| Algorithm    | Preprocessing | Search         | Space        | Best for                           |
| ------------ | ------------- | -------------- | ------------ | ---------------------------------- |
| Naive        | O(1)          | O(nm)          | O(1)         | Short text/pattern                 |
| KMP          | O(m)          | O(n)           | O(m)         | Single pattern, no backtrack       |
| Z-algorithm  | O(n+m)        | O(n+m)         | O(n+m)       | Prefix-based queries               |
| Rabin-Karp   | O(m)          | O(n) avg       | O(1)         | Multi-pattern, rolling hash tricks |
| Aho-Corasick | O(Σ\|pi\|)    | O(n + matches) | O(Σ\|pi\|·α) | Many patterns simultaneously       |
| Boyer-Moore  | O(m+α)        | O(n/m) best    | O(m+α)       | Large alphabets, long patterns     |

---
