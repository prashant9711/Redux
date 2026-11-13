
using Xunit;
using API.Interfaces;
using API.Problems.NPComplete.NPC_MINESWEEPER;
using API.Problems.NPComplete.NPC_MINESWEEPER.Solvers;
using API.Problems.NPComplete.NPC_MINESWEEPER.Verifiers;
using API.Problems.NPComplete.NPC_MINESWEEPER.Visualizations;

namespace redux_tests;

#pragma warning disable CS1591

public class MINESWEEPER_Tests {

    private readonly MinesweeperSolver _solver = new();
    private readonly MinesweeperVerifier _verifier = new();

    // Parser tests

    [Theory]
    [InlineData("?", 1, 1)]
    [InlineData("0,?;?,1", 2, 2)]
    [InlineData("1,?,1;?,?,?;1,?,1", 3, 3)]
    [InlineData("0,?,?;1,?,?", 2, 3)]
    public void Parser_AcceptsValidBoards(
        string input, int rows, int columns) {

        var problem = new MINESWEEPER(input);

        Assert.Equal(rows, problem.Rows);
        Assert.Equal(columns, problem.Columns);
    }

    [Theory]
    [InlineData("")]
    [InlineData("1,?;?,1,?")]
    [InlineData("1,9")]
    [InlineData("1,-1")]
    [InlineData("1,x")]
    [InlineData("1,;?,1")]
    [InlineData(";1,?")]
    [InlineData("1,?;")]
    public void Parser_RejectsInvalidBoards(string input) {
        Assert.Throws<ProblemParseException>(
            () => new MINESWEEPER(input));
    }

    // Verifier tests

    [Fact]
    public void Verifier_AcceptsValidCertificate() {
        var problem = new MINESWEEPER();

        Assert.True(_verifier.verify(
            problem, MinesweeperVerifier.CertificateExample));
    }

    [Theory]
    [InlineData("0,0,0;0,0,0;0,0,0")]
    [InlineData("0,1,0;0,0,0;0,0,0")]
    [InlineData("1,0,0;1,0,1;0,0,0")]
    [InlineData("0,0,0;1,1,1;0,0,0")]
    public void Verifier_RejectsIncorrectPlacements(string certificate) {
        var problem = new MINESWEEPER();

        Assert.False(_verifier.verify(problem, certificate));
    }

    [Theory]
    [InlineData("")]
    [InlineData("{}")]
    [InlineData("0,0;1,0")]
    [InlineData("0,0,0;1,0;0,0,0")]
    [InlineData("0,0,0;1,x,1;0,0,0")]
    [InlineData("0,0,0;1,2,1;0,0,0")]
    [InlineData("0,0,0;1,-1,1;0,0,0")]
    [InlineData("0,0,0;1,0,1;0,0,0;0,0,0")]
    public void Verifier_RejectsMalformedCertificates(
        string certificate) {

        Assert.False(_verifier.verify(
            new MINESWEEPER(), certificate));
    }

    // Solver tests

    [Theory]
    [InlineData("1,?,1;?,?,?;1,?,1")]
    [InlineData("0,?")]
    [InlineData("1,?")]
    [InlineData("?,1,?")]
    [InlineData("?")]
    [InlineData("0")]
    public void Solver_ProducesVerifiableSolution(string input) {
        var problem = new MINESWEEPER(input);

        string solution = _solver.solve(problem);

        Assert.NotEqual("{}", solution);
        Assert.True(_verifier.verify(problem, solution),
            $"Invalid solver output for {input}: {solution}");
    }

    [Theory]
    [InlineData("1")]
    [InlineData("2,?")]
    [InlineData("0,1")]
    [InlineData("1,?;?,0")]
    public void Solver_ReturnsNoSolution(string input) {
        var problem = new MINESWEEPER(input);

        Assert.Equal("{}", _solver.solve(problem));
    }

    [Fact]
    public void Solver_DoesNotModifyOriginalBoard() {
        var problem = new MINESWEEPER();
        var original = problem.grid.Select(
            row => row.ToArray()).ToArray();

        _solver.solve(problem);

        for (int r = 0; r < problem.Rows; r++)
            Assert.Equal(original[r], problem.grid[r]);
    }

    [Fact]
    public void Solver_RespectsTimeout() {
        var problem = new MINESWEEPER();

        var solver = new MinesweeperSolver {
            timerHasExpired = true
        };

        Assert.Equal(SolveRun.TimedOutAnswer,
            solver.solve(problem));
    }

    // Metadata tests

    [Fact]
    public void CertificateExample_IsValidForDefaultInstance() {
        var problem = new MINESWEEPER();

        Assert.True(_verifier.verify(
            problem, MinesweeperVerifier.CertificateExample));
    }

    [Fact]
    public void Problem_DeclaresCorrectComplexity() {
        var problem = new MINESWEEPER();

        Assert.Equal(ComplexityClass.NPComplete,
            problem.complexityClass);

        Assert.Equal(ProblemVariant.Decision,
            problem.problemVariant);
    }

    // Visualization tests

    [Fact]
    public void Visualization_CreatesCorrectGraph() {
        var problem = new MINESWEEPER();
        var visualization = new MinesweeperDefaultVisualization();

        var graph = visualization.visualize(problem);

        Assert.Equal(9, graph.nodes.Count);
        Assert.Equal(20, graph.links.Count);
        Assert.Contains(graph.nodes,
            node => node.name == "(0,0):1");
        Assert.Contains(graph.nodes,
            node => node.name == "(1,1):?");
    }

    [Fact]
    public void SolvedVisualization_HighlightsMines() {
        var problem = new MINESWEEPER();
        var visualization = new MinesweeperDefaultVisualization();

        var graph = visualization.SolvedVisualization(
            problem, MinesweeperVerifier.CertificateExample);

        var result = Assert.IsType<
            API.Interfaces.JSON_Objects.Graphs.API_GraphJSON>(graph);

        Assert.Equal("Solution",
            result.nodes.Single(n => n.name == "(1,0):?").color);

        Assert.Equal("Solution",
            result.nodes.Single(n => n.name == "(1,2):?").color);

        Assert.NotEqual("Solution",
            result.nodes.Single(n => n.name == "(0,0):1").color);
    }
}
