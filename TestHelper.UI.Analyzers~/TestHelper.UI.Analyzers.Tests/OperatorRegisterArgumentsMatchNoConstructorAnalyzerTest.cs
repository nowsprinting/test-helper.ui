using System.Threading.Tasks;
using Xunit;
using Verifier =
    TestHelper.UI.Analyzers.Tests.TestDataVerifier<TestHelper.UI.Analyzers.OperatorRegisterArgumentsMatchNoConstructorAnalyzer>;

namespace TestHelper.UI.Analyzers.Tests
{
    public class OperatorRegisterArgumentsMatchNoConstructorAnalyzerTest
    {
        [Theory]
        [InlineData("TooManyArgs", 14, 18, "TooManyArgsOperator")]
        [InlineData("SealedClassToValueType", 14, 18, "SealedClassToValueTypeOperator")]
        [InlineData("ArrayCreationArgs", 14, 18, "ArrayCreationArgsOperator")]
        [InlineData("StructWithoutImplicitConversion", 14, 18, "StructWithoutImplicitConversionOperator")]
        [InlineData("NullToNonNullableValueType", 14, 18, "NullToNonNullableValueTypeOperator")]
        [InlineData("NoConstructorMatchesAmongMultiple", 14, 18, "NoConstructorMatchesAmongMultipleOperator")]
        public async Task NoConstructorMatches_ReportsAtRegister(string caseName, int line, int column, string typeName)
        {
            var expected = Verifier.Diagnostic().WithLocation(line, column).WithArguments(typeName);
            await Verifier.VerifyAsync($"TestHelperUI4011/{caseName}.cs", expected);
        }

        [Theory]
        [InlineData("ExactMatch")]
        [InlineData("FewerArgsThanParameters")]
        [InlineData("ImplicitNumericConversion")]
        [InlineData("NullToReferenceType")]
        [InlineData("NullToNullableValueType")]
        [InlineData("ValueToNullableValueType")]
        [InlineData("InterfaceTypedArg")]
        [InlineData("NullableValueTypeArg")]
        [InlineData("NonSealedClassArg")]
        [InlineData("ObjectTypedArg")]
        [InlineData("OneOfMultipleConstructorsMatches")]
        [InlineData("ParamsArrayConstructor")]
        [InlineData("EnumArgToIntegralParameter")]
        [InlineData("NoArgs")]
        [InlineData("NullArgs")]
        [InlineData("ArgsArrayVariable")]
        [InlineData("NoPublicConstructor")]
        [InlineData("GenericTypeParameter")]
        [InlineData("OtherPoolType")]
        public async Task PossiblyMatchingOrOutOfScope_ReportsNothing(string caseName)
        {
            await Verifier.VerifyAsync($"TestHelperUI4011/{caseName}.cs");
        }
    }
}
