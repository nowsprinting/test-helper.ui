using System.Threading.Tasks;
using Xunit;
using Verifier =
    TestHelper.UI.Analyzers.Tests.TestDataVerifier<TestHelper.UI.Analyzers.OperatorConstructorNotPreservedAnalyzer>;

namespace TestHelper.UI.Analyzers.Tests
{
    public class OperatorConstructorNotPreservedAnalyzerTest
    {
        [Theory]
        [InlineData("NoArgs_ConstructorNotPreserved", 14, 18, "NoArgs_ConstructorNotPreservedOperator")]
        [InlineData("NoArgs_ImplicitDefaultConstructorAndClassNotPreserved", 14, 18, "NoArgs_ImplicitDefaultConstructorAndClassNotPreservedOperator")]
        [InlineData("WithArgs_MatchingConstructorNotPreserved", 14, 18, "WithArgs_MatchingConstructorNotPreservedOperator")]
        [InlineData("ArgsArrayVariable_AnyConstructorNotPreserved", 15, 18, "ArgsArrayVariable_AnyConstructorNotPreservedOperator")]
        public async Task NotPreservedConstructor_ReportsAtRegister(string caseName, int line, int column, string typeName)
        {
            var expected = Verifier.Diagnostic().WithLocation(line, column).WithArguments(typeName);
            await Verifier.VerifyAsync($"TestHelperUI4003/{caseName}.cs", expected);
        }

        [Theory]
        [InlineData("NoArgs_ConstructorPreserved")]
        [InlineData("NoArgs_ImplicitDefaultConstructorAndClassPreserved")]
        [InlineData("CustomPreserveAttributeInOtherNamespace")]
        [InlineData("WithArgs_OnlyNonMatchingConstructorNotPreserved")]
        [InlineData("NoArgs_RequiredParameterAndNotPreserved")]
        [InlineData("NoArgs_MultipleConstructorsNotPreserved")]
        public async Task PreservedOrOutOfScope_ReportsNothing(string caseName)
        {
            await Verifier.VerifyAsync($"TestHelperUI4003/{caseName}.cs");
        }
    }
}
