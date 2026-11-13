
using API.Interfaces;
using API.Interfaces.JSON_Objects;
using API.Interfaces.JSON_Objects.Graphs;
using API.Problems.NPComplete.NPC_MINESWEEPER.Solvers;

namespace API.Problems.NPComplete.NPC_MINESWEEPER.Visualizations;

class MinesweeperDefaultVisualization
    : IVisualization<MINESWEEPER, API_GraphJSON> {

    public string visualizationName { get; } =
        "Minesweeper Consistency Visualization";

    public string visualizationDefinition { get; } =
        "Displays a Minesweeper board as a graph, connecting adjacent " +
        "cells and highlighting mine locations in a solution.";

    public string source { get; } =
        "Kaye, Richard. 2000. Minesweeper is NP-complete. " +
        "The Mathematical Intelligencer 22(2): 9-15.";

    public string sourceFile { get; } = SourceFile.Path();

    public string[] contributors { get; } = { "Prashant Pant" };

    public VisualizationType visualizationType { get; } =
        VisualizationType.GraphD3;

    public ISolver solver { get; } = new MinesweeperSolver();

    public MinesweeperDefaultVisualization() { }

    public API_GraphJSON visualize(MINESWEEPER problem) {
        var nodes = new List<string>();
        var edges = new List<KeyValuePair<string, string>>();

        for (int r = 0; r < problem.Rows; r++) {
            for (int c = 0; c < problem.Columns; c++) {
                nodes.Add(NodeName(r, c, problem.grid[r][c]));

                // Connect each cell to its neighbors.
                // Only add edges in forward directions to avoid duplicates.
                int[,] directions = {
                    { 0, 1 },
                    { 1, -1 },
                    { 1, 0 },
                    { 1, 1 }
                };

                for (int d = 0; d < directions.GetLength(0); d++) {
                    int nr = r + directions[d, 0];
                    int nc = c + directions[d, 1];

                    if (nr >= 0 && nr < problem.Rows &&
                        nc >= 0 && nc < problem.Columns) {
                        edges.Add(new KeyValuePair<string, string>(
                            NodeName(r, c, problem.grid[r][c]),
                            NodeName(nr, nc, problem.grid[nr][nc])
                        ));
                    }
                }
            }
        }

        var graph = new API_GraphJSON(nodes, edges);

        foreach (var node in graph.nodes) {
            if (node.name.EndsWith(":?"))
                node.color = "Unknown";
            else
                node.color = "Clue";
        }

        return graph;
    }

    public API_JSON SolvedVisualization(
        MINESWEEPER problem, string solution) {

        API_GraphJSON graph = visualize(problem);

        if (solution == "{}" ||
            solution == SolveRun.TimedOutAnswer ||
            string.IsNullOrWhiteSpace(solution))
            return graph;

        // Never highlight an invalid solution.
        var verifier = new Verifiers.MinesweeperVerifier();

        if (!verifier.verify(problem, solution))
            return graph;

        string[] rows = solution.Split(';');

        for (int r = 0; r < problem.Rows; r++) {
            string[] cells = rows[r].Split(',');

            for (int c = 0; c < problem.Columns; c++) {
                string name = NodeName(r, c, problem.grid[r][c]);

                var node = graph.nodes.First(n => n.name == name);

                if (cells[c].Trim() == "1")
                    node.color = "Solution";
            }
        }

        return graph;
    }

    private static string NodeName(int row, int col, int value) {
        string label = value == -1 ? "?" : value.ToString();
        return $"({row},{col}):{label}";
    }
}
