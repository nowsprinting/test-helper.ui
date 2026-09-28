using System.Threading.Tasks;
using Xunit;
using Verifier =
    TestHelper.UI.Analyzers.Tests.TestDataVerifier<TestHelper.UI.Analyzers.OperatorConstructorParameterWithoutDefaultValueAnalyzer>;

namespace TestHelper.UI.Analyzers.Tests
{
    public class OperatorConstructorParameterWithoutDefaultValueAnalyzerTest
    {
        [Theory]
        [InlineData("RequiredIntParameter", 14, 18, "value", "Int32")]
        [InlineData("OptionalAttributeWithoutDefaultValue", 15, 18, "value", "Int32")]
        [InlineData("MultipleRequiredParameters", 14, 18, "first", "Int32")]
        [InlineData("InjectedTypeAndRequiredParameter", 14, 18, "value", "Int32")]
        public async Task RequiredParameterWithoutArgs_ReportsAtRegister(string caseName, int line, int column, string parameterName, string typeName)
        {
            var expected = Verifier.Diagnostic().WithLocation(line, column).WithArguments(parameterName, typeName);
            await Verifier.VerifyAsync($"TestHelperUI4002/{caseName}.cs", expected);
        }

        [Theory]
        [InlineData("AllParametersHaveDefaultValues")]
        [InlineData("InjectedTypesWithoutDefaultValue")]
        [InlineData("NoParameters")]
        [InlineData("WithArgs")]
        [InlineData("MultiplePublicConstructors")]
        public async Task ResolvableOrOutOfScope_ReportsNothing(string caseName)
        {
            await Verifier.VerifyAsync($"TestHelperUI4002/{caseName}.cs");
        }
    }
}
