using System.Threading.Tasks;
using Xunit;
using Verifier = TestHelper.UI.Analyzers.Tests.TestDataVerifier<TestHelper.UI.Analyzers.OperatorWithoutPublicConstructorAnalyzer>;

namespace TestHelper.UI.Analyzers.Tests
{
    public class OperatorWithoutPublicConstructorAnalyzerTest
    {
        [Theory]
        [InlineData("ImplementsClickOperatorWithPrivateConstructor", 11, 18, "ImplementsClickOperatorWithPrivateConstructor")]
        [InlineData("ImplementsClickOperatorWithProtectedConstructor", 11, 18, "ImplementsClickOperatorWithProtectedConstructor")]
        [InlineData("ImplementsClickOperatorWithInternalConstructor", 11, 18, "ImplementsClickOperatorWithInternalConstructor")]
        [InlineData("ImplementsClickOperatorWithPrivateProtectedConstructor", 11, 18, "ImplementsClickOperatorWithPrivateProtectedConstructor")]
        [InlineData("ImplementsClickOperatorWithProtectedInternalConstructor", 11, 18, "ImplementsClickOperatorWithProtectedInternalConstructor")]
        [InlineData("ImplementsOperatorDirectlyWithPrivateConstructor", 11, 18, "ImplementsOperatorDirectlyWithPrivateConstructor")]
        [InlineData("InheritsOperatorBaseWithPrivateConstructor", 25, 18, "InheritsOperatorBaseWithPrivateConstructor")]
        [InlineData("GenericOperatorWithPrivateConstructor", 11, 18, "GenericOperatorWithPrivateConstructor")]
        [InlineData("PartialOperatorWithPrivateConstructor", 11, 26, "PartialOperatorWithPrivateConstructor")]
        [InlineData("NestedOperatorWithPrivateConstructor", 13, 22, "Nested")]
        [InlineData("StaticAndPrivateConstructors", 11, 18, "StaticAndPrivateConstructors")]
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
