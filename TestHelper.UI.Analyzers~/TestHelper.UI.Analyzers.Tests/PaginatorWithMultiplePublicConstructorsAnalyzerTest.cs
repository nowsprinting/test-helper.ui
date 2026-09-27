using System.Threading.Tasks;
using Xunit;
using Verifier =
    TestHelper.UI.Analyzers.Tests.TestDataVerifier<
        TestHelper.UI.Analyzers.PaginatorWithMultiplePublicConstructorsAnalyzer>;

namespace TestHelper.UI.Analyzers.Tests
{
    public class PaginatorWithMultiplePublicConstructorsAnalyzerTest
    {
        [Theory]
        [InlineData("MultiplePublicConstructors", 8, 18)]
        [InlineData("ImplementsPaginatorDirectly", 8, 18)]
        [InlineData("InheritsPaginatorBase", 21, 18)]
        [InlineData("PartialWithConstructorsInEachPart", 8, 26)]
        public async Task MultiplePublicConstructors_ReportsOnceAtClass(string caseName, int line, int column)
        {
            var expected = Verifier.Diagnostic().WithLocation(line, column).WithArguments(caseName);
            await Verifier.VerifyAsync($"TestHelperUI4009/{caseName}.cs", expected);
        }

        [Theory]
        [InlineData("SinglePublicConstructor")]
        [InlineData("PublicAndNonPublicConstructors")]
        [InlineData("BaseClassHasMultiplePublicConstructors")]
        [InlineData("NoPublicConstructor")]
        [InlineData("AbstractPaginator")]
        [InlineData("NotPaginator")]
        public async Task SinglePublicConstructorOrOutOfScope_ReportsNothing(string caseName)
        {
            await Verifier.VerifyAsync($"TestHelperUI4009/{caseName}.cs");
        }
    }
}
