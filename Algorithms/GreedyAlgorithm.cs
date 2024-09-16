namespace Algorithms;

/// <summary>
/// Class with Greedy Algorithm implementation.
/// </summary>
public class GreedyAlgorithm
{
	/// <summary>
	/// Partitions a graph into <paramref name="k"/> subsets using a greedy algorithm.
	/// </summary>
	/// <param name="adjacencyMatrix">The adjacency matrix of the graph.</param>
	/// <param name="n">The number of vertices in the graph.</param>
	/// <param name="k">The number of partitions.</param>
	/// <returns>A list of partitions, each partition being a list of vertices.</returns>
	public static string Partition(double[,] adjacencyMatrix, int k)
	{
		int n = adjacencyMatrix.GetLength(0);
		List<List<int>> partitions = new();
		for (int i = 0; i < k; i++)
			partitions.Add(new());

		int baseSize = n / k;
		int extraVertices = n % k;
		var partitionSizes = new int[k];
		for (int i = 0; i < k; i++)
			partitionSizes[i] = baseSize + (i < extraVertices ? 1 : 0);

		int startVertex = FindPseudoperipheralVertex(adjacencyMatrix, n);
		List<int> pseudoperipheralVertices = new() { startVertex };
		partitions[0].Add(startVertex);
		var usedVertices = new HashSet<int> { startVertex };

		FillPartition(adjacencyMatrix, partitions[0], usedVertices, partitionSizes[0]);

		for (int i = 1; i < k; i++)
		{
			startVertex = GetFarthestVertex(adjacencyMatrix, pseudoperipheralVertices);
			pseudoperipheralVertices.Add(startVertex);
			if (startVertex == -1)
				continue;

			partitions[i].Add(startVertex);
			usedVertices.Add(startVertex);
			FillPartition(adjacencyMatrix, partitions[i], usedVertices, partitionSizes[i]);
		}

		ImprovePartitioning(adjacencyMatrix, partitions);

		return Common.Common.GetSerializedMatrix(partitions);
	}

	/// <summary>
	/// Finds a pseudoperipheral vertex of the graph, which is a vertex with the greatest distance from other vertices.
	/// </summary>
	/// <param name="adjacencyMatrix">The adjacency matrix of the graph.</param>
	/// <param name="n">The number of vertices (size of the graph).</param>
	/// <returns>The pseudoperipheral vertex.</returns>
	private static int FindPseudoperipheralVertex(double[,] adjacencyMatrix, int n)
	{
		int maxDist = -1;
		int pseudoperipheralVertex = 0;

		for (int i = 0; i < n; i++)
		{
			int dist = BFS(adjacencyMatrix, i, out _);
			if (dist > maxDist)
			{
				maxDist = dist;
				pseudoperipheralVertex = i;
			}
		}

		return pseudoperipheralVertex;
	}

	/// <summary>
	/// Performs a Breadth-First Search (BFS) starting from a vertex to find the maximum distance to any other vertex.
	/// </summary>
	/// <param name="adjacencyMatrix">The adjacency matrix of the graph.</param>
	/// <param name="startVertex">The starting vertex for BFS.</param>
	/// <param name="farthestVertex">The farthest vertex found from the starting vertex.</param>
	/// <returns>The maximum distance from the starting vertex to any other vertex.</returns>
	private static int BFS(double[,] adjacencyMatrix, int startVertex, out int farthestVertex)
	{
		int n = adjacencyMatrix.GetLength(0);
		var distances = new int[n];
		Array.Fill(distances, -1);

		var queue = new Queue<int>();
		queue.Enqueue(startVertex);
		distances[startVertex] = 0;

		int maxDist = 0;
		farthestVertex = startVertex;

		while (queue.Count > 0)
		{
			int vertex = queue.Dequeue();
			for (int neighbor = 0; neighbor < n; neighbor++)
			{
				if (adjacencyMatrix[vertex, neighbor] > 0 && distances[neighbor] == -1)
				{
					distances[neighbor] = distances[vertex] + 1;
					queue.Enqueue(neighbor);

					if (distances[neighbor] > maxDist)
					{
						maxDist = distances[neighbor];
						farthestVertex = neighbor;
					}
				}
			}
		}

		return maxDist;
	}

	/// <summary>
	/// Fills a partition with vertices up to the target size using a BFS approach to explore neighbors.
	/// </summary>
	/// <param name="adjacencyMatrix">The adjacency matrix of the graph.</param>
	/// <param name="partition">The partition to be filled.</param>
	/// <param name="usedVertices">A set of used vertices to avoid adding the same vertex multiple times.</param>
	/// <param name="targetSize">The target size of the partition.</param>
	private static void FillPartition(double[,] adjacencyMatrix, List<int> partition, HashSet<int> usedVertices, int targetSize)
	{
		var queue = new Queue<int>(partition);
		do
		{
			var vertex = queue.Dequeue();
			var adjacentVertices = GetAdjacentVertices(adjacencyMatrix, vertex);
			foreach (var neighbor in adjacentVertices)
			{
				if (!usedVertices.Contains(neighbor))
				{
					partition.Add(neighbor);
					usedVertices.Add(neighbor);
					queue.Enqueue(neighbor);
				}

				if (partition.Count >= targetSize)
					return;
			}

		} while (partition.Count < targetSize && queue.Count > 0);

		if (partition.Count < targetSize)
		{
			do
			{
				var boundaryVertex = FindBoundaryVertex(adjacencyMatrix, usedVertices);
				if (boundaryVertex == -1)
					break;

				partition.Add(boundaryVertex);
				usedVertices.Add(boundaryVertex);
			} while (partition.Count < targetSize);
		}
	}

