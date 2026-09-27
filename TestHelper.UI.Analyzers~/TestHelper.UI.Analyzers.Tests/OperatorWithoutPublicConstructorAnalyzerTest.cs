using System.Threading.Tasks;
using Xunit;
using Verifier =
    TestHelper.UI.Analyzers.Tests.TestDataVerifier<TestHelper.UI.Analyzers.OperatorWithoutPublicConstructorAnalyzer>;

namespace TestHelper.UI.Analyzers.Tests
{
    public class OperatorWithoutPublicConstructorAnalyzerTest
    {
        [Theory]
        [InlineData("ImplementsClickOperatorWithPrivateConstructor", 10, 18,
            "ImplementsClickOperatorWithPrivateConstructor")]
        [InlineData("ImplementsClickOperatorWithProtectedConstructor", 10, 18,
            "ImplementsClickOperatorWithProtectedConstructor")]
        [InlineData("ImplementsClickOperatorWithInternalConstructor", 10, 18,
            "ImplementsClickOperatorWithInternalConstructor")]
        [InlineData("ImplementsClickOperatorWithPrivateProtectedConstructor", 10, 18,
            "ImplementsClickOperatorWithPrivateProtectedConstructor")]
        [InlineData("ImplementsClickOperatorWithProtectedInternalConstructor", 10, 18,
            "ImplementsClickOperatorWithProtectedInternalConstructor")]
        [InlineData("ImplementsOperatorDirectlyWithPrivateConstructor", 10, 18,
            "ImplementsOperatorDirectlyWithPrivateConstructor")]
        [InlineData("InheritsOperatorBaseWithPrivateConstructor", 24, 18, "InheritsOperatorBaseWithPrivateConstructor")]
        [InlineData("GenericOperatorWithPrivateConstructor", 10, 18, "GenericOperatorWithPrivateConstructor")]
        [InlineData("PartialOperatorWithPrivateConstructor", 10, 26, "PartialOperatorWithPrivateConstructor")]
        [InlineData("NestedOperatorWithPrivateConstructor", 12, 22, "Nested")]
        [InlineData("StaticAndPrivateConstructors", 10, 18, "StaticAndPrivateConstructors")]
        public async Task OperatorWithoutPublicConstructor_ReportsOnceAtClassIdentifier(
            string caseName, int line, int column, string className)
        {
            var expected = Verifier.Diagnostic().WithLocation(line, column).WithArguments(className);
            await Verifier.VerifyAsync($"TestHelperUI4001/{caseName}.cs", expected);
        }

        [Theory]
        [InlineData("PublicConstructor")]
        [InlineData("PublicAndPrivateConstructors")]
        [InlineData("NoExplicitConstructor")]
        [InlineData("StaticConstructorOnly")]
        [InlineData("AbstractOperatorWithProtectedConstructor")]
        [InlineData("NotOperatorWithPrivateConstructor")]
        public async Task ClassRentableOrOutOfScope_ReportsNothing(string caseName)
        {
            await Verifier.VerifyAsync($"TestHelperUI4001/{caseName}.cs");
        }
    }
}
