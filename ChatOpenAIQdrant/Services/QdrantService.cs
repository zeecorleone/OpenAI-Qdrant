

using ChatOpenAIQdrant.Interfaces;
using Microsoft.Extensions.Configuration;
using Qdrant.Client;
using Qdrant.Client.Grpc;
namespace ChatOpenAIQdrant.Services;

public class QdrantService : IQdrantService
{
    private readonly QdrantClient _client;
    private const int VectorSize = 1536;

    public QdrantService(IConfiguration configuration)
    {
        var qdrantHost = configuration["Qdrant:Host"];
        _client = new QdrantClient(qdrantHost!, port: 6334, https: false);
    }

    public async Task CreateCollectionIfNotExistsAsync(string collectionName)
    {
        if (!await _client.CollectionExistsAsync(collectionName))
        {
            await _client.CreateCollectionAsync(collectionName, new VectorParams
            {
                Size = VectorSize,
                Distance = Distance.Cosine
            });
        }
    }

    public async Task AddDocumentAsync(ReadOnlyMemory<float> embedding, string collectionName, ulong id, string content)
    {
        var point = new PointStruct()
        {
            Id = (ulong)id,
            Vectors = embedding.ToArray(),
            Payload =
            {
                ["content"] = content
            }
        };

        await _client.UpsertAsync(collectionName, new[] { point });
    }

    public async Task<string> SearchClosestDocumentAsync(string collectionName, ReadOnlyMemory<float> queryEmbedding)
    {
        var searchResult = await _client.SearchAsync(collectionName, queryEmbedding, limit: 1);
        return searchResult.Count > 0 ? searchResult[0].Payload["content"].ToString() : string.Empty;
    }

}
