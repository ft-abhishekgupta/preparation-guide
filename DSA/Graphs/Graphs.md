# Graphs

A graph is a non-linear data structure consisting of nodes (vertices) and the edges that connect them.

- $G = (V, E)$, where $V$ is the set of vertices and $E$ is the set of edges.

## Terminology

- **Vertex:** A node in the graph.
- **Edge:** A connection between two vertices.
- **Degree:** The number of edges connected to a vertex.
- **Weight:** A value associated with an edge, representing cost, distance, or capacity.
- **In-degree:** The number of edges entering a vertex in a directed graph.
- **Out-degree:** The number of edges leaving a vertex in a directed graph.
- **Path:** A sequence of edges connecting a sequence of vertices.
- **Cycle:** A path that starts and ends at the same vertex.

## Types of Graphs

![Examples of different graph types](image-6.png)

## Graph Representation

![Adjacency matrix and adjacency list representations](image-7.png)

| Property           | **Adjacency Matrix** | **Adjacency List**        |
| ------------------ | -------------------- | ------------------------- |
| **Structure**      | `V × V` array        | Neighbors for each vertex |
| **Check Edge**     | **O(1)**             | O(degree)                 |
| **Find Neighbors** | O(V)                 | **O(degree)**             |
| **Add Edge**       | O(1)                 | O(1)                      |
| **Space**          | O(V²)                | **O(V + E)**              |
| **Best For**       | Dense graphs         | Sparse graphs             |

## Graph Traversal

Traversal is the process of systematically visiting the vertices and edges of a graph.

| Property             | **Depth-First Search (DFS)**                                     | **Breadth-First Search (BFS)**                                                                 |
| -------------------- | ---------------------------------------------------------------- | ---------------------------------------------------------------------------------------------- |
| **Approach**         | Explore as far as possible along each branch before backtracking | Explore all neighbors at the present depth prior to moving on to nodes at the next depth level |
| **Data Structure**   | Stack (recursion or explicit)                                    | Queue                                                                                          |
| **Time Complexity**  | O(V + E)                                                         | O(V + E)                                                                                       |
| **Space Complexity** | O(V)                                                             | O(V)                                                                                           |
| **Use Cases**        | Topological sorting, cycle detection, pathfinding in mazes       | Shortest path in unweighted graphs, level order traversal                                      |

> BFS goes wide and DFS goes deep.

## Algorithm Selection Guide

```mermaid
flowchart TD
  G["Graph Problem"] --> T{"Goal?"}

  T -- "Traversal" --> TR["DFS or BFS"]
  T -- "Shortest path" --> P{"Single-source or all-pairs?"}
  T -- "Cycle / DAG" --> C{"Directed?"}

  P -- "All-pairs" --> F["Floyd-Warshall"]
  P -- "Single-source" --> W{"Edge weights?"}
  W -- "Unweighted" --> U["BFS"]
  W -- "0 or 1" --> Z["0-1 BFS"]
  W -- "Non-negative" --> D["Dijkstra"]
  W -- "Negative allowed" --> B["Bellman-Ford"]
  B --> N["Detects negative cycles"]

  C -- "Yes" --> CD["Topological sort or color DFS"]
  C -- "No" --> CU["Union-Find or DFS"]
```
