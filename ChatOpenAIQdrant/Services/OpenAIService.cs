

using OpenAI.Chat;
using OpenAI.Embeddings;
using OpenAI;
using Microsoft.Extensions.Configuration;
using ChatOpenAIQdrant.Interfaces;

namespace ChatOpenAIQdrant.Services;

public class OpenAIService : IOpenAIService
{
    private readonly OpenAIClient _client;
    private readonly EmbeddingClient _embeddingsClient;
    private readonly ChatClient _chatClient;
    private readonly string _embeddingModel = "text-embedding-ada-002";
    private readonly string _completionModel = "gpt-4o-mini";
    private readonly IQdrantService _qdrantService;

    public OpenAIService(IConfiguration configuration, IQdrantService qdrantService)
    {
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY", EnvironmentVariableTarget.Machine);
        _client = new OpenAIClient(apiKey);
        _embeddingsClient = _client.GetEmbeddingClient(_embeddingModel);
        _chatClient = _client.GetChatClient(_completionModel);
        _qdrantService = qdrantService;
    }

    public async Task<ReadOnlyMemory<float>> GenerateEmbeddingsAsync(string input)
    {
        OpenAIEmbedding embedding = await _embeddingsClient.GenerateEmbeddingAsync(input);
        ReadOnlyMemory<float> vector = embedding.ToFloats();
        return vector;
    }

    public async Task<string> GenerateCompletionAsync(string prompt)
    {
        var completion = await _chatClient.CompleteChatAsync(prompt);
        return completion.Value.Content[0].Text;
    }


    //public async Task<string> GenerateChatCompletionAsync(string prompt)
    //{
    //    var completion = await _chatClient.CompleteChatAsync(prompt);
    //    return completion.Value.Content[0].Text;
    //}
}
