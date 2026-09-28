using System.Threading.Tasks;
using Xunit;
using Verifier =
    TestHelper.UI.Analyzers.Tests.TestDataVerifier<TestHelper.UI.Analyzers.PaginatorConstructorNotPreservedAnalyzer>;

namespace TestHelper.UI.Analyzers.Tests
{
    public class PaginatorConstructorNotPreservedAnalyzerTest
    {
        [Theory]
        [InlineData("NoArgs_ConstructorNotPreserved", 12, 18, "NoArgs_ConstructorNotPreservedPaginator")]
        [InlineData("NoArgs_ImplicitDefaultConstructorAndClassNotPreserved", 12, 18, "NoArgs_ImplicitDefaultConstructorAndClassNotPreservedPaginator")]
        [InlineData("WithArgs_MatchingConstructorNotPreserved", 12, 18, "WithArgs_MatchingConstructorNotPreservedPaginator")]
        [InlineData("ArgsArrayVariable_AnyConstructorNotPreserved", 13, 18, "ArgsArrayVariable_AnyConstructorNotPreservedPaginator")]
        public async Task NotPreservedConstructor_ReportsAtRegister(string caseName, int line, int column, string typeName)
        {
            var expected = Verifier.Diagnostic().WithLocation(line, column).WithArguments(typeName);
            await Verifier.VerifyAsync($"TestHelperUI4008/{caseName}.cs", expected);
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
            await Verifier.VerifyAsync($"TestHelperUI4008/{caseName}.cs");
        }
    }
}
