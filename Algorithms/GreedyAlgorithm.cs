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
	/// <param name="k">The number of partitions.</param>
	/// <returns>A serialized representation of the graph partitions.</returns>
	public static string Partition(double[,] adjacencyMatrix, int k)
	{
		int n = adjacencyMatrix.GetLength(0);

		var partitions = GreedyPartition(adjacencyMatrix, n, k);

		return Common.Common.GetSerializedMatrix(partitions);
	}

	/// <summary>
	/// Performs a greedy partitioning of the graph.
	/// </summary>
	/// <param name="adjacencyMatrix">The adjacency matrix of the graph.</param>
	/// <param name="n">The number of vertices in the graph.</param>
	/// <param name="k">The number of partitions.</param>
	/// <returns>A list of partitions, each partition being a list of vertices.</returns>
	private static List<List<int>> GreedyPartition(double[,] adjacencyMatrix, int n, int k)
	{
		List<List<int>> partitions = new();
		for (int i = 0; i < k; i++)
			partitions.Add(new List<int>());

		int startVertex = FindPseudoperipheralVertex(adjacencyMatrix, n);
		partitions[0].Add(startVertex);
		var usedVertices = new HashSet<int> { startVertex };

		FillPartition(adjacencyMatrix, partitions[0], usedVertices, n / k);

		for (int i = 1; i < k; i++)
		{
			int boundaryVertex = FindBoundaryVertex(adjacencyMatrix, usedVertices);
			if (boundaryVertex == -1) continue;

			partitions[i].Add(boundaryVertex);
			usedVertices.Add(boundaryVertex);
			FillPartition(adjacencyMatrix, partitions[i], usedVertices, n / k);
		}

		ImprovePartitioning(adjacencyMatrix, partitions);

		return partitions;
	}

	/// <summary>
	/// Finds a pseudoperipheral vertex of the graph, which is a vertex with the greatest distance from other vertices.
	/// </summary>
	/// <param name="adjacencyMatrix">The adjacency matrix of the graph.</param>
	/// <param name="k">The number of vertices (size of the graph).</param>
	/// <returns>The pseudoperipheral vertex.</returns>
	private static int FindPseudoperipheralVertex(double[,] adjacencyMatrix, int k)
	{
		int maxDist = -1;
		int pseudoperipheralVertex = 0;

		for (int i = 0; i < k; i++)
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
		int numVertices = adjacencyMatrix.GetLength(0);
		var distances = new int[numVertices];
		Array.Fill(distances, -1);

		var queue = new Queue<int>();
		queue.Enqueue(startVertex);
		distances[startVertex] = 0;

		int maxDist = 0;
		farthestVertex = startVertex;

		while (queue.Count > 0)
		{
			int vertex = queue.Dequeue();
			for (int neighbor = 0; neighbor < numVertices; neighbor++)
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
		while (partition.Count < targetSize && queue.Count > 0)
		{
			int vertex = queue.Dequeue();
			for (int neighbor = 0; neighbor < adjacencyMatrix.GetLength(0); neighbor++)
			{
				if (adjacencyMatrix[vertex, neighbor] > 0 && !usedVertices.Contains(neighbor))
				{
					partition.Add(neighbor);
					usedVertices.Add(neighbor);
					queue.Enqueue(neighbor);

					if (partition.Count >= targetSize)
						break;
				}
			}
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
		foreach (var vertex in usedVertices)
		{
			for (int neighbor = 0; neighbor < adjacencyMatrix.GetLength(0); neighbor++)
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

		do
		{
			improved = false;
			foreach (var partition in partitions)
			{
				var candidates = partition.ToList();
				foreach (var vertex in candidates)
				{
					for (int neighbor = 0; neighbor < adjacencyMatrix.GetLength(0); neighbor++)
					{
						if (adjacencyMatrix[vertex, neighbor] > 0)
						{
							int currentPartition = GetPartitionIndex(partitions, vertex);
							int neighborPartition = GetPartitionIndex(partitions, neighbor);

							if (currentPartition != neighborPartition)
							{
								if (neighborPartition >= 0 && neighborPartition < partitions.Count)
								{
									MoveVertex(partitions, vertex, neighborPartition);
									int newEdgeCount = CountSeparatingEdges(adjacencyMatrix, partitions);

									if (newEdgeCount < CountSeparatingEdges(adjacencyMatrix, partitions))
									{
										improved = true;
									}
									else
									{
										MoveVertex(partitions, vertex, currentPartition);
									}
								}
							}
						}
					}
				}
			}

			iteration++;
		} while (improved && iteration < maxIterations);
	}

	/// <summary>
	/// Gets the index of the partition containing the specified vertex.
	/// </summary>
	/// <param name="partitions">The list of partitions.</param>
	/// <param name="vertex">The vertex whose partition index is to be found.</param>
	/// <returns>The index of the partition containing the vertex, or -1 if the vertex is not found in any partition.</returns>
	private static int GetPartitionIndex(List<List<int>> partitions, int vertex)
	{
		for (int i = 0; i < partitions.Count; i++)
		{
			if (partitions[i].Contains(vertex))
				return i;
		}

		return -1;
	}

	/// <summary>
	/// Moves a vertex from its current partition to a target partition, if the target partition index is valid.
	/// </summary>
	/// <param name="partitions">The list of partitions.</param>
	/// <param name="vertex">The vertex to be moved.</param>
	/// <param name="targetPartition">The index of the target partition.</param>
	private static void MoveVertex(List<List<int>> partitions, int vertex, int targetPartition)
	{
		if (targetPartition < 0 || targetPartition >= partitions.Count)
		{
			Console.WriteLine($"Wrong target partition index: {targetPartition}");
			return;
		}

		for (int i = 0; i < partitions.Count; i++)
		{
			if (partitions[i].Remove(vertex))
				break;
		}

		if (targetPartition >= 0 && targetPartition < partitions.Count)
		{
			partitions[targetPartition].Add(vertex);
		}
	}

	/// <summary>
	/// Counts the number of edges that separate different partitions in the graph.
	/// </summary>
	/// <param name="adjacencyMatrix">The adjacency matrix of the graph.</param>
	/// <param name="partitions">The list of partitions.</param>
	/// <returns>The number of separating edges.</returns>
	private static int CountSeparatingEdges(double[,] adjacencyMatrix, List<List<int>> partitions)
	{
		int count = 0;
		var vertexToPartition = new Dictionary<int, int>();
		for (int i = 0; i < partitions.Count; i++)
		{
			foreach (var vertex in partitions[i])
			{
				vertexToPartition[vertex] = i;
			}
		}

		for (int i = 0; i < adjacencyMatrix.GetLength(0); i++)
		{
			for (int j = 0; j < adjacencyMatrix.GetLength(1); j++)
			{
				if (adjacencyMatrix[i, j] > 0)
				{
					if (vertexToPartition.TryGetValue(i, out int partitionI) &&
						vertexToPartition.TryGetValue(j, out int partitionJ) &&
						partitionI != partitionJ)
					{
						count++;
					}
				}
			}
		}

		return count / 2;
	}
}
