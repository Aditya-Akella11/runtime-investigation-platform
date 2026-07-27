namespace RuntimeInvestigation.Shared.Domain;
public abstract class BaseEntity
{
    protected BaseEntity() => Id = Guid.NewGuid().ToString("N");
    public string Id { get; protected set; }
}
