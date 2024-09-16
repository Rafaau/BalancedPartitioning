using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Common;

namespace Algorithms;

/// <summary>
/// Class with Geometric algorithm implementation.
/// </summary>
public class GeometricAlgorithm
{
	/// <summary>
	/// Partitions a graph into <paramref name="k"/> subsets using a greedy algorithm.
	/// </summary>
	/// <param name="adjacencyMatrix">The adjacency matrix of the graph.</param>
	/// <param name="weightsMatrix">The weights of the edges in the graph.</param>
	/// <param name="k">Amount of partitions.</param>
	/// <returns>Serialized sets of vertices in each partition.</returns>
	public static string Partition(double[,] adjacencyMatrix, double[,] weightsMatrix, int k)
	{
		List<double[]> points = GeneratePointsFromGraph(adjacencyMatrix, weightsMatrix);
		List<double[]> projectedPoints = ProjectPointsToSphere(points);
		double[] centerPoint = ApproximateCenterPoint(projectedPoints);
		List<double[]> shiftedPoints = ShiftPoints(projectedPoints, centerPoint);

		var partitions = RecursivePartition(shiftedPoints, k, adjacencyMatrix, weightsMatrix);

		return Common.Common.GetSerializedMatrix(partitions);
	}

	/// <summary>
	/// Generates a list of 2D points from the given graph using the Laplacian matrix and its eigenvectors.
	/// </summary>
	/// <param name="adjacencyMatrix">The adjacency matrix of the graph.</param>
	/// <param name="weights">The weights of the edges in the graph.</param>
	/// <returns>A list of 2D points representing the vertices of the graph.</returns>
	private static List<double[]> GeneratePointsFromGraph(double[,] adjacencyMatrix, double[,] weights)
	{
		int n = adjacencyMatrix.GetLength(0);
		var laplacianMatrix = GenerateLaplacianMatrix(weights);

		var evd = laplacianMatrix.Evd();
		var eigenValues = evd.EigenValues.Real().ToArray();
		var eigenVectors = evd.EigenVectors.ToArray();

		var sortedIndices = eigenValues
			.Select((value, index) => new { value, index })
			.OrderBy(x => x.value)
			.Select(x => x.index)
			.ToArray();

		var selectedIndices = sortedIndices.Skip(1).Take(2).ToArray();

		List<double[]> points = new List<double[]>();
		for (int i = 0; i < n; i++)
		{
			double[] point = new double[2];
			for (int j = 0; j < 2; j++)
			{
				point[j] = eigenVectors[i, selectedIndices[j]];
			}
			points.Add(point);
		}

		return points;
	}

	/// <summary>
	/// Projects the given points to the surface of a unit sphere in one higher dimension.
	/// </summary>
	/// <param name="points">A list of points to be projected.</param>
	/// <returns>A list of points projected onto the unit sphere.</returns>
	private static List<double[]> ProjectPointsToSphere(List<double[]> points)
	{
		int dimensions = points[0].Length;
		List<double[]> projectedPoints = new List<double[]>();

		foreach (var point in points)
		{
			double norm = Math.Sqrt(point.Sum(coord => coord * coord) + 1);
			double[] projectedPoint = new double[dimensions + 1];
			for (int i = 0; i < dimensions; i++)
			{
				projectedPoint[i] = point[i] / norm;
			}
			projectedPoint[dimensions] = 1 / norm;
			projectedPoints.Add(projectedPoint);
		}

		return projectedPoints;
	}

	/// <summary>
	/// Approximates the center point of the given set of points.
	/// </summary>
	/// <param name="points">A list of points from which to compute the approximate center.</param>
	/// <returns>The approximate center point as an array of doubles.</returns>
	private static double[] ApproximateCenterPoint(List<double[]> points)
	{
		int dimensions = points[0].Length;
		double[] center = new double[dimensions];
		foreach (var point in points)
		{
			for (int i = 0; i < dimensions; i++)
			{
				center[i] += point[i];
			}
		}
		for (int i = 0; i < dimensions; i++)
		{
			center[i] /= points.Count;
		}
		return center;
	}

	/// <summary>
	/// Shifts the given points so that the specified center point becomes the origin.
	/// </summary>
	/// <param name="points">A list of points to be shifted.</param>
	/// <param="center">The center point to shift the points around.</param>
	/// <returns>A list of shifted points.</returns>
	private static List<double[]> ShiftPoints(List<double[]> points, double[] center)
	{
		int dimensions = points[0].Length;
		List<double[]> shiftedPoints = new List<double[]>();

		foreach (var point in points)
		{
			double[] shiftedPoint = new double[dimensions];
			for (int i = 0; i < dimensions; i++)
			{
				shiftedPoint[i] = point[i] - center[i];
			}
			shiftedPoints.Add(shiftedPoint);
		}

		return shiftedPoints;
	}

