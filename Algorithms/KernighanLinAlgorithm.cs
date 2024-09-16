namespace Algorithms;

/// <summary>
/// Class with the implementation of the Kernighan-Lin algorithm.
/// </summary>
public class KernighanLinAlgorithm
{
	/// <summary>
	/// Partitions a graph into <paramref name="k"/> subsets using the Kernighan-Lin algorithm.
	/// </summary>
	/// <param name="adjacencyMatrix">The adjacency matrix of the graph.</param>
	/// <param name="k">The number of partitions.</param>
	/// <returns>A serialized representation of the graph partitions.</returns>
	public static string Partition(double[,] adjacencyMatrix, int k)
	{
		int n = adjacencyMatrix.GetLength(0);

		List<List<int>> partitions = new List<List<int>>();
		for (int i = 0; i < k; i++)
		{
			partitions.Add(new List<int>());
		}

		List<int> vertices = Enumerable.Range(0, n).OrderBy(x => Guid.NewGuid()).ToList();
		for (int i = 0; i < n; i++)
		{
			partitions[i % k].Add(vertices[i]);
		}

		Tuple<int, int, int, int> bestGainPair;

		do
		{
			bestGainPair = GetBestGainPair(adjacencyMatrix, partitions);
			if (bestGainPair.Item1 != -1 && bestGainPair.Item2 != -1)
			{
				partitions[bestGainPair.Item3].Remove(bestGainPair.Item1);
				partitions[bestGainPair.Item3].Add(bestGainPair.Item2);
				partitions[bestGainPair.Item4].Remove(bestGainPair.Item2);
				partitions[bestGainPair.Item4].Add(bestGainPair.Item1);
			}
		} while (bestGainPair.Item1 != -1 && bestGainPair.Item2 != -1);

		return Common.Common.GetSerializedMatrix(partitions);
	}

	/// <summary>
	/// Finds the best pair of nodes to swap between partitions to minimize cut edges.
	/// </summary>
	/// <param name="adjacencyMatrix">The adjacency matrix of the graph.</param>
	/// <param name="partitions">The current partitions of the graph.</param>
	/// <returns>A tuple with the nodes to swap and their partitions.</returns>
	private static Tuple<int, int, int, int> GetBestGainPair(double[,] adjacencyMatrix, List<List<int>> partitions)
	{
		Tuple<int, int, int, int> bestGainPair = new(-1, -1, -1, -1);
		int lowestCutEdges = CalculateTotalCutEdges(adjacencyMatrix, partitions);
		for (int i = 0; i < partitions.Count; i++)
		{
			for (int j = i + 1; j < partitions.Count; j++)
			{
				foreach (var u in partitions[i])
				{
					foreach (var v in partitions[j])
					{
						List<List<int>> partitionsCopy = ClonePartitions(partitions);

						partitionsCopy[i].Remove(u);
						partitionsCopy[i].Add(v);
						partitionsCopy[j].Remove(v);
						partitionsCopy[j].Add(u);

						int cutEdges = CalculateTotalCutEdges(adjacencyMatrix, partitionsCopy);
						if (cutEdges < lowestCutEdges)
						{
							lowestCutEdges = cutEdges;
							bestGainPair = new Tuple<int, int, int, int>(u, v, i, j);
						}
					}
				}
			}
		}

		return bestGainPair;
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
}