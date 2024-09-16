using System.Text;

namespace Common;

/// <summary>
/// Common utility functions.
/// </summary>
public static class Common
{
	/// <summary>
	/// Generates a random adjacency matrix for a graph.
	/// </summary>
	/// <param name="n">Number of vertices.</param>
	/// <param name="maxEdgesPerVertex">Maximum edges per vertex.</param>
	/// <returns>Returns a random adjacency matrix.</returns>
	public static double[,] GenerateRandomAdjacencyMatrix(int n, int maxEdgesPerVertex)
	{
		Random rand = new Random();
		double[,] adjacencyMatrix = new double[n, n];
		int[] edgesCount = new int[n];
		HashSet<int> oneEdgeNeighbors = new HashSet<int>();

		for (int i = 0; i < n; i++)
		{
			int edgesToAdd;

			if (oneEdgeNeighbors.Contains(i))
			{
				edgesToAdd = rand.Next(2, maxEdgesPerVertex + 1);
			}
			else
			{
				edgesToAdd = rand.Next(1, maxEdgesPerVertex + 1);
			}

			while (edgesCount[i] < edgesToAdd)
			{
				int neighbor;
				do
				{
					neighbor = rand.Next(n);
				} while (neighbor == i || adjacencyMatrix[i, neighbor] == 1 || edgesCount[neighbor] >= maxEdgesPerVertex);

				adjacencyMatrix[i, neighbor] = 1;
				adjacencyMatrix[neighbor, i] = 1;

				edgesCount[i]++;
				edgesCount[neighbor]++;

				if (edgesToAdd == 1)
				{
					oneEdgeNeighbors.Add(neighbor);
				}
			}
		}

		return adjacencyMatrix;
	}


	/// <summary>
	/// Generates a random weighted adjacency matrix for a graph.
	/// </summary>
	/// <param name="adjacencyMatrix">Adjacency matrix.</param>
	/// <param name="minWeight">Minimum weight.</param>
	/// <param name="maxWeight">Maximum weight.</param>
	/// <returns>Returns a random weighted adjacency matrix.</returns>
	public static double[,] GenerateRandomWeightedAdjacencyMatrix(double[,] adjacencyMatrix, double minWeight, double maxWeight)
	{
		int n = adjacencyMatrix.GetLength(0);
		Random rand = new Random();
		double[,] weightedAdjacencyMatrix = new double[n, n];

		for (int i = 0; i < n; i++)
		{
			for (int j = i + 1; j < n; j++)
			{
				if (adjacencyMatrix[i, j] == 1)
				{
					double weight = Math.Round(rand.NextDouble() * (maxWeight - minWeight) / 0.5) * 0.5 + minWeight;
					weightedAdjacencyMatrix[i, j] = weight;
					weightedAdjacencyMatrix[j, i] = weight;
				}
			}
		}

		return weightedAdjacencyMatrix;
	}

	/// <summary>
	/// Serializes a matrix to a string.
	/// </summary>
	/// <param name="matrix">Matrix to serialize.</param>
	/// <returns>Serialized matrix.</returns>
	public static string SerializeMatrix(double[,] matrix)
	{
		int rows = matrix.GetLength(0);
		int cols = matrix.GetLength(1);

		StringBuilder sb = new StringBuilder();
		sb.Append("{");
		for (int i = 0; i < rows; i++)
		{
			sb.Append("{");
			for (int j = 0; j < cols; j++)
			{
				sb.Append(matrix[i, j] + (j < cols - 1 ? "," : ""));
			}
			sb.Append("}" + (i < rows - 1 ? "," : ""));
			sb.Append("");
		}
		sb.Append("}");
		return sb.ToString();
	}

