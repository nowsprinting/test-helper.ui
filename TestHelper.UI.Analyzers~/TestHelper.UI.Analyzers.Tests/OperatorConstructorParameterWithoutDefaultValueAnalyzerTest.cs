using System.Threading.Tasks;
using Xunit;
using Verifier =
    TestHelper.UI.Analyzers.Tests.TestDataVerifier<
        TestHelper.UI.Analyzers.OperatorConstructorParameterWithoutDefaultValueAnalyzer>;

namespace TestHelper.UI.Analyzers.Tests
{
    public class OperatorConstructorParameterWithoutDefaultValueAnalyzerTest
    {
        [Fact]
        public async Task RequiredIntAndLogger_ReportsEachParameter()
        {
            await Verifier.VerifyAsync("TestHelperUI4002/RequiredIntAndLogger.cs",
                Verifier.Diagnostic().WithLocation(12, 41).WithArguments("holdMillis", "RequiredIntAndLogger"),
                Verifier.Diagnostic().WithLocation(12, 61).WithArguments("logger", "RequiredIntAndLogger"));
        }

        [Fact]
        public async Task RefAndOutParameters_ReportsEachParameter()
        {
            await Verifier.VerifyAsync("TestHelperUI4002/RefAndOutParameters.cs",
                Verifier.Diagnostic().WithLocation(12, 44).WithArguments("a", "RefAndOutParameters"),
                Verifier.Diagnostic().WithLocation(12, 55).WithArguments("b", "RefAndOutParameters"));
        }

        [Theory]
        [InlineData("RequiredAndDefault", 13, 20, "text")]
        [InlineData("ParamsArray", 12, 41, "values")]
        [InlineData("OptionalAttributeWithoutDefaultValue", 13, 68, "value")]
        [InlineData("ImplementsOperatorDirectly", 12, 47, "value")]
        [InlineData("InheritsOperatorBase", 24, 41, "value")]
        [InlineData("PublicAndPrivateConstructors", 12, 49, "value")]
        public async Task ParameterWithoutDefaultValue_ReportsAtParameter(
            string caseName, int line, int column, string parameterName)
        {
            var expected = Verifier.Diagnostic().WithLocation(line, column).WithArguments(parameterName, caseName);
            await Verifier.VerifyAsync($"TestHelperUI4002/{caseName}.cs", expected);
        }

        [Theory]
        [InlineData("AllParametersHaveDefaultValues")]
        [InlineData("DefaultValueByAttribute")]
        [InlineData("NoExplicitConstructor")]
        [InlineData("MultiplePublicConstructors")]
        [InlineData("NoPublicConstructor")]
        [InlineData("AbstractOperator")]
        [InlineData("NotOperator")]
        public async Task RentableOrOutOfScope_ReportsNothing(string caseName)
        {
            await Verifier.VerifyAsync($"TestHelperUI4002/{caseName}.cs");
        }
    }
}
