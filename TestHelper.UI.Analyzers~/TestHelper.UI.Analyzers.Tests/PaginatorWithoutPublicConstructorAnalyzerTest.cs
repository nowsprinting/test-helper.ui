using System.Threading.Tasks;
using Xunit;
using Verifier =
    TestHelper.UI.Analyzers.Tests.TestDataVerifier<TestHelper.UI.Analyzers.PaginatorWithoutPublicConstructorAnalyzer>;

namespace TestHelper.UI.Analyzers.Tests
{
    public class PaginatorWithoutPublicConstructorAnalyzerTest
    {
        [Theory]
        [InlineData("ImplementsPaginatorOfComponentWithPrivateConstructor", 8, 18,
            "ImplementsPaginatorOfComponentWithPrivateConstructor")]
        [InlineData("ImplementsPaginatorOfComponentWithProtectedConstructor", 8, 18,
            "ImplementsPaginatorOfComponentWithProtectedConstructor")]
        [InlineData("ImplementsPaginatorOfComponentWithInternalConstructor", 8, 18,
            "ImplementsPaginatorOfComponentWithInternalConstructor")]
        [InlineData("ImplementsPaginatorOfComponentWithPrivateProtectedConstructor", 8, 18,
            "ImplementsPaginatorOfComponentWithPrivateProtectedConstructor")]
        [InlineData("ImplementsPaginatorOfComponentWithProtectedInternalConstructor", 8, 18,
            "ImplementsPaginatorOfComponentWithProtectedInternalConstructor")]
        [InlineData("ImplementsPaginatorDirectlyWithPrivateConstructor", 8, 18,
            "ImplementsPaginatorDirectlyWithPrivateConstructor")]
        [InlineData("InheritsPaginatorBaseWithPrivateConstructor", 23, 18,
            "InheritsPaginatorBaseWithPrivateConstructor")]
        [InlineData("GenericPaginatorWithPrivateConstructor", 8, 18, "GenericPaginatorWithPrivateConstructor")]
        [InlineData("PartialPaginatorWithPrivateConstructor", 8, 26, "PartialPaginatorWithPrivateConstructor")]
        [InlineData("NestedPaginatorWithPrivateConstructor", 10, 22, "Nested")]
        [InlineData("StaticAndPrivateConstructors", 8, 18, "StaticAndPrivateConstructors")]
        public async Task PaginatorWithoutPublicConstructor_ReportsOnceAtClassIdentifier(
            string caseName, int line, int column, string className)
        {
            var expected = Verifier.Diagnostic().WithLocation(line, column).WithArguments(className);
            await Verifier.VerifyAsync($"TestHelperUI4006/{caseName}.cs", expected);
        }

        [Theory]
        [InlineData("PublicConstructor")]
        [InlineData("PublicAndPrivateConstructors")]
        [InlineData("NoExplicitConstructor")]
        [InlineData("StaticConstructorOnly")]
        [InlineData("AbstractPaginatorWithProtectedConstructor")]
        [InlineData("NotPaginatorWithPrivateConstructor")]
        public async Task ClassRentableOrOutOfScope_ReportsNothing(string caseName)
        {
            await Verifier.VerifyAsync($"TestHelperUI4006/{caseName}.cs");
        }
    }
}
