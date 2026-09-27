using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace TestHelper.UI.Analyzers.Tests
{
    /// <summary>
    /// Reads a fixture from TestData/, runs the analyzer on it together with the dummies under TestData/Dummies/,
    /// and checks the reported diagnostics against the expected ones.
    /// Fixtures and dummies are real .cs files compiled by the Tests project, not string literals,
    /// so Unity/NUnit/TestHelper.UI signatures are type-checked at build time.
    /// </summary>
    internal sealed class TestDataVerifier<TAnalyzer> : AnalyzerVerifier<TAnalyzer, TestDataVerifier<TAnalyzer>.Test, XUnitVerifier>
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        /// <param name="testDataPath">Path relative to TestData/, e.g. "TestHelperUI1001/CaseName.cs"</param>
        /// <param name="expected">Diagnostics the analyzer must report on the fixture</param>
        public static Task VerifyAsync(string testDataPath, params DiagnosticResult[] expected)
        {
            return VerifyAsync(new Test(), testDataPath, expected);
        }

        /// <param name="test">A subclass instance when the case needs more than the single analyzer</param>
        /// <param name="testDataPath">Path relative to TestData/, e.g. "TestHelperUI1001/CaseName.cs"</param>
        /// <param name="expected">Diagnostics the analyzer must report on the fixture</param>
        public static Task VerifyAsync(Test test, string testDataPath, params DiagnosticResult[] expected)
        {
            test.TestCode = File.ReadAllText(Path.Combine(TestDataFiles.Root, testDataPath));
            // WithLocation(line, col) without a path resolves against DefaultFilePath ("/0/Test0.cs"), i.e. Sources[0].
            // The fixture must therefore be added first (via TestCode above) and the dummies only afterwards;
            // adding the dummies in the constructor would push the fixture to a later index and break every location assertion.
            foreach (var dummy in TestDataFiles.Dummies)
            {
                test.TestState.Sources.Add(dummy);
            }

            test.ExpectedDiagnostics.AddRange(expected);
            return test.RunAsync();
        }

        internal class Test : CSharpAnalyzerTest<TAnalyzer, XUnitVerifier>
        {
            public Test()
            {
                // Unity 6 compiles against .NET Standard 2.1. Net60 and later also work but would let fixtures
                // use APIs that do not exist in Unity.
                ReferenceAssemblies = ReferenceAssemblies.NetStandard.NetStandard21;
            }

            protected override CompilationOptions CreateCompilationOptions()
            {
                // The Tests project has Nullable=enable while the verifier defaults to disabled.
                // Without this, a fixture containing string? builds in the Tests project but fails with CS8632 in the verifier.
                var options = (CSharpCompilationOptions)base.CreateCompilationOptions();
                return options.WithNullableContextOptions(NullableContextOptions.Enable);
            }
        }
    }

    /// <summary>
    /// Paths and dummy sources under TestData/. <see cref="TestDataVerifier{TAnalyzer}"/> gets a separate static field per type argument,
    /// so the directory scan lives in this non-generic type and runs once.
    /// </summary>
    internal static class TestDataFiles
    {
        public static string Root { get; } = Path.Combine(AppContext.BaseDirectory, "TestData");

        public static ImmutableArray<string> Dummies { get; } = Directory
            .EnumerateFiles(Path.Combine(Root, "Dummies"), "*.cs", SearchOption.AllDirectories)
            .OrderBy(path => path, StringComparer.Ordinal)
            .Select(File.ReadAllText)
            .ToImmutableArray();
    }
}
