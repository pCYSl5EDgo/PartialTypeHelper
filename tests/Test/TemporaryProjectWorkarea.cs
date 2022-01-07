using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace PartialTypeHelper.Test;

public sealed class TemporaryProjectWorkarea : IDisposable
{
    private readonly string tempDirPath;
    private readonly string targetCsprojFileName = "TempTargetProject.csproj";
    private readonly string referencedCsprojFileName = "TempReferencedProject.csproj";
    private readonly bool cleanOnDisposing;

    /// <summary>
    /// Gets the identifier of the workarea.
    /// </summary>
    public Guid WorkareaId { get; }

    /// <summary>
    /// Gets Generator target csproj file path.
    /// </summary>
    public string TargetCsProjectPath { get; }

    /// <summary>
    /// Gets csproj file path Referenced from TargetProject.
    /// </summary>
    public string ReferencedCsProjectPath { get; }

    public string TargetProjectDirectory { get; }

    public string ReferencedProjectDirectory { get; }

    public string OutputDirectory { get; }

    public static TemporaryProjectWorkarea Create(bool cleanOnDisposing = true)
    {
        return new TemporaryProjectWorkarea(cleanOnDisposing);
    }

    private TemporaryProjectWorkarea(bool cleanOnDisposing)
    {
        WorkareaId = Guid.NewGuid();
        this.cleanOnDisposing = cleanOnDisposing;
        this.tempDirPath = Path.Combine(Path.GetTempPath(), $"PartialTypeHelper.Tests-{WorkareaId}");

        TargetProjectDirectory = Path.Combine(tempDirPath, "TargetProject");
        ReferencedProjectDirectory = Path.Combine(tempDirPath, "ReferencedProject");
        OutputDirectory = Path.Combine(tempDirPath, "Output");

        Directory.CreateDirectory(TargetProjectDirectory);
        Directory.CreateDirectory(ReferencedProjectDirectory);
        Directory.CreateDirectory(OutputDirectory);
        Directory.CreateDirectory(Path.Combine(OutputDirectory, "bin"));

        var thisLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        Debug.Assert(thisLocation is not null);
        var solutionRootDir = Path.GetFullPath(Path.Combine(thisLocation, "../../../.."));
        var helperProjectDir = Path.Combine(solutionRootDir, "src/PartialTypeHelper/PartialTypeHelper.csproj");

        ReferencedCsProjectPath = Path.Combine(ReferencedProjectDirectory, referencedCsprojFileName);
        var referencedCsprojContents = @"
<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include=""" + helperProjectDir + @""" />
  </ItemGroup>
</Project>
";
        AddFileToReferencedProject(referencedCsprojFileName, referencedCsprojContents);

        TargetCsProjectPath = Path.Combine(TargetProjectDirectory, targetCsprojFileName);
        var csprojContents = @"
<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include=""" + helperProjectDir + @""" />
    <ProjectReference Include=""" + ReferencedCsProjectPath + @""" />
  </ItemGroup>
</Project>
";
        AddFileToTargetProject(targetCsprojFileName, csprojContents);
    }

    /// <summary>
    /// Add file to target project.
    /// </summary>
    public void AddFileToTargetProject(string fileName, string contents)
    {
        File.WriteAllText(Path.Combine(TargetProjectDirectory, fileName), contents.Trim());
    }

    /// <summary>
    /// Add file to project, referenced by target project.
    /// </summary>
    public void AddFileToReferencedProject(string fileName, string contents)
    {
        File.WriteAllText(Path.Combine(ReferencedProjectDirectory, fileName), contents.Trim());
    }

    public Compilation GetOutputCompilation()
    {
        var refAsmDir = Path.GetDirectoryName(typeof(object).Assembly.Location);
        Debug.Assert(refAsmDir is not null);

        var referenceCompilation = CSharpCompilation.Create(Guid.NewGuid().ToString())
            .AddSyntaxTrees(
                Directory.EnumerateFiles(ReferencedProjectDirectory, "*.cs", SearchOption.AllDirectories)
                    .Select(x => CSharpSyntaxTree.ParseText(File.ReadAllText(x), CSharpParseOptions.Default, x)))
            .AddReferences(MetadataReference.CreateFromFile(Path.Combine(refAsmDir, "System.Private.CoreLib.dll")))
            .AddReferences(MetadataReference.CreateFromFile(Path.Combine(refAsmDir, "System.Runtime.Extensions.dll")))
            .AddReferences(MetadataReference.CreateFromFile(Path.Combine(refAsmDir, "System.Collections.dll")))
            .AddReferences(MetadataReference.CreateFromFile(Path.Combine(refAsmDir, "System.Linq.dll")))
            .AddReferences(MetadataReference.CreateFromFile(Path.Combine(refAsmDir, "System.Console.dll")))
            .AddReferences(MetadataReference.CreateFromFile(Path.Combine(refAsmDir, "System.Runtime.dll")))
            .AddReferences(MetadataReference.CreateFromFile(Path.Combine(refAsmDir, "System.Memory.dll")))
            .AddReferences(MetadataReference.CreateFromFile(Path.Combine(refAsmDir, "netstandard.dll")))
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var compilation = CSharpCompilation.Create(Guid.NewGuid().ToString())
            .AddSyntaxTrees(
                Directory.EnumerateFiles(TargetProjectDirectory, "*.cs", SearchOption.AllDirectories)
                    .Concat(Directory.EnumerateFiles(OutputDirectory, "*.cs", SearchOption.AllDirectories))
                    .Select(x => CSharpSyntaxTree.ParseText(File.ReadAllText(x), CSharpParseOptions.Default, x)))
            .AddReferences(referenceCompilation.ToMetadataReference())
            .AddReferences(MetadataReference.CreateFromFile(Path.Combine(refAsmDir, "System.Private.CoreLib.dll")))
            .AddReferences(MetadataReference.CreateFromFile(Path.Combine(refAsmDir, "System.Runtime.Extensions.dll")))
            .AddReferences(MetadataReference.CreateFromFile(Path.Combine(refAsmDir, "System.Collections.dll")))
            .AddReferences(MetadataReference.CreateFromFile(Path.Combine(refAsmDir, "System.Linq.dll")))
            .AddReferences(MetadataReference.CreateFromFile(Path.Combine(refAsmDir, "System.Console.dll")))
            .AddReferences(MetadataReference.CreateFromFile(Path.Combine(refAsmDir, "System.Runtime.dll")))
            .AddReferences(MetadataReference.CreateFromFile(Path.Combine(refAsmDir, "System.Memory.dll")))
            .AddReferences(MetadataReference.CreateFromFile(Path.Combine(refAsmDir, "netstandard.dll")))
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        return compilation;
    }

    public void Dispose()
    {
        if (cleanOnDisposing)
        {
            Directory.Delete(tempDirPath, true);
        }
    }
}

public static class CompilationHelper
{
    public static INamedTypeSymbol[] GetNamedTypeSymbolsFromGenerated(this Compilation compilation)
    {
        return compilation.SyntaxTrees
            .Select(x => compilation.GetSemanticModel(x))
            .SelectMany(semanticModel =>
            {
                return semanticModel.SyntaxTree.GetRoot()
                    .DescendantNodes()
                    .Select(x => semanticModel.GetDeclaredSymbol(x))
                    .OfType<INamedTypeSymbol>();
            })
            .ToArray();
    }
}
