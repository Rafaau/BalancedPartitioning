namespace Algorithms;

/// <summary>
/// Class implementing the brute force algorithm for balanced partitioning.
/// </summary>
public class BruteForce
{
	/// <summary>
	/// Partitions a graph into <paramref name="k"/> subsets using a greedy algorithm.
	/// </summary>
	/// <param name="adjacencyMatrix">Adjacency matrix of the graph.</param>
	/// <param name="k">Amount of partitions.</param>
	/// <returns>Returns the partitioned graph.</returns>
	public static string Partition(double[,] adjacencyMatrix, int k = 2)
	{
		var bestPartitionMatrix = BruteForceBalancedPartition(adjacencyMatrix, k);

		return Common.Common.SerializeMatrix(bestPartitionMatrix);
	}

	/// <summary>
	/// Generates all possible partitions and returns the one with the minimum number of cut edges.
	/// </summary>
	/// <param name="adjacencyMatrix">The adjacency matrix of the graph.</param>
	/// <param name="k">The number of partitions.</param>
	/// <returns>Returns the partitioned graph.</returns>
	private static double[,] BruteForceBalancedPartition(double[,] adjacencyMatrix, int k)
	{
		int n = adjacencyMatrix.GetLength(0);
		List<int> vertices = new List<int>(Enumerable.Range(0, n));
		int minCutEdges = int.MaxValue;
		List<List<int>> bestPartition = null;

		foreach (var partition in GeneratePartitions(vertices, k))
		{
			int cutEdges = CalculateCutEdges(adjacencyMatrix, partition);
			if (cutEdges < minCutEdges)
			{
				minCutEdges = cutEdges;
				bestPartition = partition;
			}
		}

		int maxSubsetSize = bestPartition.Max(subset => subset.Count);
		double[,] partitionMatrix = new double[k, maxSubsetSize];
		for (int i = 0; i < k; i++)
		{
			for (int j = 0; j < bestPartition[i].Count; j++)
			{
				partitionMatrix[i, j] = bestPartition[i][j];
			}
			for (int j = bestPartition[i].Count; j < maxSubsetSize; j++)
			{
				partitionMatrix[i, j] = -1;
			}
		}

		return partitionMatrix;
	}

	/// <summary>
	/// Checks if a partition is balanced.
	/// </summary>
	/// <param name="partition">The partition to check.</param>
	/// <returns>True if the partition is balanced, false otherwise.</returns>
	private static bool IsBalancedPartition(List<List<int>> partition)
	{
		int maxSize = partition.Max(subset => subset.Count);
		int minSize = partition.Min(subset => subset.Count);
		return maxSize - minSize <= 1;
	}