	/// <summary>
	/// Recursively partitions the given set of points into k subsets using the adjacency and weights matrices.
	/// </summary>
	/// <param name="points">The points to be partitioned.</param>
	/// <param name="k">The number of subsets to partition into.</param>
	/// <param name="adjacencyMatrix">The adjacency matrix of the graph.</param>
	/// <param name="weightsMatrix">The weights matrix of the graph.</param>
	/// <returns>A list of lists, where each inner list contains the indices of the points in one of the k partitions.</returns>
	private static List<List<int>> RecursivePartition(List<double[]> points, int k, double[,] adjacencyMatrix, double[,] weightsMatrix)
	{
		if (k == 1)
			return new List<List<int>> { Enumerable.Range(0, points.Count).ToList() };

		double[] normalVector = ChooseRandomNormalVector(points[0].Length);
		(var group1, var group2) = PartitionPointsAlongCircle(points, normalVector);
		BalanceSubsets(group1, group2, points);

		List<List<int>> result = new();
		int k1 = k / 2;
		int k2 = k - k1;

		var group1Points = group1.Select(index => points[index]).ToList();
		var group2Points = group2.Select(index => points[index]).ToList();

		var partitions1 = RecursivePartition(group1Points, k1, adjacencyMatrix, weightsMatrix);
		var partitions2 = RecursivePartition(group2Points, k2, adjacencyMatrix, weightsMatrix);

		result.AddRange(partitions1.Select(partition => partition.Select(index => group1[index]).ToList()));
		result.AddRange(partitions2.Select(partition => partition.Select(index => group2[index]).ToList()));

		return result;
	}

	/// <summary>
	/// Chooses a random normal vector in the given number of dimensions.
	/// </summary>
	/// <param name="dimensions">The number of dimensions for the normal vector.</param>
	/// <returns>A random normal vector as an array of doubles.</returns>
	private static double[] ChooseRandomNormalVector(int dimensions)
	{
		double[] vector = new double[dimensions];
		Random rand = new Random();
		for (int i = 0; i < dimensions; i++)
		{
			vector[i] = rand.NextDouble() - 0.5;
		}
		double norm = Math.Sqrt(vector.Sum(v => v * v));
		for (int i = 0; i < dimensions; i++)
		{
			vector[i] /= norm;
		}
		return vector;
	}

	/// <summary>
	/// Partitions the given points along a circle defined by the normal vector.
	/// </summary>
	/// <param name="points">The points to be partitioned.</param>
	/// <param name="normalVector">The normal vector defining the circle.</param>
	/// <returns>A tuple containing two lists of indices, representing the two partitions.</returns>
	private static Tuple<List<int>, List<int>> PartitionPointsAlongCircle(List<double[]> points, double[] normalVector)
	{
		List<int> group1 = new();
		List<int> group2 = new();

		double median = points.Select(p => DotProduct(p, normalVector)).Median();

		for (int i = 0; i < points.Count; i++)
		{
			if (DotProduct(points[i], normalVector) < median)
				group1.Add(i);
			else
				group2.Add(i);
		}

		return Tuple.Create(group1, group2);
	}

	/// <summary>
	/// Balances the two subsets of points to ensure their sizes are as equal as possible.
	/// </summary>
	/// <param name="group1">The first subset of points.</param>
	/// <param name="group2">The second subset of points.</param>
	/// <param name="points">The original list of points.</param>
	private static void BalanceSubsets(List<int> group1, List<int> group2, List<double[]> points)
	{
		while (Math.Abs(group1.Count - group2.Count) > 1)
		{
			if (group1.Count > group2.Count)
			{
				int moveIndex = FindClosestToMedian(group1, points);
				group2.Add(group1[moveIndex]);
				group1.RemoveAt(moveIndex);
			}
			else
			{
				int moveIndex = FindClosestToMedian(group2, points);
				group1.Add(group2[moveIndex]);
				group2.RemoveAt(moveIndex);
			}
		}
	}

	/// <summary>
	/// Finds the index of the point in the given group that is closest to the median.
	/// </summary>
	/// <param name="group">The group of points to search.</param>
	/// <param name="points">The original list of points.</param>
	/// <returns>The index of the point closest to the median.</returns>
	private static int FindClosestToMedian(List<int> group, List<double[]> points)
	{
		double median = points.Select(p => p.Sum()).Median();
		double minDistance = double.MaxValue;
		int closestIndex = -1;

		foreach (var index in group)
		{
			double distance = Math.Abs(points[index].Sum() - median);
			if (distance < minDistance)
			{
				minDistance = distance;
				closestIndex = index;
			}
		}

		return closestIndex;
	}

	/// <summary>
	/// Computes the dot product of two vectors.
	/// </summary>
	/// <param name="vector1">The first vector.</param>
	/// <param name="vector2">The second vector.</param>
	/// <returns>The dot product of the two vectors.</returns>
	private static double DotProduct(double[] vector1, double[] vector2)
	{
		double dotProduct = 0;
		for (int i = 0; i < vector1.Length; i++)
		{
			dotProduct += vector1[i] * vector2[i];
		}
		return dotProduct;
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
}
