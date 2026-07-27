namespace RuntimeInvestigation.Domain.Entities;

public class ApplicationEntity
{
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

    public string Id { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
}
