using API.Interfaces;
using SPADE;

namespace API.Problems.NPComplete.NPC_KERNEL.Verifiers;

class KernelVerifier : IVerifier<KERNEL> {
    public const string CertificateGrammar = "S, a set of vertices in N with no internal arcs, and an arc u->v into S from every u outside S";
    public const string CertificateExample = "{1,3}";
    public string verifierName { get; } = "Finding Kernel Solution Verifier";
    public string verifierDefinition { get; } = "Checks independence and that every vertex outside S has an outgoing arc into S.";
    public string source { get; } = "https://users.encs.concordia.ca/~chvatal/kernel.html";
    public string sourceFile { get; } = SourceFile.Path();
    public string[] contributors { get; } = Array.Empty<string>();
    public string certificate { get; private set; } = string.Empty;

    public bool verify(KERNEL problem, string certificate) {
        this.certificate = certificate;
        string text = certificate.Trim();
        if (!text.StartsWith('{') || !text.EndsWith('}')) {
            throw new CertificateParseException(problem, certificate, "expected a set enclosed in braces, for example {1,3}");
        }
        HashSet<string> chosen;
        try {
            StringParser parser = new("{S | S is set}");
            parser.parse(certificate);
            chosen = parser["S"].ToList().Select(node => node.ToString()).ToHashSet();
        } catch (Exception ex) when (ParseGuard.IsCertificateParseFailure(ex)) {
            throw new CertificateParseException(problem, certificate, "expected a set of vertices, for example {1,3}", ex);
        }
        return IsKernel(problem, chosen);
    }

    internal static bool IsKernel(KERNEL problem, HashSet<string> chosen) {
        if (!chosen.IsSubsetOf(problem.nodes)) return false;

        HashSet<string> absorbed = new();
        foreach (var arc in problem.edges) {
            if (chosen.Contains(arc.Value)) {
                if (chosen.Contains(arc.Key)) return false;
                absorbed.Add(arc.Key);
            }
        }
        return problem.nodes.All(node => chosen.Contains(node) || absorbed.Contains(node));
    }
}
