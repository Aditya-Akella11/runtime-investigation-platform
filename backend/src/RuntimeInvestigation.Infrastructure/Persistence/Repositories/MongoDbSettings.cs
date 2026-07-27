namespace RuntimeInvestigation.Infrastructure.Persistence.Repositories;

public sealed class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = "RuntimeInvestigation";
    public string ApplicationsCollectionName { get; set; } = "applications";
}
