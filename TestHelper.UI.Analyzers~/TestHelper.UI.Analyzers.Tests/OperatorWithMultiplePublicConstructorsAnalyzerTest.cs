using System.Threading.Tasks;
using Xunit;
using Verifier =
    TestHelper.UI.Analyzers.Tests.TestDataVerifier<
        TestHelper.UI.Analyzers.OperatorWithMultiplePublicConstructorsAnalyzer>;

namespace TestHelper.UI.Analyzers.Tests
{
    public class OperatorWithMultiplePublicConstructorsAnalyzerTest
    {
        [Theory]
        [InlineData("MultiplePublicConstructors", 10, 18)]
        [InlineData("ImplementsOperatorDirectly", 10, 18)]
        [InlineData("InheritsOperatorBase", 22, 18)]
        [InlineData("PartialWithConstructorsInEachPart", 10, 26)]
        public async Task MultiplePublicConstructors_ReportsOnceAtClass(string caseName, int line, int column)
        {
            var expected = Verifier.Diagnostic().WithLocation(line, column).WithArguments(caseName);
            await Verifier.VerifyAsync($"TestHelperUI4004/{caseName}.cs", expected);
        }

        [Theory]
        [InlineData("SinglePublicConstructor")]
        [InlineData("PublicAndNonPublicConstructors")]
        [InlineData("StaticAndPublicConstructors")]
        [InlineData("BaseClassHasMultiplePublicConstructors")]
        [InlineData("NoExplicitConstructor")]
        [InlineData("NoPublicConstructor")]
        [InlineData("AbstractOperator")]
        [InlineData("NotOperator")]
        public async Task SinglePublicConstructorOrOutOfScope_ReportsNothing(string caseName)
        {
            await Verifier.VerifyAsync($"TestHelperUI4004/{caseName}.cs");
        }
    }
}
