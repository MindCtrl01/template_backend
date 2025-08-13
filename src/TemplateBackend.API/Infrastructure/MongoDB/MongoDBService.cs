using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;

namespace TemplateBackend.API.Infrastructure.MongoDB;

/// <summary>
/// Generic MongoDB service implementation
/// </summary>
/// <typeparam name="T">Document type</typeparam>
public class MongoDBService<T> : IMongoDBService<T> where T : class
{
    private readonly IMongoCollection<T> _collection;
    private readonly ILogger<MongoDBService<T>> _logger;

    public MongoDBService(IConfiguration configuration, IOptions<MongoDBSettings> settings, ILogger<MongoDBService<T>> logger)
    {
        var mongoSettings = settings.Value;
        var connectionString = configuration.GetConnectionString("MongoDB") ?? "mongodb://localhost:27017";
        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(mongoSettings.DatabaseName);
        
        // Get collection name from type name (e.g., EmailDocument -> Emails)
        var collectionName = typeof(T).Name.Replace("Document", "").ToLowerInvariant() + "s";
        _collection = database.GetCollection<T>(collectionName);
        _logger = logger;
    }

    public async Task<T> CreateAsync(T document)
    {
        try
        {
            await _collection.InsertOneAsync(document);
            _logger.LogInformation("Document created successfully in collection {CollectionName}", _collection.CollectionNamespace.CollectionName);
            return document;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create document in collection {CollectionName}", _collection.CollectionNamespace.CollectionName);
            throw;
        }
    }

    public async Task<List<T>> CreateManyAsync(IEnumerable<T> documents)
    {
        try
        {
            var documentsList = documents.ToList();
            if (!documentsList.Any())
            {
                _logger.LogWarning("No documents provided for bulk insert");
                return new List<T>();
            }

            await _collection.InsertManyAsync(documentsList);
            _logger.LogInformation("Bulk insert completed successfully. {Count} documents created in collection {CollectionName}", 
                documentsList.Count, _collection.CollectionNamespace.CollectionName);
            return documentsList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to perform bulk insert in collection {CollectionName}", _collection.CollectionNamespace.CollectionName);
            throw;
        }
    }

    public async Task<T?> GetByIdAsync(string id)
    {
        try
        {
            var filter = Builders<T>.Filter.Eq("_id", ObjectId.Parse(id));
            var document = await _collection.Find(filter).FirstOrDefaultAsync();
            return document;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get document by ID {Id} from collection {CollectionName}", id, _collection.CollectionNamespace.CollectionName);
            return null;
        }
    }

    public async Task<List<T>> GetAllAsync(int page = 1, int pageSize = 10)
    {
        try
        {
            var skip = (page - 1) * pageSize;
            var documents = await _collection.Find(FilterDefinition<T>.Empty)
                .Skip(skip)
                .Limit(pageSize)
                .ToListAsync();

            return documents;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all documents from collection {CollectionName}", _collection.CollectionNamespace.CollectionName);
            return new List<T>();
        }
    }

    public async Task<(List<T> Documents, int TotalCount)> GetAllWithTotalAsync(int page = 1, int pageSize = 10)
    {
        try
        {
            var skip = (page - 1) * pageSize;
            var totalCount = await _collection.CountDocumentsAsync(FilterDefinition<T>.Empty);
            var documents = await _collection.Find(FilterDefinition<T>.Empty)
                .Skip(skip)
                .Limit(pageSize)
                .ToListAsync();

            return (documents, (int)totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all documents from collection {CollectionName}", _collection.CollectionNamespace.CollectionName);
            return (new List<T>(), 0);
        }
    }

    public async Task<T?> UpdateAsync(string id, T document)
    {
        try
        {
            var filter = Builders<T>.Filter.Eq("_id", ObjectId.Parse(id));
            var result = await _collection.ReplaceOneAsync(filter, document);
            
            if (result.ModifiedCount > 0)
            {
                _logger.LogInformation("Document updated successfully. ID: {Id}, Collection: {CollectionName}", id, _collection.CollectionNamespace.CollectionName);
                return document;
            }
            
            _logger.LogWarning("Document not found for update. ID: {Id}, Collection: {CollectionName}", id, _collection.CollectionNamespace.CollectionName);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update document with ID {Id} in collection {CollectionName}", id, _collection.CollectionNamespace.CollectionName);
            return null;
        }
    }

    public async Task<bool> DeleteAsync(string id)
    {
        try
        {
            var filter = Builders<T>.Filter.Eq("_id", ObjectId.Parse(id));
            var result = await _collection.DeleteOneAsync(filter);
            
            if (result.DeletedCount > 0)
            {
                _logger.LogInformation("Document deleted successfully. ID: {Id}, Collection: {CollectionName}", id, _collection.CollectionNamespace.CollectionName);
                return true;
            }
            
            _logger.LogWarning("Document not found for deletion. ID: {Id}, Collection: {CollectionName}", id, _collection.CollectionNamespace.CollectionName);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete document with ID {Id} from collection {CollectionName}", id, _collection.CollectionNamespace.CollectionName);
            return false;
        }
    }

    public async Task<List<T>> FindAsync(Func<T, bool>? filter = null, Func<T, object>? sort = null, int limit = 50)
    {
        try
        {
            var documents = await _collection.Find(FilterDefinition<T>.Empty).ToListAsync();
            
            if (filter != null)
            {
                documents = documents.Where(filter).ToList();
            }
            
            if (sort != null)
            {
                documents = documents.OrderBy(sort).ToList();
            }
            
            return documents.Take(limit).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to find documents in collection {CollectionName}", _collection.CollectionNamespace.CollectionName);
            return new List<T>();
        }
    }

    public async Task<long> CountAsync(Func<T, bool>? filter = null)
    {
        try
        {
            if (filter == null)
            {
                return await _collection.CountDocumentsAsync(FilterDefinition<T>.Empty);
            }
            
            var documents = await _collection.Find(FilterDefinition<T>.Empty).ToListAsync();
            return documents.Count(filter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to count documents in collection {CollectionName}", _collection.CollectionNamespace.CollectionName);
            return 0;
        }
    }

    public async Task<bool> ExistsAsync(string id)
    {
        try
        {
            var filter = Builders<T>.Filter.Eq("_id", ObjectId.Parse(id));
            var count = await _collection.CountDocumentsAsync(filter);
            return count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check existence of document with ID {Id} in collection {CollectionName}", id, _collection.CollectionNamespace.CollectionName);
            return false;
        }
    }
} 