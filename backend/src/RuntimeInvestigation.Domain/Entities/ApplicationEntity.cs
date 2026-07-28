namespace RuntimeInvestigation.Domain.Entities;

public class ApplicationEntity
{
    protected ApplicationEntity()
    {
    }

    public ApplicationEntity(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Application name is required.", nameof(name));
        }

        Id = Guid.NewGuid().ToString("N");
        Name = name.Trim();
        Description = description?.Trim();
        CreatedAt = DateTime.UtcNow;
    }

    public string Id { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    
    public void Update(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Application name is required.", nameof(name));
        }

        Name = name.Trim();
        Description = description?.Trim();
    }
}