	/// <summary>
	/// Calculates the number of cut edges in a partition.
	/// </summary>
	/// <param name="adjacencyMatrix">The adjacency matrix of the graph.</param>
	/// <param name="partition">The partition of the graph.</param>
	/// <returns>The number of cut edges.</returns>
	private static int CalculateCutEdges(double[,] adjacencyMatrix, List<List<int>> partition)
	{
		int cutEdges = 0;
		for (int i = 0; i < partition.Count; i++)
		{
			for (int j = i + 1; j < partition.Count; j++)
			{
				foreach (int u in partition[i])
				{
					foreach (int v in partition[j])
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
	/// Generates all possible partitions of a set of nodes into k subsets.
	/// </summary>
	/// <param name="vertices">Vertices of the graph.</param>
	/// <param name="k">Amount of partitions.</param>
	/// <returns>Eumerable of all possible partitions.</returns>
	private static IEnumerable<List<List<int>>> GeneratePartitions(List<int> vertices, int k)
	{
		List<List<int>> partitions = new();
		for (int i = 0; i < k; i++)
		{
			partitions.Add(new List<int>());
		}

		foreach (var p in Backtrack(vertices, partitions, k, 0))
		{
			yield return p;
		}
	}

	/// <summary>
	/// Recursive backtracking algorithm to generate all possible partitions of a set of nodes into k subsets.
	/// </summary>
	/// <param name="vertices">The set of vertices.</param>
	/// <param name="partition">The current partition.</param>
	/// <param name="k">The amount of partitions.</param>
	/// <param name="index">The current index.</param>
	/// <returns>Returns all possible partitions.</returns>
	private static IEnumerable<List<List<int>>> Backtrack(List<int> vertices, List<List<int>> partition, int k, int index)
	{
		if (index == vertices.Count)
		{
			if (IsBalancedPartition(partition))
			{
				yield return partition.Select(subset => subset.ToList()).ToList();
			}
			yield break;
		}

		for (int i = 0; i < k; i++)
		{
			partition[i].Add(vertices[index]);
			foreach (var p in Backtrack(vertices, partition, k, index + 1))
			{
				yield return p;
			}
			partition[i].RemoveAt(partition[i].Count - 1);
		}
	}
}

/// <summary>
/// Class implementing the brute force algorithm for balanced partitioning with edge weights.
/// </summary>
public class BruteForceWeighted
{
	/// <summary>
	/// Partitions a graph into <paramref name="k"/> subsets using a greedy algorithm.
	/// </summary>
	/// <param name="adjacencyMatrix">The adjacency matrix of the graph.</param>
	/// <param name="edgeWeightMatrix">The edge weight matrix of the graph.</param>
	/// <param name="k">The amount of partitions.</param>
	/// <returns>Serialized matrix of the best partition.</returns>
	public static string Partition(double[,] adjacencyMatrix, double[,] edgeWeightMatrix, int k)
	{
		var bestPartitionMatrix = BruteForceBalancedPartition(adjacencyMatrix, edgeWeightMatrix, k);

		return Common.Common.SerializeMatrix(bestPartitionMatrix);
	}

	/// <summary>
	/// Checks if a partition is balanced.
	/// </summary>
	/// <param name="partition">The partition to check.</param>
	/// <param name="k">The amount of partitions.</param>
	/// <returns>Returns true if the partition is balanced, false otherwise.</returns>
	private static bool IsBalancedPartition(List<List<int>> partition, int k)
	{
		int maxSize = partition.Max(subset => subset.Count);
		int minSize = partition.Min(subset => subset.Count);
		return maxSize - minSize <= 1;
	}

	/// <summary>
	/// Calculates the number of cut edges in a partition.
	/// </summary>
	/// <param name="edgeWeightMatrix">The edge weight matrix of the graph.</param>
	/// <param name="partition">The partition of the graph.</param>
	/// <returns>The total weight of the cut edges.</returns>
	private static double CalculateCutWeight(double[,] edgeWeightMatrix, List<List<int>> partition)
	{
		double cutWeight = 0;
		for (int i = 0; i < partition.Count; i++)
		{
			for (int j = i + 1; j < partition.Count; j++)
			{
				foreach (int u in partition[i])
				{
					foreach (int v in partition[j])
					{
						cutWeight += edgeWeightMatrix[u, v];
					}
				}
			}
		}
		return cutWeight;
	}

	/// <summary>
	/// Generates all possible partitions of a set of vertices into k subsets.
	/// </summary>
	/// <param name="vertices">The vertices of the graph.</param>
	/// <param name="k">The amount of partitions.</param>
	/// <returns>Returns all possible partitions.</returns>
	private static IEnumerable<List<List<int>>> GeneratePartitions(List<int> vertices, int k)
	{
		int n = vertices.Count;
		int[] indices = new int[n];
		for (int i = 0; i < Math.Pow(k, n); i++)
		{
			List<List<int>> partition = new();
			for (int j = 0; j < k; j++)
			{
				partition.Add(new List<int>());
			}
			for (int j = 0; j < n; j++)
			{
				partition[indices[j]].Add(vertices[j]);
			}
			if (IsBalancedPartition(partition, k))
			{
				yield return partition;
			}
			for (int j = 0; j < n; j++)
			{
				if (++indices[j] < k) break;
				indices[j] = 0;
			}
		}
	}

	/// <summary>
	/// Generates all possible partitions and returns the one with the minimum cut weight.
	/// </summary>
	/// <param name="adjacencyMatrix">The adjacency matrix of the graph.</param>
	/// <param name="edgeWeightMatrix">The edge weight matrix of the graph.</param>
	/// <param name="k">The amount of partitions.</param>
	/// <returns>Returns the best partition.</returns>
	private static double[,] BruteForceBalancedPartition(double[,] adjacencyMatrix, double[,] edgeWeightMatrix, int k)
	{
		int n = adjacencyMatrix.GetLength(0);
		List<int> vertices = new List<int>(Enumerable.Range(0, n));
		double minCutWeight = double.MaxValue;
		List<List<int>> bestPartition = null;

		foreach (var partition in GeneratePartitions(vertices, k))
		{
			double cutWeight = CalculateCutWeight(edgeWeightMatrix, partition);
			if (cutWeight < minCutWeight)
			{
				minCutWeight = cutWeight;
				bestPartition = partition;
			}
		}

		int maxSubsetSize = bestPartition.Max(subset => subset.Count);
		double[,] partitionMatrix = new double[k, maxSubsetSize];
		for (int i = 0; i < k; i++)
		{
			for (int j = 0; j < bestPartition[i].Count; j++)
			{
				partitionMatrix[i, j] = bestPartition[i][j];
			}
			for (int j = bestPartition[i].Count; j < maxSubsetSize; j++)
			{
				partitionMatrix[i, j] = -1;
			}
		}

		return partitionMatrix;
	}
}

