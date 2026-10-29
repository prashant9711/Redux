using API.Interfaces;
using API.Problems.NPComplete.NPC_KERNEL.Verifiers;

namespace API.Problems.NPComplete.NPC_KERNEL.Solvers;

class KernelBruteForce : ISolver<KERNEL> {
    public string solverName { get; } = "Finding Kernel Brute Force";
    public string solverDefinition { get; } = "Enumerates vertex subsets and returns the first kernel, or an empty string if none exists. Returns an empty string if the timer expires.";
    public string source { get; } = "Exhaustive subset enumeration using the kernel definition.";
    public string sourceFile { get; } = SourceFile.Path();
    public string[] contributors { get; } = Array.Empty<string>();
    public bool timerHasExpired { get; set; }
    public SolverType solverType { get; } = SolverType.BruteForce;
    public SolverComplexityBucket complexityBucket { get; } = SolverComplexityBucket.Exponential;
    public MathematicalFunction timeComplexity { get; } = new("2^n * (n + m)", ", n = number of vertices, m = number of arcs");
    public MathematicalFunction spaceComplexity { get; } = new("n", ", n = number of vertices");

    public string solve(KERNEL problem) {
        HashSet<string> chosen = new();
        return Search(0) ?? string.Empty;

        string? Search(int index) {
            if (timerHasExpired) return null;
            if (index == problem.nodes.Count) {
                return KernelVerifier.IsKernel(problem, chosen)
                    ? "{" + string.Join(",", problem.nodes.Where(chosen.Contains)) + "}"
                    : null;
            }

            string? answer = Search(index + 1);
            if (answer != null || timerHasExpired) return answer;
            chosen.Add(problem.nodes[index]);
            answer = Search(index + 1);
            chosen.Remove(problem.nodes[index]);
            return answer;
        }
    }
}
