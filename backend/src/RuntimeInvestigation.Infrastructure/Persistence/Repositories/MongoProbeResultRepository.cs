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

public sealed class MongoProbeResultRepository : IProbeResultRepository
{
    private readonly IMongoCollection<ProbeResult> _collection;

    static MongoProbeResultRepository()
    {
        if (!BsonClassMap.IsClassMapRegistered(typeof(ProbeResult)))
        {
            BsonClassMap.RegisterClassMap<ProbeResult>(cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(r => r.Id).SetSerializer(new StringSerializer(BsonType.String));
                cm.SetIgnoreExtraElements(true);
            });
        }
    }

    public MongoProbeResultRepository(MongoDbSettings settings)
    {
        var client = new MongoClient(settings.ConnectionString);
        var database = client.GetDatabase(settings.DatabaseName);
        _collection = database.GetCollection<ProbeResult>(settings.EvidenceCollectionName ?? "evidence");
    }

    public async Task AddAsync(ProbeResult result, CancellationToken cancellationToken = default)
    {
        await _collection.InsertOneAsync(result, null, cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<ProbeResult> results, CancellationToken cancellationToken = default)
    {
        var list = results as IList<ProbeResult> ?? new List<ProbeResult>(results);
        if (list.Count > 0)
        {
            await _collection.InsertManyAsync(list, cancellationToken: cancellationToken);
        }
    }

    public async Task<IReadOnlyList<ProbeResult>> GetByProbeIdAsync(string probeId, int limit = 100, CancellationToken cancellationToken = default)
    {
        var filter = Builders<ProbeResult>.Filter.Eq(r => r.ProbeId, probeId);
        return await _collection.Find(filter)
            .SortByDescending(r => r.CapturedAtUtc)
            .Limit(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<long> DeleteOlderThanAsync(DateTime thresholdUtc, CancellationToken cancellationToken = default)
    {
        var filter = Builders<ProbeResult>.Filter.Lt(r => r.CapturedAtUtc, thresholdUtc);
        var res = await _collection.DeleteManyAsync(filter, cancellationToken);
        return res.DeletedCount;
    }
}
