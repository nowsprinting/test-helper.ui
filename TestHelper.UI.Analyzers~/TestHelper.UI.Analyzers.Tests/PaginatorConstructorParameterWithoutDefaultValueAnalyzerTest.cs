using System.Threading.Tasks;
using Xunit;
using Verifier =
    TestHelper.UI.Analyzers.Tests.TestDataVerifier<TestHelper.UI.Analyzers.PaginatorConstructorParameterWithoutDefaultValueAnalyzer>;

namespace TestHelper.UI.Analyzers.Tests
{
    public class PaginatorConstructorParameterWithoutDefaultValueAnalyzerTest
    {
        [Theory]
        [InlineData("RequiredStringParameter", 12, 18, "text", "String")]
        [InlineData("RequiredComponentParameter", 13, 18, "scrollRect", "ScrollRect")]
        [InlineData("InjectableForOperatorButNotForPaginator", 12, 18, "logger", "ILogger")]
        public async Task RequiredParameterWithoutArgs_ReportsAtRegister(string caseName, int line, int column, string parameterName, string typeName)
        {
            var expected = Verifier.Diagnostic().WithLocation(line, column).WithArguments(parameterName, typeName);
            await Verifier.VerifyAsync($"TestHelperUI4007/{caseName}.cs", expected);
        }

        [Theory]
        [InlineData("AllParametersHaveDefaultValues")]
        [InlineData("NoParameters")]
        [InlineData("WithArgs")]
        public async Task ResolvableOrOutOfScope_ReportsNothing(string caseName)
        {
            await Verifier.VerifyAsync($"TestHelperUI4007/{caseName}.cs");
        }
    }
}
