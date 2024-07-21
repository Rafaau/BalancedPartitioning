namespace WebAPI.Models;

/// <summary>
/// The input for the weighted partitioning algorithm.
/// </summary>
public class WeightedPartitioningInput
{
	/// <summary>
	/// The adjacency matrix of the graph.
	/// </summary>
	public string AdjacencyMatrix { get; set; }

	/// <summary>
	/// The edge weight matrix of the graph.
	/// </summary>
	public string WeightsMatrix { get; set; }
}
