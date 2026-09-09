using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using RuntimeInvestigation.Application.Features.Users;
using RuntimeInvestigation.Domain.Entities;

namespace RuntimeInvestigation.Infrastructure.Persistence.Repositories;

public class MongoUserRepository : IUserRepository
{
    private readonly IMongoCollection<TenantUser> _collection;

    static MongoUserRepository()
    {
        if (!BsonClassMap.IsClassMapRegistered(typeof(TenantUser)))
        {
            BsonClassMap.RegisterClassMap<TenantUser>(cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(u => u.Id).SetSerializer(new StringSerializer(BsonType.String));
                cm.SetIgnoreExtraElements(true);
            });
        }
    }

    public MongoUserRepository(MongoDbSettings settings)
    {
        var client = new MongoClient(settings.ConnectionString);
        var database = client.GetDatabase(settings.DatabaseName);
        _collection = database.GetCollection<TenantUser>(settings.UsersCollectionName);

        // Compound unique index on (TenantId, Email)
        var indexKeys = Builders<TenantUser>.IndexKeys.Ascending(u => u.TenantId).Ascending(u => u.Email);
        var indexOptions = new CreateIndexOptions { Unique = true };
        _collection.Indexes.CreateOne(new CreateIndexModel<TenantUser>(indexKeys, indexOptions));
    }

    public async Task<TenantUser?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<TenantUser>.Filter.Eq(u => u.Id, id);
        return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<TenantUser?> GetByEmailAsync(string tenantId, string email, CancellationToken cancellationToken = default)
    {
        var filter = Builders<TenantUser>.Filter.And(
            Builders<TenantUser>.Filter.Eq(u => u.TenantId, tenantId),
            Builders<TenantUser>.Filter.Eq(u => u.Email, email.ToLowerInvariant())
        );
        return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TenantUser>> GetByTenantIdAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<TenantUser>.Filter.Eq(u => u.TenantId, tenantId);
        return await _collection.Find(filter).SortByDescending(u => u.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task CreateAsync(TenantUser user, CancellationToken cancellationToken = default)
    {
        await _collection.InsertOneAsync(user, null, cancellationToken);
    }

    public async Task UpdateRoleAsync(string id, UserRole role, CancellationToken cancellationToken = default)
    {
        var filter = Builders<TenantUser>.Filter.Eq(u => u.Id, id);
        var update = Builders<TenantUser>.Update.Set(u => u.Role, role);
        await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
    }
}
