using System.Threading.Tasks;
using Xunit;
using Verifier =
    TestHelper.UI.Analyzers.Tests.TestDataVerifier<TestHelper.UI.Analyzers.PaginatorWithoutGenericInterfaceAnalyzer>;

namespace TestHelper.UI.Analyzers.Tests
{
    public class PaginatorWithoutGenericInterfaceAnalyzerTest
    {
        [Theory]
        [InlineData("ImplementsPaginatorDirectly", 8, 18)]
        [InlineData("InheritsBaseImplementingPaginatorOnly", 21, 18)]
        [InlineData("ImplementsInterfaceInheritingPaginatorOnly", 12, 18)]
        [InlineData("PartialPaginator", 8, 26)]
        public async Task PaginatorWithoutGenericInterface_ReportsOnceAtClass(string caseName, int line, int column)
        {
            var expected = Verifier.Diagnostic().WithLocation(line, column).WithArguments(caseName);
            await Verifier.VerifyAsync($"TestHelperUI4010/{caseName}.cs", expected);
        }

        [Theory]
        [InlineData("ImplementsGenericPaginator")]
        [InlineData("InheritsBaseImplementingGenericPaginator")]
        [InlineData("ImplementsInterfaceInheritingGenericPaginator")]
        [InlineData("GenericClassWithOwnTypeParameter")]
        [InlineData("AbstractPaginator")]
        [InlineData("NotPaginator")]
        public async Task GenericInterfaceImplementedOrOutOfScope_ReportsNothing(string caseName)
        {
            await Verifier.VerifyAsync($"TestHelperUI4010/{caseName}.cs");
        }
    }
}
