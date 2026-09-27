using System.Threading.Tasks;
using Xunit;
using Verifier =
    TestHelper.UI.Analyzers.Tests.TestDataVerifier<TestHelper.UI.Analyzers.RentAllWithoutListAnalyzer>;

namespace TestHelper.UI.Analyzers.Tests
{
    public class RentAllWithoutListAnalyzerTest
    {
        [Theory]
        [InlineData("OmittedArgument", 7, 13)]
        [InlineData("NullArgument", 9, 13)]
        [InlineData("DefaultArgument", 9, 13)]
        [InlineData("NamedNullArgument", 7, 13)]
        [InlineData("ConditionalAccess", 7, 18)]
        [InlineData("CallThroughSubclass", 7, 13)]
        public async Task RentAllWithoutList_ReportsAtInvocation(string caseName, int line, int column)
        {
            var expected = Verifier.Diagnostic().WithLocation(line, column);
            await Verifier.VerifyAsync($"TestHelperUI3002/{caseName}.cs", expected);
        }

        [Theory]
        [InlineData("ListArgument")]
        [InlineData("NullableVariableArgument")]
        [InlineData("OtherTypeRentAll")]
        public async Task RentAllWithNonConstantListOrOutOfScope_ReportsNothing(string caseName)
        {
            await Verifier.VerifyAsync($"TestHelperUI3002/{caseName}.cs");
        }
    }
}
