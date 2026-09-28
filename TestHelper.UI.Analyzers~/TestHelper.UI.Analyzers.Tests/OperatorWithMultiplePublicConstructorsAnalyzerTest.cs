using System.Threading.Tasks;
using Xunit;
using Verifier =
    TestHelper.UI.Analyzers.Tests.TestDataVerifier<TestHelper.UI.Analyzers.OperatorWithMultiplePublicConstructorsAnalyzer>;

namespace TestHelper.UI.Analyzers.Tests
{
    public class OperatorWithMultiplePublicConstructorsAnalyzerTest
    {
        [Theory]
        [InlineData("NoArgs", 14, 18, "NoArgsOperator")]
        [InlineData("NullArgs", 14, 18, "NullArgsOperator")]
        [InlineData("DefaultArgs", 14, 18, "DefaultArgsOperator")]
        [InlineData("EmptyArrayArgs", 14, 18, "EmptyArrayArgsOperator")]
        [InlineData("EmptyArrayInitializerArgs", 14, 18, "EmptyArrayInitializerArgsOperator")]
        [InlineData("PartialWithConstructorsInEachPart", 14, 18, "PartialWithConstructorsInEachPartOperator")]
        public async Task MultiplePublicConstructorsWithoutArgs_ReportsAtRegister(string caseName, int line, int column, string typeName)
        {
            var expected = Verifier.Diagnostic().WithLocation(line, column).WithArguments(typeName);
            await Verifier.VerifyAsync($"TestHelperUI4004/{caseName}.cs", expected);
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
            await Verifier.VerifyAsync($"TestHelperUI4004/{caseName}.cs");
        }
    }
}
