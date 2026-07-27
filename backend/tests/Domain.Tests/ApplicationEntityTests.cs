using RuntimeInvestigation.Domain.Entities;

namespace Domain.Tests;

public class ApplicationEntityTests
{
    [Fact]
    public void Constructor_ShouldCreateApplicationWithGeneratedId()
    {
        var application = new ApplicationEntity("Payments Service", "Handles customer payments");

        Assert.False(string.IsNullOrWhiteSpace(application.Id));
        Assert.Equal("Payments Service", application.Name);
        Assert.Equal("Handles customer payments", application.Description);
        Assert.NotEqual(default, application.CreatedAt);
    }

    [Fact]
    public void Constructor_ShouldThrowWhenNameIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => new ApplicationEntity(string.Empty, "desc"));
    }
}
