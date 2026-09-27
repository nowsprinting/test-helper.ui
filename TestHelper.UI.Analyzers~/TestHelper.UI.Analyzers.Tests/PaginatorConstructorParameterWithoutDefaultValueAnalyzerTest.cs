using System.Threading.Tasks;
using Xunit;
using Verifier =
    TestHelper.UI.Analyzers.Tests.TestDataVerifier<
        TestHelper.UI.Analyzers.PaginatorConstructorParameterWithoutDefaultValueAnalyzer>;

namespace TestHelper.UI.Analyzers.Tests
{
    public class PaginatorConstructorParameterWithoutDefaultValueAnalyzerTest
    {
        [Fact]
        public async Task RequiredTargetComponentAndDelay_ReportsEachParameter()
        {
            await Verifier.VerifyAsync("TestHelperUI4007/RequiredTargetComponentAndDelay.cs",
                Verifier.Diagnostic().WithLocation(10, 62)
                    .WithArguments("carousel", "RequiredTargetComponentAndDelay"),
                Verifier.Diagnostic().WithLocation(10, 78)
                    .WithArguments("pageDelaySeconds", "RequiredTargetComponentAndDelay"));
        }

        [Fact]
        public async Task RefAndOutParameters_ReportsEachParameter()
        {
            await Verifier.VerifyAsync("TestHelperUI4007/RefAndOutParameters.cs",
                Verifier.Diagnostic().WithLocation(10, 44).WithArguments("a", "RefAndOutParameters"),
                Verifier.Diagnostic().WithLocation(10, 55).WithArguments("b", "RefAndOutParameters"));
        }

        [Theory]
        [InlineData("ParamsArray", 10, 41, "values")]
        [InlineData("OptionalAttributeWithoutDefaultValue", 11, 68, "value")]
        [InlineData("ImplementsPaginatorDirectly", 10, 48, "value")]
        [InlineData("InheritsPaginatorBase", 23, 42, "value")]
        [InlineData("PublicAndPrivateConstructors", 10, 49, "value")]
        public async Task ParameterWithoutDefaultValue_ReportsAtParameter(
            string caseName, int line, int column, string parameterName)
        {
            var expected = Verifier.Diagnostic().WithLocation(line, column).WithArguments(parameterName, caseName);
            await Verifier.VerifyAsync($"TestHelperUI4007/{caseName}.cs", expected);
        }

        [Theory]
        [InlineData("AllParametersHaveDefaultValues")]
        [InlineData("DefaultValueByAttribute")]
        [InlineData("MultiplePublicConstructors")]
        [InlineData("NoPublicConstructor")]
        [InlineData("AbstractPaginator")]
        [InlineData("NotPaginator")]
        public async Task RentableOrOutOfScope_ReportsNothing(string caseName)
        {
            await Verifier.VerifyAsync($"TestHelperUI4007/{caseName}.cs");
        }
    }
}
