using RuntimeInvestigation.Shared.Extensions;
using RuntimeInvestigation.Shared.Results;
using RuntimeInvestigation.Shared.Validation;
namespace Domain.Tests;
public class SharedFoundationTests
{
    [Fact]
    public void Result_RepresentsSuccessAndFailure()
    {
        Assert.True(Result<int>.Success(42).IsSuccess);
        Assert.Equal("invalid", Result.Failure(new Error("validation", "invalid")).Error!.Message);
    }
    [Fact] public void Guard_RejectsMissingValues() => Assert.Throws<RuntimeInvestigation.Shared.Exceptions.ValidationException>(() => Guard.Required(" ", "name"));
    [Fact] public void NullIfWhiteSpace_NormalizesStrings() { Assert.Null("  ".NullIfWhiteSpace()); Assert.Equal("value", " value ".NullIfWhiteSpace()); }
}
