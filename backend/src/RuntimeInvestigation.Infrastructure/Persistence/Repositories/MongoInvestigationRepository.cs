using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using RuntimeInvestigation.Application.Features.Investigations;
using RuntimeInvestigation.Domain.Entities;

namespace RuntimeInvestigation.Infrastructure.Persistence.Repositories;

public sealed class MongoInvestigationRepository : IInvestigationRepository
{
    private readonly IMongoCollection<Investigation> _collection;

    static MongoInvestigationRepository()
    {
        if (!BsonClassMap.IsClassMapRegistered(typeof(Investigation)))
        {
            BsonClassMap.RegisterClassMap<Investigation>(cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(i => i.Id)
                    .SetSerializer(new StringSerializer(BsonType.String));
                cm.SetIgnoreExtraElements(true);
            });
        }
    }

    public MongoInvestigationRepository(MongoDbSettings settings)
    {
        var client = new MongoClient(settings.ConnectionString);
        var database = client.GetDatabase(settings.DatabaseName);
        _collection = database.GetCollection<Investigation>(settings.InvestigationsCollectionName);
    }

    public async Task<Investigation?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Investigation>.Filter.Eq(i => i.Id, id);
        return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Investigation>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _collection.Find(Builders<Investigation>.Filter.Empty)
            .SortByDescending(i => i.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Investigation>> GetByApplicationIdAsync(string applicationId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Investigation>.Filter.Eq(i => i.ApplicationId, applicationId);
        return await _collection.Find(filter)
            .SortByDescending(i => i.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task CreateAsync(Investigation investigation, CancellationToken cancellationToken = default)
    {
        await _collection.InsertOneAsync(investigation, null, cancellationToken);
    }

    public async Task UpdateAsync(Investigation investigation, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Investigation>.Filter.Eq(i => i.Id, investigation.Id);
        var result = await _collection.ReplaceOneAsync(filter, investigation, new ReplaceOptions { IsUpsert = false }, cancellationToken);
        if (result.MatchedCount == 0)
        {
            throw new KeyNotFoundException($"Investigation {investigation.Id} was not found.");
        }
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Investigation>.Filter.Eq(i => i.Id, id);
        var result = await _collection.DeleteOneAsync(filter, cancellationToken);
        return result.DeletedCount > 0;
    }
}
