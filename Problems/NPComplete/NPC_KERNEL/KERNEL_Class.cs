using API.DummyClasses;
using API.Interfaces;
using API.Problems.NPComplete.NPC_KERNEL.Solvers;
using API.Problems.NPComplete.NPC_KERNEL.Verifiers;
using SPADE;

namespace API.Problems.NPComplete.NPC_KERNEL;

class KERNEL : IGraphProblem<KernelBruteForce, KernelVerifier, DummyVisualization, UtilCollectionGraph> {
    public string problemName { get; } = "Finding Kernel";
    public string problemLink { get; } = "https://users.encs.concordia.ca/~chvatal/kernel.html";
    public string formalDefinition { get; } = "KERNEL = {<G> | G = (N,E) is a directed graph and there exists S subset N such that E intersect (S cross S) is empty and, for every u in N-S, there exists v in S with (u,v) in E}";
    public string problemDefinition { get; } = "Determine whether a directed graph has a kernel: an independent set S such that every vertex outside S has an outgoing arc to a vertex in S.";
    public string inputDescription { get; } = "G, a directed graph";
    public string outputDescription { get; } = "True or False, whether G has a kernel";
    public string source { get; } = "V. Chvatal, On the computational complexity of finding a kernel, Report CRM-300, Centre de recherches mathematiques, Universite de Montreal, 1973.";
    public string sourceFile { get; } = SourceFile.Path();
    public string sourceLink { get; } = "https://users.encs.concordia.ca/~chvatal/kernel.html";
    public string wikiName { get; } = "";
    public string[] contributors { get; } = Array.Empty<string>();
    public ComplexityClass complexityClass { get; } = ComplexityClass.NPComplete;
    public ProblemType problemType { get; } = ProblemType.GraphTheory;

    public const string InstanceGrammar = "{(N,E) | N is set, E subset N cross N}";
    private const string _defaultInstance = "({1,2,3},{(1,2),(2,3)})";
    public string defaultInstance { get; } = _defaultInstance;
    public string instance { get; set; } = string.Empty;
    public string instanceFormat { get; } = $"Directed graph (N,E), with a set of vertices and a set of ordered arcs (u,v). Format: {InstanceGrammar} Example: {_defaultInstance}";
    public string certificateFormat { get; } = $"Format: {KernelVerifier.CertificateGrammar} Example: {KernelVerifier.CertificateExample}";
    public List<string> nodes { get; set; }
    public List<KeyValuePair<string, string>> edges { get; set; }
    public UtilCollectionGraph graph { get; set; }
    public KernelBruteForce defaultSolver { get; } = new KernelBruteForce();
    public KernelVerifier defaultVerifier { get; } = new KernelVerifier();
    public DummyVisualization defaultVisualization { get; } = new DummyVisualization();

    public KERNEL() : this(_defaultInstance) {
    }

    public KERNEL(string stringInstance) {
        instance = stringInstance;
        if (string.IsNullOrWhiteSpace(stringInstance)) {
            throw new ProblemParseException(problemName, stringInstance, "instance is empty");
        }

        try {
            StringParser parser = new(InstanceGrammar);
            parser.parse(stringInstance);
            nodes = parser["N"].ToList().Select(node => node.ToString()).ToList();
            edges = parser["E"].ToList().Select(edge =>
                new KeyValuePair<string, string>(edge[0].ToString(), edge[1].ToString())).ToList();
            HashSet<string> vertexSet = nodes.ToHashSet();
            if (edges.Any(edge => !vertexSet.Contains(edge.Key) || !vertexSet.Contains(edge.Value))) {
                throw new ProblemParseException(problemName, stringInstance, "every arc endpoint must belong to N");
            }
            graph = new UtilCollectionGraph(parser["N"], parser["E"]);
        } catch (Exception ex) when (ParseGuard.IsCertificateParseFailure(ex)) {
            throw new ProblemParseException(problemName, stringInstance, "expected a directed graph (N,E) with ordered arcs", ex);
        }
    }
}
