using RuntimeInvestigation.Domain.Entities;
namespace Domain.Tests;
public class InvestigationTests
{
    [Fact]
    public void Investigation_FollowsLifecycle()
    {
        var item = new Investigation("app", "Intermittent failures");
        item.Start(); item.Resolve(); item.Close();
        Assert.Equal(InvestigationStatus.Closed, item.Status);
    }
    [Fact]
    public void Investigation_RejectsInvalidTransition()
    {
        var item = new Investigation("app", "Intermittent failures");
        Assert.Throws<InvalidOperationException>(() => item.Resolve());
    }
}
