namespace RuntimeInvestigation.Infrastructure.Persistence.Repositories;

public sealed class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = "RuntimeInvestigation";
    public string ApplicationsCollectionName { get; set; } = "applications";
    public string InvestigationsCollectionName { get; set; } = "investigations";
    public string ProbesCollectionName { get; set; } = "probes";
    public string EvidenceCollectionName { get; set; } = "evidence";
    public string AuditCollectionName { get; set; } = "audit";
    public string UsersCollectionName { get; set; } = "users";
}
