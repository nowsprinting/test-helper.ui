using System.Threading.Tasks;
using Xunit;
using Verifier =
    TestHelper.UI.Analyzers.Tests.TestDataVerifier<TestHelper.UI.Analyzers.PaginatorWithoutPublicConstructorAnalyzer>;

namespace TestHelper.UI.Analyzers.Tests
{
    public class PaginatorWithoutPublicConstructorAnalyzerTest
    {
        [Theory]
        [InlineData("NoArgs", 12, 18, "NoArgsPaginator", "has no public constructor")]
        [InlineData("NullArgs", 12, 18, "NullArgsPaginator", "has no public constructor")]
        [InlineData("DefaultArgs", 12, 18, "DefaultArgsPaginator", "has no public constructor")]
        [InlineData("WithArgs", 12, 18, "WithArgsPaginator", "has no public constructor")]
        [InlineData("AbstractType", 12, 18, "AbstractTypePaginator", "is an abstract class")]
        [InlineData("InterfaceTypeArgument", 12, 18, "IPaginator", "is an interface")]
        [InlineData("ArgsArrayVariable", 12, 18, "ArgsArrayVariablePaginator", "has no public constructor")]
        [InlineData("ThroughDerivedPool", 12, 18, "ThroughDerivedPoolPaginator", "has no public constructor")]
        [InlineData("ConditionalAccess", 12, 19, "ConditionalAccessPaginator", "has no public constructor")]
        [InlineData("FluentChain", 12, 62, "FluentChainPaginator", "has no public constructor")]
        public async Task NoPublicConstructor_ReportsAtRegister(string caseName, int line, int column, string typeName, string detail)
        {
            var expected = Verifier.Diagnostic().WithLocation(line, column).WithArguments(typeName, detail);
            await Verifier.VerifyAsync($"TestHelperUI4006/{caseName}.cs", expected);
        }

        [Theory]
        [InlineData("PublicConstructor")]
        [InlineData("ImplicitDefaultConstructor")]
        [InlineData("GenericTypeParameter")]
        [InlineData("RentOnly")]
        [InlineData("OtherTypeRegister")]
        [InlineData("DeclarationOnly")]
        public async Task PublicConstructorOrOutOfScope_ReportsNothing(string caseName)
        {
            await Verifier.VerifyAsync($"TestHelperUI4006/{caseName}.cs");
        }
    }
}
