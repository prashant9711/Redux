// Contributors: Prashant Pant and Bektur Akkabakov. LLM implementation.
using API.Interfaces;

namespace API.Problems.NPComplete.NPC_KERNEL.Solvers;

class KernelBruteForce : ISolver<KERNEL> {
    public string solverName { get; } = "Kernel Brute Force";
    public string solverDefinition { get; } = "Enumerates all vertex subsets without pruning; checks independence and outgoing absorption. Returns a kernel as {v1,v2,...}, or {} if none exists (also the empty graph's kernel).";
    public string source { get; } = "V. Chvatal, On the computational complexity of finding a kernel, Report CRM-300, Universite de Montreal, 1973; author's online account. Subset enumeration is an original implementation of the definition, not an algorithm attributed to the report.";
    public string sourceLink { get; } = "https://users.encs.concordia.ca/~chvatal/kernel.html";
    public string sourceFile { get; } = SourceFile.Path();
    public string[] contributors { get; } = { "Prashant Pant", "Bektur Akkabakov" };
    public SolverType solverType { get; } = SolverType.BruteForce;
    public SolverComplexityBucket complexityBucket { get; } = SolverComplexityBucket.Exponential;
    // Up to 2^n subsets, each scanning n vertices and m arcs. HashSet operations
    // can cost O(n) under collisions: strict worst case below; with expected O(1)
    // hashing the time bound is O(2^n * (n + m + 1)).
    public MathematicalFunction timeComplexity { get; } = new MathematicalFunction("2^n * (n + 1) * (n + m + 1)", ", n = |V|, m = |E|; worst case including hash collisions");
    // Vertex array, Boolean counter and two sets; excludes the input graph.
    public MathematicalFunction spaceComplexity { get; } = new MathematicalFunction("n + 1", ", n = |V|; auxiliary space");
    private volatile bool expired;
    public bool timerHasExpired { get => expired; set => expired = value; }

    public string solve(KERNEL problem) {
        if (timerHasExpired) return SolveRun.TimedOutAnswer;
        int[] vertices = problem.vertices.ToArray();
        // Boolean counter avoids fixed-width masks and their 32/64-vertex limits.
        bool[] bits = new bool[vertices.Length];
        var selected = new HashSet<int>();
        var absorbed = new HashSet<int>();
        while (true) {
            if (timerHasExpired) return SolveRun.TimedOutAnswer;
            selected.Clear();
            for (int i = 0; i < vertices.Length; i++) {
                if (timerHasExpired) return SolveRun.TimedOutAnswer;
                if (bits[i]) selected.Add(vertices[i]);
            }
            bool valid = true;
            absorbed.Clear();
            foreach (var edge in problem.edges) {
                if (timerHasExpired) return SolveRun.TimedOutAnswer;
                // A self-loop forbids selecting its vertex.
                if (selected.Contains(edge.u) && selected.Contains(edge.v)) valid = false;
                if (selected.Contains(edge.v)) absorbed.Add(edge.u);
            }
            foreach (int vertex in vertices) {
                if (timerHasExpired) return SolveRun.TimedOutAnswer;
                if (!selected.Contains(vertex) && !absorbed.Contains(vertex)) valid = false;
            }
            if (valid) return "{" + string.Join(",", vertices.Where(selected.Contains)) + "}";
            int position = 0;
            while (position < bits.Length && bits[position]) {
                if (timerHasExpired) return SolveRun.TimedOutAnswer;
                bits[position++] = false;
            }
            if (position == bits.Length) return "{}";
            bits[position] = true;
        }
    }
}