	/// <summary>
	/// Deserializes a matrix from a string.
	/// </summary>
	/// <param name="serializedMatrix">Serialized matrix.</param>
	/// <returns>Returns the deserialized matrix.</returns>
	public static double[,] DeserializeMatrix(string serializedMatrix)
	{
		var trimmedInput = serializedMatrix.Replace(" ", "").Replace("\n", "").Replace("\r", "").Trim('{', '}');

		var rows = trimmedInput.Split(new[] { "},{" }, StringSplitOptions.None);

		int rowCount = rows.Length;
		int colCount = rows[0].Split(',').Length;

		double[,] matrix = new double[rowCount, colCount];

		for (int i = 0; i < rowCount; i++)
		{
			var values = rows[i].Trim('{', '}').Split(',');
			for (int j = 0; j < colCount; j++)
			{
				matrix[i, j] = double.Parse(values[j]);
			}
		}

		return matrix;
	}

	/// <summary>
	/// Parses the output of the clingo solver and returns the JSON representation of the answer.
	/// </summary>
	/// <param name="output">Standard output of the clingo solver.</param>
	/// <returns>Returns the JSON representation of the answer.</returns>
	public static string GetClingoAnswerJSON(string output)
	{
		byte optimization = Convert.ToByte(string.Join("", output.Skip(output.LastIndexOf("Optimization : ") + 15).Take(1)));
		int indexOfLastAnswer = output.LastIndexOf("Answer: ") + 11;
		int indexOfOptimization = output.LastIndexOf($"Optimization: {optimization}");
		string mostOptimalAnswer = string.Join("", output.Substring(indexOfLastAnswer, indexOfOptimization - indexOfLastAnswer));
		
		StringBuilder sb = new StringBuilder();
		sb.Append("{");
		var parts = ParseParts(mostOptimalAnswer);
		foreach (var part in parts.DistinctBy(p => p.Id))
		{
			sb.Append("{");
			foreach (var p in parts.Where(p => p.Id == part.Id))
			{
				sb.Append($"{p.Vertex},");
			}
			if (sb.ToString().EndsWith(","))
			{
				sb.Remove(sb.Length - 1, 1);
			}
			sb.Append("},");
		}
		if (sb.ToString().EndsWith(","))
		{
			sb.Remove(sb.Length - 1, 1);
		}
		sb.Append("}");
		return sb.ToString();
	}

	public static string GetSerializedMatrix(List<List<int>> partitions)
	{
		StringBuilder sb = new();
		sb.Append("{");
		for (int i = 0; i < partitions.Count; i++)
		{
			sb.Append("{");
			sb.Append(string.Join(",", partitions[i]));
			sb.Append("}");
			if (i < partitions.Count - 1)
			{
				sb.Append(",");
			}
		}
		sb.Append("}");

		return sb.ToString();
	}

	/// <summary>
	/// Parses the most optimal answer from the clingo solver output.
	/// </summary>
	/// <param name="mostOptimalAnswer">The most optimal answer.</param>
	/// <returns>Returns the parsed parts.</returns>
	private static List<(int Id, int Vertex)> ParseParts(string mostOptimalAnswer)
	{
		var parts = new List<(int Id, int Vertex)>();
		string[] partsStr = mostOptimalAnswer.Split(" ");
		foreach (string part in partsStr)
		{
			int partId = Convert.ToInt32(string.Join("", part.Skip(part.IndexOf(",") + 1).Take(part.IndexOf(")") - part.IndexOf(",") - 1)));
			int vertex = Convert.ToInt32(string.Join("", part.Skip(part.IndexOf("(") + 1).Take(part.IndexOf(",") - part.IndexOf("(") - 1)));
			parts.Add(new()
			{
				Id = partId,
				Vertex = vertex
			});
		}

		return parts;
	}
}

/// <summary>
/// Contains extension methods for IEnumerable.
/// </summary>
public static class Extensions
{
	/// <summary>
	/// Computes the median of a sequence of doubles.
	/// </summary>
	/// <param name="source">The sequence of doubles.</param>
	/// <returns>The median of the sequence.</returns>
	public static double Median(this IEnumerable<double> source)
	{
		var sorted = source.OrderBy(x => x).ToArray();
		int count = sorted.Length;
		if (count % 2 == 0)
			return (sorted[count / 2 - 1] + sorted[count / 2]) / 2.0;
		else
			return sorted[count / 2];
	}
}
