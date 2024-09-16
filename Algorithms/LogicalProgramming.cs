using System.Diagnostics;

namespace Algorithms;

/// <summary>
/// Class responsible for generating ASP program file and parsing the output.
/// </summary>
public class LogicalProgramming
{
	/// <summary>
	/// Generates ASP program file and runs clingo solver to partition the graph.
	/// </summary>
	/// <param name="adjacencyMatrix">The adjacency matrix of the graph.</param>
	/// <param name="k">The amount of partitions.</param>
	/// <param name="weightsMatrix">The weights matrix of the graph.</param>
	/// <returns>Returns the partition matrix.</returns>
	public static string Partition(double[,] adjacencyMatrix, int k, double[,] weightsMatrix = null)
	{
		string filePath = "C:/Users/kelo1/Desktop/Magisterka/balanced_partitioning_dynamic.lp";
		GenerateFile(adjacencyMatrix, k, filePath, weightsMatrix);

		ProcessStartInfo startInfo = new ProcessStartInfo
		{
			FileName = "D:/conda/Library/bin/clingo.exe", 
			Arguments = $"{filePath}",
			RedirectStandardOutput = true,
			UseShellExecute = false,
			CreateNoWindow = true
		};

		using (Process process = Process.Start(startInfo))
		{
			using (StreamReader reader = process.StandardOutput)
			{
				string output = reader.ReadToEnd();
				return Common.Common.GetClingoAnswerJSON(output);
			}
		}
	}

	/// <summary>
	/// Generates ASP program file based on the given adjacency matrix.
	/// </summary>
	/// <param name="adjacencyMatrix">The adjacency matrix of the graph</param>
	/// <param name="weightsMatrix">The weights matrix of the graph</param>
	private static void GenerateFile(double[,] adjacencyMatrix, int k, string filePath, double[,] weightsMatrix = null)
	{
		int n = adjacencyMatrix.GetLength(0);
		string aspProgram;

		if (weightsMatrix != null)
		{
			List<(int, int, int)> edges = GenerateEdgesWithWeights(adjacencyMatrix, weightsMatrix);
			aspProgram = GenerateAspProgramWeighted(n, edges, k);
		}
		else
		{
			List<(int, int)> edges = GenerateEdges(adjacencyMatrix);
			aspProgram = GenerateAspProgram(n, edges, k);
		}

		File.WriteAllText(filePath, aspProgram);
	}

	/// <summary>
	/// Generates ASP program file content based on the given adjacency matrix.
	/// </summary>
	/// <param name="n">The number of graph vertices.</param>
	/// <param name="edges">The adjacency matrix of the graph.</param>
	/// <param name="k">The amount of partitions.</param>
	/// <returns>Returns ASP program file content.</returns>
	private static string GenerateAspProgram(int n, List<(int, int)> edges, int k)
	{
		var edgeStatements = string.Join("\n", edges.Select(edge => $"edge({edge.Item1}, {edge.Item2})."));

		var aspProgram =
$@"% Declarations of vertices
vertex(0..{n - 1}).

% Declarations of edges
{edgeStatements}

% The number of partitions
k({k}).

% Declaration of vertex and subset
1 {{ part(V, 1..K) }} :- vertex(V), k(K).

% Minimization of the number of edges between parts
cut_edge(X, Y) :- edge(X, Y), part(X, P1), part(Y, P2), P1 != P2.
#minimize {{ 1,X,Y : cut_edge(X, Y) }}.

% Predicates counting the number of vertices in subsets
part_size(P, S) :- S = #count {{ V : part(V, P) }}, k(K), P = 1..K.

% Limitation of the equality of the number of vertices in subsets
:- k(K), P1 = 1..K, P2 = 1..K, P1 < P2, part_size(P1, S1), part_size(P2, S2), S1 != S2.

% Printing the result
#show part/2.
";
		return aspProgram;
	}

	/// <summary>
	/// Generates ASP program file content based on the given adjacency matrix and weights matrix.
	/// </summary>
	/// <param name="n">The number of graph vertices.</param>
	/// <param name="edges">The number of graph edges.</param>
	/// <param name="k">The amount of partitions.</param>
	/// <returns>The ASP program file content.</returns>
	private static string GenerateAspProgramWeighted(int n, List<(int, int, int)> edges, int k)
	{
		var edgeStatements = string.Join("\n", edges.Select(edge => $"edge({edge.Item1}, {edge.Item2}, {edge.Item3})."));

		var aspProgram =
$@"% Declarations of vertices
vertex(0..{n - 1}).

% Declarations of edges
{edgeStatements}

% The number of partitions
k({k}).

% Declaration of vertex and subset
1 {{ part(V, 1..K) }} :- vertex(V), k(K).

% Minimization of the sum of weights of edges between parts
cut_edge(X, Y, W) :- edge(X, Y, W), part(X, P1), part(Y, P2), P1 != P2.
#minimize {{ W,X,Y : cut_edge(X, Y, W) }}.

% Predicates counting the number of vertices in subsets
part_size(P, S) :- S = #count {{ V : part(V, P) }}, k(K), P = 1..K.

% Limitation of the equality of the number of vertices in subsets
:- k(K), P1 = 1..K, P2 = 1..K, P1 < P2, part_size(P1, S1), part_size(P2, S2), S1 != S2.

% Printing the result
#show part/2.
";

		return aspProgram;
	}

	/// <summary>
	/// Generates a list of edges based on the given adjacency matrix.
	/// </summary>
	/// <param name="adjacencyMatrix">The adjacency matrix of the graph.</param>
	/// <returns>Returns list of edges.</returns>
	private static List<(int, int)> GenerateEdges(double[,] adjacencyMatrix)
	{
		List<(int, int)> edges = new();
		for (int i = 0; i < adjacencyMatrix.GetLength(0); i++)
		{
			for (int j = i + 1; j < adjacencyMatrix.GetLength(1); j++)
			{
				if (adjacencyMatrix[i, j] == 1)
					edges.Add((i, j));
			}
		}
		return edges;
	}

	/// <summary>
	/// Generates a list of edges with weights based on the given adjacency matrix and weights matrix.
	/// </summary>
	/// <param name="adjacencyMatrix">The adjacency matrix of the graph.</param>
	/// <param name="weightsMatrix">The weights matrix of the graph.</param>
	/// <returns>Returns list of edges with weights.</returns>
	private static List<(int, int, int)> GenerateEdgesWithWeights(double[,] adjacencyMatrix, double[,] weightsMatrix)
	{
		List<(int, int, int)> edges = new();
		for (int i = 0; i < adjacencyMatrix.GetLength(0); i++)
		{
			for (int j = i + 1; j < adjacencyMatrix.GetLength(1); j++)
			{
				if (adjacencyMatrix[i, j] == 1)
					edges.Add((i, j, (int)weightsMatrix[i, j]));
			}
		}
		return edges;
	}
}
