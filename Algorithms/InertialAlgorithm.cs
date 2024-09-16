using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Algorithms;

/// <summary>
/// Class with inertial algorithm implementation.
/// </summary>
public class InertialAlgorithm
{
	/// <summary>
	/// Partitions a graph into two subsets using a greedy algorithm.
	/// </summary>
	/// <param name="adjacencyMatrix">The adjacency matrix of the graph.</param>
	/// <param name="weightsMatrix">The weights matrix of the graph.</param>
	/// <returns>A serialized representation of the graph partitions.</returns>
	public static string Partition(double[,] adjacencyMatrix, double[,] weightsMatrix)
	{
		var laplacianMatrix = GenerateLaplacianMatrix(weightsMatrix);

		var eigen = laplacianMatrix.Evd();
		var eigenvalues = eigen.EigenValues.Real();
		var eigenvectors = eigen.EigenVectors;

		int fiedlerIndex = FindSecondSmallestEigenvalueIndex(eigenvalues);
		var fiedlerVector = eigenvectors.Column(fiedlerIndex);

		var partitions = PartitionVertices(fiedlerVector);

		return Common.Common.GetSerializedMatrix(partitions);
	}

	/// <summary>
	/// Generates the Laplacian matrix of the graph from the weights matrix.
	/// </summary>
	/// <param name="weightsMatrix">The weights matrix of the graph's edges.</param>
	/// <returns>The Laplacian matrix of the graph.</returns>
	private static Matrix<double> GenerateLaplacianMatrix(double[,] weightsMatrix)
	{
		int n = weightsMatrix.GetLength(0);
		var weights = DenseMatrix.OfArray(weightsMatrix);

		var degreeMatrix = DenseMatrix.CreateDiagonal(n, n, i => weights.Row(i).Sum());

		return degreeMatrix - weights;
	}

	/// <summary>
	/// Partitions vertices based on the Fiedler vector using the median value.
	/// </summary>
	/// <param name="fiedlerVector">The Fiedler vector obtained from the Laplacian matrix.</param>
	/// <returns>A tuple containing two arrays of vertex indices representing the partitions.</returns>
	private static List<List<int>> PartitionVertices(Vector<double> fiedlerVector)
	{
		int n = fiedlerVector.Count;
		List<int> partition1 = new();
		List<int> partition2 = new();

		var sortedValues = fiedlerVector.OrderBy(v => v).ToArray();
		double median = sortedValues[n / 2];

		for (int i = 0; i < n; i++)
		{
			double projection = fiedlerVector[i];

			if (projection >= median)
				partition1.Add(i);
			else
				partition2.Add(i);
		}

		return new List<List<int>> { partition1, partition2 };
	}

	/// <summary>
	/// Finds the index of the second smallest eigenvalue in the vector of eigenvalues.
	/// </summary>
	/// <param name="eigenvalues">The vector of eigenvalues of the Laplacian matrix.</param>
	/// <returns>The index of the second smallest eigenvalue.</returns>
	private static int FindSecondSmallestEigenvalueIndex(Vector<double> eigenvalues)
	{
		double min = double.MaxValue;
		double secondMin = double.MaxValue;
		int secondMinIndex = -1;

		for (int i = 0; i < eigenvalues.Count; i++)
		{
			if (eigenvalues[i] < min)
			{
				secondMin = min;
				min = eigenvalues[i];
			}
			else if (eigenvalues[i] < secondMin && eigenvalues[i] != min)
			{
				secondMin = eigenvalues[i];
				secondMinIndex = i;
			}
		}

		return secondMinIndex;
	}
}
