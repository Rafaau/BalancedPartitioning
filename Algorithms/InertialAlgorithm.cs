using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Algorithms;

/// <summary>
/// Class with inertial algorithm implementation.
/// </summary>
public class InertialAlgorithm
{
	/// <summary>
	/// Partitions a graph into <paramref name="k"/> subsets using a greedy algorithm.
	/// </summary>
	/// <param name="adjacencyMatrix">The adjacency matrix of the graph.</param>
	/// <param name="weightsMatrix">The weights matrix of the graph.</param>
	/// <returns>A serialized representation of the graph partitions.</returns>
	public static string Partition(double[,] adjacencyMatrix, double[,] weightsMatrix)
	{
		List<List<int>> partitions = PerformInertialPartitioning(adjacencyMatrix, weightsMatrix);

		return Common.Common.GetSerializedMatrix(partitions);
	}

	/// <summary>
	/// Performs inertial partitioning on the graph.
	/// </summary>
	/// <param name="adjacencyMatrix">The adjacency matrix of the graph.</param>
	/// <param name="weightsMatrix">The weights matrix of the graph's edges.</param>
	/// <returns>Returns the partitions as lists of vertex indices.</returns>
	public static List<List<int>> PerformInertialPartitioning(double[,] adjacencyMatrix, double[,] weightsMatrix)
	{
		int n = adjacencyMatrix.GetLength(0);

		var adjacency = DenseMatrix.OfArray(adjacencyMatrix);
		var weights = DenseMatrix.OfArray(weightsMatrix);

		var laplacianMatrix = GenerateLaplacianMatrix(weights);

		var eigen = laplacianMatrix.Evd();
		var eigenvalues = eigen.EigenValues.Real();
		var eigenvectors = eigen.EigenVectors;

		int fiedlerIndex = FindTheSmallestEigenvalueIndex(eigenvalues);
		var fiedlerVector = eigenvectors.Column(fiedlerIndex);

		Console.WriteLine("Fiedler Vector:");
		for (int i = 0; i < fiedlerVector.Count; i++)
		{
			Console.WriteLine($"Vertex {i}: {fiedlerVector[i]}");
		}

		var partition = PartitionVertices(fiedlerVector);

		return new List<List<int>> { partition.Item1.ToList(), partition.Item2.ToList() };
	}

	/// <summary>
	/// Generates the Laplacian matrix of the graph from the weights matrix.
	/// </summary>
	/// <param name="weightsMatrix">The weights matrix of the graph's edges.</param>
	/// <returns>The Laplacian matrix of the graph.</returns>
	private static Matrix<double> GenerateLaplacianMatrix(Matrix<double> weightsMatrix)
	{
		int n = weightsMatrix.RowCount;

		var degreeMatrix = DenseMatrix.CreateDiagonal(n, n, i => weightsMatrix.Row(i).Sum());

		return degreeMatrix - weightsMatrix;
	}

	/// <summary>
	/// Partitions vertices based on the Fiedler vector using the median value.
	/// </summary>
	/// <param name="fiedlerVector">The Fiedler vector obtained from the Laplacian matrix.</param>
	/// <returns>A tuple containing two arrays of vertex indices representing the partitions.</returns>
	private static Tuple<int[], int[]> PartitionVertices(Vector<double> fiedlerVector)
	{
		int n = fiedlerVector.Count;
		var partition1 = new List<int>();
		var partition2 = new List<int>();

		var sortedValues = fiedlerVector.ToArray().OrderBy(v => v).ToArray();
		double median = sortedValues[n / 2];

		for (int i = 0; i < n; i++)
		{
			double projection = fiedlerVector[i];
			Console.WriteLine($"Vertex {i} Projection: {projection}");  // Debugging line

			if (projection >= median)
			{
				partition1.Add(i);
			}
			else
			{
				partition2.Add(i);
			}
		}

		Console.WriteLine("Partition 1 count: " + partition1.Count);
		Console.WriteLine("Partition 2 count: " + partition2.Count);

		return Tuple.Create(partition1.ToArray(), partition2.ToArray());
	}

	/// <summary>
	/// Finds the index of the smallest eigenvalue in the vector of eigenvalues.
	/// </summary>
	/// <param name="eigenvalues">The vector of eigenvalues of the Laplacian matrix.</param>
	/// <returns>The index of the smallest eigenvalue.</returns>
	private static int FindTheSmallestEigenvalueIndex(Vector<double> eigenvalues)
	{
		double min = double.MaxValue;
		int index = -1;

		for (int i = 0; i < eigenvalues.Count; i++)
		{
			if (eigenvalues[i] < min)
			{
				min = eigenvalues[i];
				index = i;
			}
		}

		return index;
	}
}
