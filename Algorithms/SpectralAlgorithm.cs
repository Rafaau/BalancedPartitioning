using Google.OrTools.LinearSolver;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Algorithms;

/// <summary>
/// Class with spectral algorithm implementation.
/// </summary>
public static class SpectralAlgorithm
{
	/// <summary>
	/// Partitions a graph into two subsets using the spectral algorithm.
	/// </summary>
	/// <param name="adjacencyMatrix">The adjacency matrix of the graph.</param>
	public static string Partition(double[,] adjacencyMatrix)
	{
		var laplacianMatrix = GenerateLaplacianMatrix(adjacencyMatrix);

		var eigen = laplacianMatrix.Evd();
		var eigenvalues = eigen.EigenValues.Real();
		var eigenvectors = eigen.EigenVectors;

		int fiedlerIndex = FindSecondSmallestIndex(eigenvalues);
		var fiedlerVector = eigenvectors.Column(fiedlerIndex);

		var partitions = PartitionGraphORTools(fiedlerVector);

		return Common.Common.GetSerializedMatrix(partitions);
	}

	/// <summary>
	/// Generates the Laplacian matrix of the graph from the adjacency matrix.
	/// </summary>
	/// <param name="adjacencyMatrix">The adjacency matrix of the graph.</param>
	/// <returns>The Laplacian matrix of the graph.</returns>
	private static Matrix<double> GenerateLaplacianMatrix(double[,] adjacencyMatrix)
	{
		int n = adjacencyMatrix.GetLength(0);
		var adjacency = DenseMatrix.OfArray(adjacencyMatrix);
		var degreeMatrix = DenseMatrix.CreateDiagonal(n, n, i => adjacency.Row(i).Sum());

		return degreeMatrix - adjacency;
	}

	/// <summary>
	/// Finds the index of the second smallest element in a vector.
	/// </summary>
	/// <param name="eigenvalues">Vector of eigenvalues.</param>
	/// <returns>Returns the index of the second smallest element.</returns>
	private static int FindSecondSmallestIndex(Vector<double> eigenvalues)
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

	/// <summary>
	/// Partitions the graph using OR-Tools.
	/// </summary>
	/// <param name="fiedlerVector">Second smallest eigenvector.</param>
	/// <returns>Returns the partitioned graph<./returns>
	/// <exception cref="Exception">Thrown when no solution is found.</exception>
	private static List<List<int>> PartitionGraphORTools(Vector<double> fiedlerVector)
	{
		int n = fiedlerVector.Count;

		Solver solver = Solver.CreateSolver("SCIP");

		if (solver == null)
		{
			Console.WriteLine("SCIP solver unavailable.");
			throw new Exception("SCIP solver unavailable.");
		}

		Variable[] x = new Variable[n];
		for (int i = 0; i < n; i++)
		{
			x[i] = solver.MakeIntVar(-1, 1, $"x[{i}]");
		}

		Constraint sumConstraint = solver.MakeConstraint(0, 0, "sumConstraint");
		for (int i = 0; i < n; i++)
		{
			sumConstraint.SetCoefficient(x[i], 1);
		}

		Objective objective = solver.Objective();
		for (int i = 0; i < n; i++)
		{
			for (int j = 0; j < n; j++)
			{
				objective.SetCoefficient(x[i], fiedlerVector[i] * fiedlerVector[j]);
			}
		}
		objective.SetMinimization();

		Solver.ResultStatus resultStatus = solver.Solve();

		if (resultStatus == Solver.ResultStatus.OPTIMAL)
		{
			List<int> partition1 = new();
			List<int> partition2 = new();

			for (int i = 0; i < n; i++)
			{
				if (x[i].SolutionValue() == -1)
					partition1.Add(i);
				else
					partition2.Add(i);
			}

			return new List<List<int>> { partition1, partition2 };
		}
		else
		{
			throw new Exception("No solution found.");
		}
	}
}
