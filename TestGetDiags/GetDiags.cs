using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestGetDiags;
internal static class GetDiags
{
    public static async Task Run([CallerFilePath] string? sourceFilePath = null)
    {
        string testProjectPath = Path.Combine(Directory.GetParent(Path.GetDirectoryName(sourceFilePath)).FullName,
            "AnalyzedProj", "FailedDiagsTst.csproj");
        await RunInternal(testProjectPath, CancellationToken.None);
    }

    public static async Task RunInternal(string projectFilePath, CancellationToken cancellationToken)
    {
        using var workspace = MSBuildWorkspace.Create();
        Project project = await workspace.OpenProjectAsync(projectFilePath, cancellationToken: cancellationToken);
        

        var compilation = await project.GetCompilationAsync(cancellationToken);

        if (compilation is null)
        {
            Console.WriteLine("Compilation is null");
            return;
        }

        ImmutableArray<Diagnostic> regularDiagnostics = compilation.GetDiagnostics(cancellationToken);

        Console.WriteLine("Compilation diagnostics:");
        foreach (Diagnostic diagnostic in regularDiagnostics)
        {
            Console.WriteLine(diagnostic.ToString());
        }
        Console.WriteLine("========================");
        Console.WriteLine();

        var diagnostics = await GetAnalyzerDiagnosticsAsync(compilation, project, cancellationToken);

        Console.WriteLine("Analyzers diagnostics:");
        foreach (Diagnostic analyzerDiagnostic in diagnostics)
        {
            Console.WriteLine(analyzerDiagnostic.ToString());
        }

    }

    internal static async Task<ImmutableArray<Diagnostic>> GetAnalyzerDiagnosticsAsync(Compilation compilation,
        Project project, CancellationToken cancellationToken)
    {
        return await compilation
            .WithAnalyzers([..project.AnalyzerReferences.SelectMany(ar => ar.GetAnalyzers(project.Language))])
            .GetAnalyzerDiagnosticsAsync(cancellationToken);
    }
}

