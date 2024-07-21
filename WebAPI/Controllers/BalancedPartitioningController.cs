using Microsoft.AspNetCore.Mvc;
using WebAPI.Models;
using System.Diagnostics;

namespace WebAPI.Controllers;

/// <summary>
/// The controller for the balanced partitioning algorithms
/// </summary>
[ApiController]
[Route("")]
public class BalancedPartitioningController : ControllerBase
{
	/// <summary>
	/// The stopwatch for measuring the execution time.
	/// </summary>
	private readonly Stopwatch _stopwatch;

	/// <summary>
	/// Initializes a new instance of the <see cref="BalancedPartitioningController"/> class.
	/// </summary>
    public BalancedPartitioningController()
    {
		_stopwatch = new Stopwatch();    
    }

	/// <summary>
	/// The endpoint for generating a random adjacency matrix.
	/// </summary>
	/// <param name="numVertices">The number of vertices.</param>
	/// <param name="maxEdgesPerVertex">The maximum number of edges per vertex.</param>
	/// <returns>Returns a random adjacency matrix.</returns>
    [HttpGet("randomAdjacencyGraph")]
	[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
	public IActionResult GenerateRandomAdjacencyMatrix(int numVertices, int maxEdgesPerVertex)
	{
		try
		{
			double[,] matrix = Common.Common.GenerateRandomAdjacencyMatrix(numVertices, maxEdgesPerVertex);
			string serializedMatrix = Common.Common.SerializeMatrix(matrix);
			return new JsonResult(serializedMatrix);
		}
		catch (Exception e)
		{
			return BadRequest(e.Message);
		}
	}

	/// <summary>
	/// The endpoint for generating a random weighted adjacency matrix.
	/// </summary>
	/// <param name="adjacencyMatrix">The adjacency matrix.</param>
	/// <param name="minWeight">The minimum weight.</param>
	/// <param name="maxWeight">The maximum weight.</param>
	/// <returns>Returns a random weighted adjacency matrix.</returns>
	[HttpPost("randomWeightedAdjacencyGraph")]
	public IActionResult GenerateRandomWeightedAdjacencyMatrix([FromBody] string adjacencyMatrix, int minWeight, int maxWeight)
	{
		try
		{
			double[,] deserializedMatrix = Common.Common.DeserializeMatrix(adjacencyMatrix);
			double[,] weightedMatrix = Common.Common.GenerateRandomWeightedAdjacencyMatrix(deserializedMatrix, minWeight, maxWeight);
			string serializedMatrix = Common.Common.SerializeMatrix(weightedMatrix);
			return new JsonResult(serializedMatrix);
		}
		catch (Exception e)
		{
			return BadRequest(e.Message);
		}
	}

	/// <summary>
	/// The endpoint for partitioning a graph by the spectral algorithm.
	/// </summary>
	/// <param name="adjacencyMatrix">The adjacency matrix.</param>
	/// <returns>Returns the partitioning of the graph.</returns>
	[HttpPost("spectralAlgorithm")]
	public IActionResult PartitionGraphBySpectralAlgorithm([FromBody] string adjacencyMatrix)
	{
		try
		{
			double[,] deserializedMatrix = Common.Common.DeserializeMatrix(adjacencyMatrix);
			_stopwatch.Start();
			string result = Algorithms.SpectralAlgorithm.Partition(deserializedMatrix);
			_stopwatch.Stop();
			return new JsonResult(new PartitioningOutput(result, _stopwatch.ElapsedMilliseconds));
		}
		catch (Exception e)
		{
			return BadRequest(e.Message);
		}
	}

	/// <summary>
	/// The endpoint for partitioning a graph by the logical programming algorithm.
	/// </summary>
	/// <param name="adjacencyMatrix">The adjacency matrix.</param>
	/// <param name="k">The number of partitions.</param>
	/// <returns>Returns the partitioning of the graph.</returns>
	[HttpPost("logicalProgramming")]
	public IActionResult PartitionGraphByLogicalProgramming([FromBody] string adjacencyMatrix, int k = 2)
	{
		try
		{
			double[,] deserializedMatrix = Common.Common.DeserializeMatrix(adjacencyMatrix);
			_stopwatch.Start();
			string result = Algorithms.LogicalProgramming.Partition(deserializedMatrix, k);
			_stopwatch.Stop();
			return new JsonResult(new PartitioningOutput(result, _stopwatch.ElapsedMilliseconds));
		}
		catch (Exception e)
		{
			return BadRequest(e.Message);
		}
	}

	/// <summary>
	/// The endpoint for partitioning a graph by the Kernighan-Lin algorithm.
	/// </summary>
	/// <param name="adjacencyMatrix">The adjacency matrix.</param>
	/// <param name="k">The number of partitions.</param>
	/// <returns>Returns the partitioning of the graph.</returns>
	[HttpPost("kernighanLin")]
	public IActionResult PartitionGraphByKernighanLin([FromBody] string adjacencyMatrix, int k = 2)
	{
		try
		{
			double[,] deserializedMatrix = Common.Common.DeserializeMatrix(adjacencyMatrix);
			_stopwatch.Start();
			string result = Algorithms.KernighanLinAlgorithm.Partition(deserializedMatrix, k);
			_stopwatch.Stop();
			return new JsonResult(new PartitioningOutput(result, _stopwatch.ElapsedMilliseconds));
		}
		catch (Exception e)
		{
			return BadRequest(e.Message);
		}
	}

	/// <summary>
	/// The endpoint for partitioning a graph by the greedy algorithm.
	/// </summary>
	/// <param name="adjacencyMatrix">The adjacency matrix.</param>
	/// <param name="k">The number of partitions.</param>
	/// <returns>Returns the partitioning of the graph.</returns>
	[HttpPost("greedyAlgorithm")]
	public IActionResult PartitionByGreedyAlgorithm([FromBody] string adjacencyMatrix, int k = 2)
	{
		try
		{
			double[,] deserializedMatrix = Common.Common.DeserializeMatrix(adjacencyMatrix);
			_stopwatch.Start();
			string result = Algorithms.GreedyAlgorithm.Partition(deserializedMatrix, k);
			_stopwatch.Stop();
			return new JsonResult(new PartitioningOutput(result, _stopwatch.ElapsedMilliseconds));
		}
		catch (Exception e)
		{
			return BadRequest(e.Message);
		}
	}

	/// <summary>
	/// The endpoint for partitioning a graph by the brute force algorithm.
	/// </summary>
	/// <param name="adjacencyMatrix">The adjacency matrix.</param>
	/// <param name="k">The number of partitions.</param>
	/// <returns>Returns the partitioning of the graph.</returns>
	[HttpPost("bruteForce")]
	public IActionResult PartitionGraphByBruteForce([FromBody] string adjacencyMatrix, int k = 2)
	{
		try
		{
			double[,] deserializedMatrix = Common.Common.DeserializeMatrix(adjacencyMatrix);
			_stopwatch.Start();
			string result = Algorithms.BruteForce.Partition(deserializedMatrix, k);
			_stopwatch.Stop();
			return new JsonResult(new PartitioningOutput(result, _stopwatch.ElapsedMilliseconds));
		}
		catch (Exception e)
		{
			return BadRequest(e.Message);
		}
	}

	/// <summary>
	/// The endpoint for partitioning a graph by the geometric algorithm.
	/// </summary>
	/// <param name="input">Object containing the adjacency matrix and the weights matrix.</param>
	/// <param name="k">The number of partitions.</param>
	/// <returns>Returns the partitioning of the graph.</returns>
	[HttpPost("geometricAlgorithm")]
	public IActionResult PartitionGraphByGeometricAlgorithm([FromBody] WeightedPartitioningInput input, int k = 2)
	{
		try
		{
			double[,] deserializedMatrix = Common.Common.DeserializeMatrix(input.AdjacencyMatrix);
			double[,] deserializedWeights = Common.Common.DeserializeMatrix(input.WeightsMatrix);
			_stopwatch.Start();
			string result = Algorithms.GeometricAlgorithm.Partition(deserializedMatrix, deserializedWeights, k);
			_stopwatch.Stop();
			return new JsonResult(new PartitioningOutput(result, _stopwatch.ElapsedMilliseconds));
		}
		catch (Exception e)
		{
			return BadRequest(e.Message);
		}
	}

	/// <summary>
	/// The endpoint for partitioning a graph by the inertial algorithm.
	/// </summary>
	/// <param name="input">The object containing the adjacency matrix and the weights matrix.</param>
	/// <param name="k">The number of partitions.</param>
	/// <returns>Returns the partitioning of the graph.</returns>
	[HttpPost("inertialAlgorithm")]
	public IActionResult PartitionGraphByInertialAlgorithm([FromBody] WeightedPartitioningInput input, int k = 2)
	{
		try
		{
			double[,] deserializedMatrix = Common.Common.DeserializeMatrix(input.AdjacencyMatrix);
			double[,] deserializedWeights = Common.Common.DeserializeMatrix(input.WeightsMatrix);
			_stopwatch.Start();
			string result = Algorithms.InertialAlgorithm.Partition(deserializedMatrix, deserializedWeights);
			_stopwatch.Stop();
			return new JsonResult(new PartitioningOutput(result, _stopwatch.ElapsedMilliseconds));
		}
		catch (Exception e)
		{
			return BadRequest(e.Message);
		}
	}

	/// <summary>
	/// The endpoint for partitioning a graph by the logical programming algorithm.
	/// </summary>
	/// <param name="input">The object containing the adjacency matrix and the weights matrix.</param>
	/// <param name="k">The number of partitions.</param>
	/// <returns>Returns the partitioning of the graph.</returns>
	[HttpPost("logicalProgrammingWeighted")]
	public IActionResult PartitionGraphByLogicalProgrammingWeighted([FromBody] WeightedPartitioningInput input, int k = 2)
	{
		try
		{
			double[,] deserializedMatrix = Common.Common.DeserializeMatrix(input.AdjacencyMatrix);
			double[,] deserializedWeights = Common.Common.DeserializeMatrix(input.WeightsMatrix);
			_stopwatch.Start();
			string result = Algorithms.LogicalProgramming.Partition(deserializedMatrix, k, deserializedWeights);
			_stopwatch.Stop();
			return new JsonResult(new PartitioningOutput(result, _stopwatch.ElapsedMilliseconds));
		}
		catch (Exception e)
		{
			return BadRequest(e.Message);
		}
	}

	/// <summary>
	/// The endpoint for partitioning a graph by the logical programming algorithm.
	/// </summary>
	/// <param name="input">The object containing the adjacency matrix and the weights matrix.</param>
	/// <param name="k">The number of partitions.</param>
	/// <returns>Returns the partitioning of the graph.</returns>
	[HttpPost("bruteForceWeighted")]
	public IActionResult PartitionGraphByBruteForceWeighted([FromBody] WeightedPartitioningInput input, int k = 2)
	{
		try
		{
			double[,] deserializedMatrix = Common.Common.DeserializeMatrix(input.AdjacencyMatrix);
			double[,] deserializedWeights = Common.Common.DeserializeMatrix(input.WeightsMatrix);
			_stopwatch.Start();
			string result = Algorithms.BruteForceWeighted.Partition(deserializedMatrix, deserializedWeights, k);
			_stopwatch.Stop();
			return new JsonResult(new PartitioningOutput(result, _stopwatch.ElapsedMilliseconds));
		}
		catch (Exception e)
		{
			return BadRequest(e.Message);
		}
	}
}
