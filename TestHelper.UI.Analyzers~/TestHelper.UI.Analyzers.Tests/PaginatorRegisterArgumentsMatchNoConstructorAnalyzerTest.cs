using System.Threading.Tasks;
using Xunit;
using Verifier =
    TestHelper.UI.Analyzers.Tests.TestDataVerifier<TestHelper.UI.Analyzers.PaginatorRegisterArgumentsMatchNoConstructorAnalyzer>;

namespace TestHelper.UI.Analyzers.Tests
{
    public class PaginatorRegisterArgumentsMatchNoConstructorAnalyzerTest
    {
        [Theory]
        [InlineData("TooManyArgs", 12, 18, "TooManyArgsPaginator")]
        [InlineData("SealedClassToValueType", 12, 18, "SealedClassToValueTypePaginator")]
        [InlineData("StructWithoutImplicitConversion", 12, 18, "StructWithoutImplicitConversionPaginator")]
        [InlineData("NullToNonNullableValueType", 12, 18, "NullToNonNullableValueTypePaginator")]
        [InlineData("NoConstructorMatchesAmongMultiple", 12, 18, "NoConstructorMatchesAmongMultiplePaginator")]
        [InlineData("ComponentParameter", 13, 18, "ComponentParameterPaginator")]
        public async Task NoConstructorMatches_ReportsAtRegister(string caseName, int line, int column, string typeName)
        {
            var expected = Verifier.Diagnostic().WithLocation(line, column).WithArguments(typeName);
            await Verifier.VerifyAsync($"TestHelperUI4012/{caseName}.cs", expected);
        }

        [Theory]
        [InlineData("ExactMatch")]
        [InlineData("FewerArgsThanParameters")]
        [InlineData("ImplicitNumericConversion")]
        [InlineData("NullToReferenceType")]
        [InlineData("NullToNullableValueType")]
        [InlineData("ValueToNullableValueType")]
        [InlineData("InterfaceTypedArg")]
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
            await Verifier.VerifyAsync($"TestHelperUI4012/{caseName}.cs");
        }
    }
}
