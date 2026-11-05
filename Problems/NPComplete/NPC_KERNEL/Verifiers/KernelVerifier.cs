// Contributors: Prashant Pant and Bektur Akkabakov.
// LLM implementation of polynomial-time kernel certificate verification.
using System.Globalization;
using API.Interfaces;

namespace API.Problems.NPComplete.NPC_KERNEL.Verifiers;

class KernelVerifier : IVerifier<KERNEL> {
    public const string CertificateGrammar = "{v1,...,vk} | distinct nonnegative Int32 vertex labels forming a subset S of V; no arc has both endpoints in S, and every vertex outside S has an outgoing arc into S; {} denotes the empty candidate set";
    public const string CertificateExample = "{0,2}";
    public string verifierName { get; } = "Kernel Solution Verifier";
    public string verifierDefinition { get; } = "Checks that the proposed vertex set is independent and absorbs every outside vertex along an outgoing arc. Malformed certificates raise CertificateParseException; well-formed invalid candidates return false. The empty set is valid only for an empty graph, and is not a proof that no kernel exists.";
    public string source { get; } = "V. Chvatal, On the computational complexity of finding a kernel, Report CRM-300, Centre de recherches mathematiques, Universite de Montreal, 1973; author's online account. This verifier directly implements the two defining kernel conditions.";
    public string sourceFile { get; } = SourceFile.Path();
    public string sourceLink { get; } = "https://users.encs.concordia.ca/~chvatal/kernel.html";
    public string[] contributors { get; } = { "Prashant Pant", "Bektur Akkabakov" };
    // Balanced trees give a polynomial worst-case bound, without hash-table assumptions.
    public string complexity { get; } = "O(L + (n + m + k) * log(n + k + 2)), L = certificate length, n = |V|, m = |E|, k = certificate vertex count";
    private string _certificate = "";

    public string certificate {
        get { return _certificate; }
    }

    public bool verify(KERNEL problem, string certificate) {
        _certificate = "";
        if (string.IsNullOrWhiteSpace(certificate)) {
            throw new CertificateParseException(problem, certificate ?? "", "Certificate must be a brace-delimited vertex set.");
        }

        string text = certificate.Trim();
        if (text.Length < 2 || text[0] != '{' || text[text.Length - 1] != '}') {
            throw new CertificateParseException(problem, certificate, "Expected a vertex set such as {0,2}.");
        }

        var selected = new SortedSet<int>();
        string body = text.Substring(1, text.Length - 2);
        if (!string.IsNullOrWhiteSpace(body)) {
            foreach (string entry in body.Split(',')) {
                string token = entry.Trim();
                int vertex;
                // NumberStyles.None excludes signs, decimals and embedded whitespace.
                if (!int.TryParse(token, NumberStyles.None, CultureInfo.InvariantCulture, out vertex) || vertex < 0) {
                    throw new CertificateParseException(problem, certificate, "Each entry must be a nonnegative Int32 vertex label.");
                }
                if (!selected.Add(vertex)) {
                    throw new CertificateParseException(problem, certificate, "Vertex labels must be distinct.");
                }
            }
        }
        _certificate = certificate;

        var vertices = new SortedSet<int>(problem.vertices);
        if (!selected.IsSubsetOf(vertices)) return false;

        var absorbed = new SortedSet<int>();
        foreach (var edge in problem.edges) {
            if (!selected.Contains(edge.v)) continue;
            // This also rejects a selected vertex with a self-loop.
            if (selected.Contains(edge.u)) return false;
            absorbed.Add(edge.u);
        }

        foreach (int vertex in vertices) {
            if (!selected.Contains(vertex) && !absorbed.Contains(vertex)) return false;
        }
        return true;
    }
}
