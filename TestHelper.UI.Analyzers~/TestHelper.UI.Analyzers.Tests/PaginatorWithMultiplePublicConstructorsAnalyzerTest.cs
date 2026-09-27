using System.Threading.Tasks;
using Xunit;
using Verifier =
    TestHelper.UI.Analyzers.Tests.TestDataVerifier<TestHelper.UI.Analyzers.PaginatorWithMultiplePublicConstructorsAnalyzer>;

namespace TestHelper.UI.Analyzers.Tests
{
    public class PaginatorWithMultiplePublicConstructorsAnalyzerTest
    {
        [Theory]
        [InlineData("NoArgs", 12, 18, "NoArgsPaginator")]
        [InlineData("NullArgs", 12, 18, "NullArgsPaginator")]
        [InlineData("DefaultArgs", 12, 18, "DefaultArgsPaginator")]
        [InlineData("PartialWithConstructorsInEachPart", 12, 18, "PartialWithConstructorsInEachPartPaginator")]
        public async Task MultiplePublicConstructorsWithoutArgs_ReportsAtRegister(string caseName, int line, int column, string typeName)
        {
            var expected = Verifier.Diagnostic().WithLocation(line, column).WithArguments(typeName);
            await Verifier.VerifyAsync($"TestHelperUI4009/{caseName}.cs", expected);
        }

        [Theory]
        [InlineData("WithArgs")]
        [InlineData("ArgsArrayVariable")]
        [InlineData("SinglePublicConstructor")]
        [InlineData("PublicAndNonPublicConstructors")]
        [InlineData("StaticAndPublicConstructors")]
        [InlineData("NoPublicConstructor")]
        public async Task ExplicitArgsOrSingleConstructor_ReportsNothing(string caseName)
        {
            await Verifier.VerifyAsync($"TestHelperUI4009/{caseName}.cs");
        }
    }
}
