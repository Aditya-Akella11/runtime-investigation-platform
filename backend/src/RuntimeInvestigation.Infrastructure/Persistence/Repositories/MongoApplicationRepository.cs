using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using RuntimeInvestigation.Application.Features.Applications;
using RuntimeInvestigation.Domain.Entities;

namespace RuntimeInvestigation.Infrastructure.Persistence.Repositories;

public class MongoApplicationRepository : IApplicationRepository
{
    private readonly IMongoCollection<ApplicationEntity> _applications;

    static MongoApplicationRepository()
    {
        if (!BsonClassMap.IsClassMapRegistered(typeof(ApplicationEntity)))
        {
            BsonClassMap.RegisterClassMap<ApplicationEntity>(cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(a => a.Id)
                    .SetSerializer(new StringSerializer(BsonType.String));
                cm.SetIgnoreExtraElements(true);
            });
        }
    }

    public MongoApplicationRepository(MongoDbSettings settings)
    {
        var client = new MongoClient(settings.ConnectionString);
        var database = client.GetDatabase(settings.DatabaseName);
        _applications = database.GetCollection<ApplicationEntity>(settings.ApplicationsCollectionName);
    }

    public async Task<IReadOnlyList<ApplicationEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var results = await _applications.Find(Builders<ApplicationEntity>.Filter.Empty).ToListAsync(cancellationToken);
        return results;
    }

    public async Task<ApplicationEntity?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<ApplicationEntity>.Filter.Eq(a => a.Id, id);
        return await _applications.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ApplicationEntity> AddAsync(ApplicationEntity application, CancellationToken cancellationToken = default)
    {
        await _applications.InsertOneAsync(application, null, cancellationToken);
        return application;
    }

    public async Task UpdateAsync(ApplicationEntity application, CancellationToken cancellationToken = default)
    {
        var filter = Builders<ApplicationEntity>.Filter.Eq(a => a.Id, application.Id);
        var result = await _applications.ReplaceOneAsync(filter, application, new ReplaceOptions { IsUpsert = false }, cancellationToken);
        if (result.MatchedCount == 0)
        {
            throw new KeyNotFoundException($"Application {application.Id} was not found.");
        }
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<ApplicationEntity>.Filter.Eq(a => a.Id, id);
        await _applications.DeleteOneAsync(filter, cancellationToken);
    }
}
