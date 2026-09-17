using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using RuntimeInvestigation.Application.Features.Templates;
using RuntimeInvestigation.Domain.Entities;

namespace RuntimeInvestigation.Infrastructure.Persistence.Repositories;

public class MongoTemplateRepository : ITemplateRepository
{
    private readonly IMongoCollection<InvestigationTemplate> _collection;

    static MongoTemplateRepository()
    {
        if (!BsonClassMap.IsClassMapRegistered(typeof(InvestigationTemplate)))
        {
            BsonClassMap.RegisterClassMap<InvestigationTemplate>(cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(t => t.Id).SetSerializer(new StringSerializer(BsonType.String));
                cm.SetIgnoreExtraElements(true);
            });
        }
    }

    public MongoTemplateRepository(MongoDbSettings settings)
    {
        var client = new MongoClient(settings.ConnectionString);
        var database = client.GetDatabase(settings.DatabaseName);
        _collection = database.GetCollection<InvestigationTemplate>(settings.TemplatesCollectionName);
    }

    public async Task<InvestigationTemplate?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<InvestigationTemplate>.Filter.Eq(t => t.Id, id);
        return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<InvestigationTemplate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _collection.Find(_ => true).SortBy(t => t.Name).ToListAsync(cancellationToken);
    }

    public async Task CreateAsync(InvestigationTemplate template, CancellationToken cancellationToken = default)
    {
        await _collection.InsertOneAsync(template, null, cancellationToken);
    }
}
