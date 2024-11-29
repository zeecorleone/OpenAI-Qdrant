
using OpenAI.Chat;
using OpenAI.Embeddings;
using OpenAI;

namespace ChatOpenAIQdrant.Interfaces;

public interface IOpenAIService
{
    Task<string> GenerateCompletionAsync(string prompt);
    Task<ReadOnlyMemory<float>> GenerateEmbeddingsAsync(string input);
}
