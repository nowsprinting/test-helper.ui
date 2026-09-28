using System.Threading.Tasks;
using Xunit;
using Verifier =
    TestHelper.UI.Analyzers.Tests.TestDataVerifier<TestHelper.UI.Analyzers.OperatorWithoutPublicConstructorAnalyzer>;

namespace TestHelper.UI.Analyzers.Tests
{
    public class OperatorWithoutPublicConstructorAnalyzerTest
    {
        [Theory]
        [InlineData("NoArgs", 14, 18, "NoArgsOperator", "has no public constructor")]
        [InlineData("NullArgs", 14, 18, "NullArgsOperator", "has no public constructor")]
        [InlineData("DefaultArgs", 14, 18, "DefaultArgsOperator", "has no public constructor")]
        [InlineData("WithArgs", 14, 18, "WithArgsOperator", "has no public constructor")]
        [InlineData("AbstractType", 14, 18, "AbstractTypeOperator", "is an abstract class")]
        [InlineData("InterfaceTypeArgument", 14, 18, "IClickOperator", "is an interface")]
        [InlineData("ArgsArrayVariable", 14, 18, "ArgsArrayVariableOperator", "has no public constructor")]
        [InlineData("ThroughDerivedPool", 14, 18, "ThroughDerivedPoolOperator", "has no public constructor")]
        [InlineData("ConditionalAccess", 14, 19, "ConditionalAccessOperator", "has no public constructor")]
        [InlineData("FluentChain", 14, 61, "FluentChainOperator", "has no public constructor")]
        public async Task NoPublicConstructor_ReportsAtRegister(string caseName, int line, int column, string typeName, string detail)
        {
            var expected = Verifier.Diagnostic().WithLocation(line, column).WithArguments(typeName, detail);
            await Verifier.VerifyAsync($"TestHelperUI4001/{caseName}.cs", expected);
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
            await Verifier.VerifyAsync($"TestHelperUI4001/{caseName}.cs");
        }
    }
}
