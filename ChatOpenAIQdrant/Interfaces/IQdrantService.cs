
namespace ChatOpenAIQdrant.Interfaces;

public interface IQdrantService
{
    Task CreateCollectionIfNotExistsAsync(string collectionName);
    Task AddDocumentAsync(ReadOnlyMemory<float> embedding, string collectionName, ulong id, string content);
    Task<string> SearchClosestDocumentAsync(string collectionName, ReadOnlyMemory<float> queryEmbedding);
}
