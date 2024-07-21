namespace WebAPI.Models;

/// <summary>
/// The output of the partitioning algorithm.
/// </summary>
public class PartitioningOutput
{
    /// <summary>
    /// The adjacency matrix of the partitioned graph.
    /// </summary>
	public string PartitionMatrix { get; set; }

    /// <summary>
    /// The execution time of the algorithm.
    /// </summary>
	public long ExecutionTime { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="PartitioningOutput"/> class.
	/// </summary>
	/// <param name="partitionMatrix">The partition matrix.</param>
	/// <param name="executionTime">The execution time.</param>
	public PartitioningOutput(string partitionMatrix, long executionTime)
    {
        PartitionMatrix = partitionMatrix;
        ExecutionTime = executionTime;
    }
}
