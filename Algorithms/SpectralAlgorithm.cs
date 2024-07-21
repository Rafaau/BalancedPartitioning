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
		Matrix<double> A = DenseMatrix.OfArray(adjacencyMatrix);
		Matrix<double> D = DenseMatrix.CreateDiagonal(A.RowCount, A.ColumnCount, i => A.Row(i).Sum());
		Matrix<double> Q = D - A;

		var evd = Q.Evd();
		Vector<double> eigenvalues = evd.EigenValues.Real();
		Matrix<double> eigenvectors = evd.EigenVectors;

		int secondSmallestIndex = FindSecondSmallestIndex(eigenvalues);
		Vector<double> x2 = eigenvectors.Column(secondSmallestIndex);

		return PartitionGraphORTools(x2);
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
	/// <param name="x2">Second smallest eigenvector.</param>
	/// <returns>Returns the partitioned graph<./returns>
	/// <exception cref="Exception">Thrown when no solution is found.</exception>
	private static string PartitionGraphORTools(Vector<double> x2)
	{
		int n = x2.Count;

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
				objective.SetCoefficient(x[i], x2[i] * x2[j]);
			}
		}
		objective.SetMinimization();

		Solver.ResultStatus resultStatus = solver.Solve();

		if (resultStatus == Solver.ResultStatus.OPTIMAL)
		{
			List<int> partitionA = new();
			List<int> partitionB = new();

			for (int i = 0; i < n; i++)
			{
				if (x[i].SolutionValue() == -1)
					partitionA.Add(i);
				else
					partitionB.Add(i);
			}

			return Common.Common.GetSerializedMatrix(new List<List<int>>() { partitionA, partitionB });
		}
		else
		{
			throw new Exception("No solution found.");
		}
	}
}
