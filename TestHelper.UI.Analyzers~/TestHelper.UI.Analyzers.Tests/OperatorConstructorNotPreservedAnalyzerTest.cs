using System.Threading.Tasks;
using Xunit;
using Verifier =
    TestHelper.UI.Analyzers.Tests.TestDataVerifier<
        TestHelper.UI.Analyzers.OperatorConstructorNotPreservedAnalyzer>;

namespace TestHelper.UI.Analyzers.Tests
{
    public class OperatorConstructorNotPreservedAnalyzerTest
    {
        [Theory]
        [InlineData("NotPreservedConstructor", 12, 16)]
        [InlineData("ImplementsOperatorDirectly", 12, 16)]
        [InlineData("InheritsOperatorBase", 24, 16)]
        [InlineData("PreserveOnTypeWithParameterizedConstructor", 14, 16)]
        public async Task NotPreservedConstructor_ReportsAtConstructor(string caseName, int line, int column)
        {
            var expected = Verifier.Diagnostic().WithLocation(line, column).WithArguments(caseName);
            await Verifier.VerifyAsync($"TestHelperUI4003/{caseName}.cs", expected);
        }

        [Theory]
        [InlineData("NoExplicitConstructor", 10, 18)]
        [InlineData("PreserveOnBaseClassOnly", 24, 18)]
        [InlineData("PartialNoExplicitConstructor", 10, 26)]
        public async Task ImplicitConstructorNotPreserved_ReportsAtClass(string caseName, int line, int column)
        {
            var expected = Verifier.Diagnostic().WithLocation(line, column).WithArguments(caseName);
            await Verifier.VerifyAsync($"TestHelperUI4003/{caseName}.cs", expected);
        }

        [Fact]
        public async Task MultiplePublicConstructors_ReportsEachNotPreservedConstructor()
        {
            await Verifier.VerifyAsync("TestHelperUI4003/MultiplePublicConstructors.cs",
                Verifier.Diagnostic().WithLocation(18, 16).WithArguments("MultiplePublicConstructors"),
                Verifier.Diagnostic().WithLocation(22, 16).WithArguments("MultiplePublicConstructors"));
        }

        [Theory]
        [InlineData("PreserveOnConstructor")]
        [InlineData("PreserveOnTypeWithNoExplicitConstructor")]
        [InlineData("PreserveOnTypeWithParameterlessConstructor")]
        [InlineData("CustomPreserveAttribute")]
        [InlineData("DerivedFromPreserveAttribute")]
        [InlineData("PublicAndPrivateConstructors")]
        [InlineData("AbstractOperator")]
        [InlineData("NotOperator")]
        public async Task PreservedOrOutOfScope_ReportsNothing(string caseName)
        {
            await Verifier.VerifyAsync($"TestHelperUI4003/{caseName}.cs");
        }
    }
}
