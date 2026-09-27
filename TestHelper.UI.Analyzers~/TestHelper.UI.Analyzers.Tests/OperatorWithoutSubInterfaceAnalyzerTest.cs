using System.Threading.Tasks;
using Xunit;
using Verifier =
    TestHelper.UI.Analyzers.Tests.TestDataVerifier<
        TestHelper.UI.Analyzers.OperatorWithoutSubInterfaceAnalyzer>;

namespace TestHelper.UI.Analyzers.Tests
{
    public class OperatorWithoutSubInterfaceAnalyzerTest
    {
        [Theory]
        [InlineData("ImplementsOperatorOnly", 10, 18)]
        [InlineData("ImplementsOperatorAndUnrelatedInterface", 14, 18)]
        [InlineData("InheritsBaseImplementingOperatorOnly", 22, 18)]
        [InlineData("PartialImplementsOperatorOnly", 10, 26)]
        public async Task NoSubInterface_ReportsOnceAtClass(string caseName, int line, int column)
        {
            var expected = Verifier.Diagnostic().WithLocation(line, column).WithArguments(caseName);
            await Verifier.VerifyAsync($"TestHelperUI4005/{caseName}.cs", expected);
        }

        [Theory]
        [InlineData("ImplementsClickOperator")]
        [InlineData("ImplementsCustomSubInterface")]
        [InlineData("ImplementsIndirectSubInterface")]
        [InlineData("ImplementsGenericSubInterface")]
        [InlineData("BaseClassImplementsSubInterface")]
        [InlineData("AbstractOperator")]
        [InlineData("NotOperator")]
        public async Task SubInterfaceOrOutOfScope_ReportsNothing(string caseName)
        {
            await Verifier.VerifyAsync($"TestHelperUI4005/{caseName}.cs");
        }
    }
}