	/// <summary>
	/// Finds a boundary vertex which is in the current partitions but has neighbors not in the partitions.
	/// </summary>
	/// <param name="adjacencyMatrix">The adjacency matrix of the graph.</param>
	/// <param name="usedVertices">A set of vertices that are already used in partitions.</param>
	/// <returns>A boundary vertex if found, otherwise -1.</returns>
	private static int FindBoundaryVertex(double[,] adjacencyMatrix, HashSet<int> usedVertices)
	{
		int n = adjacencyMatrix.GetLength(0);

		foreach (var vertex in usedVertices)
		{
			for (int neighbor = 0; neighbor < n; neighbor++)
			{
				if (adjacencyMatrix[vertex, neighbor] > 0 && !usedVertices.Contains(neighbor))
					return neighbor;
			}
		}

		return -1;
	}

	/// <summary>
	/// Improves the partitioning by moving vertices between partitions and minimizing the number of separating edges.
	/// </summary>
	/// <param name="adjacencyMatrix">The adjacency matrix of the graph.</param>
	/// <param name="partitions">The current list of partitions.</param>
	private static void ImprovePartitioning(double[,] adjacencyMatrix, List<List<int>> partitions)
	{
		bool improved;
		int iteration = 0;
		int maxIterations = 100;
		int currentCutEdges = CalculateTotalCutEdges(adjacencyMatrix, partitions);
		List<List<int>> originalPartitions = ClonePartitions(partitions);

		do
		{
			improved = false;

			for (int i = 0; i < originalPartitions.Count; i++)
			{
				for (int j = i + 1; j < originalPartitions.Count; j++)
				{
					foreach (var u in originalPartitions[i])
					{
						foreach (var v in originalPartitions[j])
						{
							List<List<int>> partitionsCopy = ClonePartitions(originalPartitions);

							partitionsCopy[i].Remove(u);
							partitionsCopy[i].Add(v);
							partitionsCopy[j].Remove(v);
							partitionsCopy[j].Add(u);

							int cutEdges = CalculateTotalCutEdges(adjacencyMatrix, partitionsCopy);

							if (cutEdges < currentCutEdges)
							{
								partitions[i].Remove(u);
								partitions[i].Add(v);
								partitions[j].Remove(v);
								partitions[j].Add(u);
								currentCutEdges = cutEdges;
								improved = true;
							}
						}
					}
				}
			}

			iteration++;
		} while (iteration < maxIterations && improved);
	}

	/// <summary>
	/// Calculates the total number of cut edges between all partitions.
	/// </summary>
	/// <param name="adjacencyMatrix">The adjacency matrix of the graph.</param>
	/// <param name="partitions">The current partitions of the graph.</param>
	/// <returns>The total number of edges between different partitions.</returns>
	private static int CalculateTotalCutEdges(double[,] adjacencyMatrix, List<List<int>> partitions)
	{
		int cutEdges = 0;
		for (int i = 0; i < partitions.Count; i++)
		{
			for (int j = i + 1; j < partitions.Count; j++)
			{
				foreach (var u in partitions[i])
				{
					foreach (var v in partitions[j])
					{
						if (adjacencyMatrix[u, v] != 0)
						{
							cutEdges++;
						}
					}
				}
			}
		}
		return cutEdges;
	}

	/// <summary>
	/// Returns a list of all vertices adjacent to a given vertex.
	/// </summary>
	/// <param name="adjacencyMatrix">The adjacency matrix of the graph.</param>
	/// <param name="vertex">The vertex for which adjacent vertices are to be found.</param>
	/// <returns>A list of adjacent vertices.</returns>
	private static List<int> GetAdjacentVertices(double[,] adjacencyMatrix, int vertex)
	{
		List<int> adjacentVertices = new();
		int n = adjacencyMatrix.GetLength(0);

		for (int neighbor = 0; neighbor < n; neighbor++)
		{
			if (adjacencyMatrix[vertex, neighbor] > 0)
			{
				adjacentVertices.Add(neighbor);
			}
		}

		return adjacentVertices;
	}

	/// <summary>
	/// Returns the farthest vertex from a list of given vertices using Breadth-First Search (BFS).
	/// </summary>
	/// <param name="adjacencyMatrix">The adjacency matrix of the graph.</param>
	/// <param name="startVertices">The list of starting vertices for finding the farthest vertex.</param>
	/// <returns>The farthest vertex from any of the startVertices.</returns>
	private static int GetFarthestVertex(double[,] adjacencyMatrix, List<int> startVertices)
	{
		int n = adjacencyMatrix.GetLength(0);
		var distances = new int[n];
		Array.Fill(distances, -1);

		var queue = new Queue<int>();
		foreach (var startVertex in startVertices)
		{
			queue.Enqueue(startVertex);
			distances[startVertex] = 0;
		}

		int farthestVertex = startVertices[0];
		int maxDistance = 0;

		while (queue.Count > 0)
		{
			int vertex = queue.Dequeue();

			for (int neighbor = 0; neighbor < n; neighbor++)
			{
				if (adjacencyMatrix[vertex, neighbor] > 0 && distances[neighbor] == -1)
				{
					distances[neighbor] = distances[vertex] + 1;
					queue.Enqueue(neighbor);

					if (distances[neighbor] > maxDistance)
					{
						maxDistance = distances[neighbor];
						farthestVertex = neighbor;
					}
				}
			}
		}

		return farthestVertex;
	}

	/// <summary>
	/// Creates a deep copy of the list of partitions.
	/// </summary>
	/// <param name="partitions">The list of partitions to clone.</param>
	/// <returns>A deep copy of the partitions list.</returns>
	private static List<List<int>> ClonePartitions(List<List<int>> partitions)
	{
		List<List<int>> copy = new List<List<int>>();
		foreach (var partition in partitions)
		{
			copy.Add(new List<int>(partition));
		}
		return copy;
	}
}
