using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using RuntimeInvestigation.Application.Features.Probes;
using RuntimeInvestigation.Domain.Entities;

namespace RuntimeInvestigation.Infrastructure.Persistence.Repositories;

public sealed class MongoProbeRepository : IProbeRepository
{
    private readonly IMongoCollection<RuntimeProbe> _collection;

    static MongoProbeRepository()
    {
        if (!BsonClassMap.IsClassMapRegistered(typeof(RuntimeProbe)))
        {
            BsonClassMap.RegisterClassMap<RuntimeProbe>(cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(p => p.Id).SetSerializer(new StringSerializer(BsonType.String));
                cm.SetIgnoreExtraElements(true);
            });
        }
    }

    public MongoProbeRepository(MongoDbSettings settings)
    {
        var client = new MongoClient(settings.ConnectionString);
        var database = client.GetDatabase(settings.DatabaseName);
        _collection = database.GetCollection<RuntimeProbe>(settings.ProbesCollectionName ?? "probes");
    }

    public async Task<RuntimeProbe?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<RuntimeProbe>.Filter.Eq(p => p.Id, id);
        return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RuntimeProbe>> GetByInvestigationIdAsync(string investigationId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<RuntimeProbe>.Filter.Eq(p => p.InvestigationId, investigationId);
        return await _collection.Find(filter).SortByDescending(p => p.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RuntimeProbe>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _collection.Find(Builders<RuntimeProbe>.Filter.Empty).SortByDescending(p => p.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RuntimeProbe>> GetActiveExpiredAsync(DateTime utcNow, CancellationToken cancellationToken = default)
    {
        var filter = Builders<RuntimeProbe>.Filter.And(
            Builders<RuntimeProbe>.Filter.Eq(p => p.Status, ProbeStatus.Active),
            Builders<RuntimeProbe>.Filter.Lte(p => p.ExpiresAt, utcNow));
        return await _collection.Find(filter).ToListAsync(cancellationToken);
    }

    public async Task CreateAsync(RuntimeProbe probe, CancellationToken cancellationToken = default)
    {
        await _collection.InsertOneAsync(probe, null, cancellationToken);
    }

    public async Task UpdateAsync(RuntimeProbe probe, CancellationToken cancellationToken = default)
    {
        var filter = Builders<RuntimeProbe>.Filter.Eq(p => p.Id, probe.Id);
        await _collection.ReplaceOneAsync(filter, probe, new ReplaceOptions { IsUpsert = false }, cancellationToken);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<RuntimeProbe>.Filter.Eq(p => p.Id, id);
        var result = await _collection.DeleteOneAsync(filter, cancellationToken);
        return result.DeletedCount > 0;
    }
}
