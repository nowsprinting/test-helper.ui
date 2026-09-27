using System.Threading.Tasks;
using Xunit;
using Verifier =
    TestHelper.UI.Analyzers.Tests.TestDataVerifier<
        TestHelper.UI.Analyzers.PaginatorConstructorNotPreservedAnalyzer>;

namespace TestHelper.UI.Analyzers.Tests
{
    public class PaginatorConstructorNotPreservedAnalyzerTest
    {
        [Theory]
        [InlineData("NotPreservedConstructor", 10, 16)]
        [InlineData("ImplementsPaginatorDirectly", 10, 16)]
        [InlineData("InheritsPaginatorBase", 23, 16)]
        [InlineData("PreserveOnTypeWithParameterizedConstructor", 12, 16)]
        public async Task NotPreservedConstructor_ReportsAtConstructor(string caseName, int line, int column)
        {
            var expected = Verifier.Diagnostic().WithLocation(line, column).WithArguments(caseName);
            await Verifier.VerifyAsync($"TestHelperUI4008/{caseName}.cs", expected);
        }

        [Theory]
        [InlineData("NoExplicitConstructor", 8, 18)]
        [InlineData("PreserveOnBaseClassOnly", 23, 18)]
        [InlineData("PartialNoExplicitConstructor", 8, 26)]
        public async Task ImplicitConstructorNotPreserved_ReportsAtClass(string caseName, int line, int column)
        {
            var expected = Verifier.Diagnostic().WithLocation(line, column).WithArguments(caseName);
            await Verifier.VerifyAsync($"TestHelperUI4008/{caseName}.cs", expected);
        }

        [Fact]
        public async Task MultiplePublicConstructors_ReportsEachNotPreservedConstructor()
        {
            await Verifier.VerifyAsync("TestHelperUI4008/MultiplePublicConstructors.cs",
                Verifier.Diagnostic().WithLocation(16, 16).WithArguments("MultiplePublicConstructors"),
                Verifier.Diagnostic().WithLocation(20, 16).WithArguments("MultiplePublicConstructors"));
        }

        [Theory]
        [InlineData("PreserveOnConstructor")]
        [InlineData("PreserveOnTypeWithNoExplicitConstructor")]
        [InlineData("PreserveOnTypeWithParameterlessConstructor")]
        [InlineData("CustomPreserveAttribute")]
        [InlineData("DerivedFromPreserveAttribute")]
        [InlineData("PublicAndPrivateConstructors")]
        [InlineData("AbstractPaginator")]
        [InlineData("NotPaginator")]
        public async Task PreservedOrOutOfScope_ReportsNothing(string caseName)
        {
            await Verifier.VerifyAsync($"TestHelperUI4008/{caseName}.cs");
        }
    }
}
