

using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Embeddings;
using Qdrant.Client;
using Qdrant.Client.Grpc;
//using static Qdrant.Client.Grpc.Qdrant;


string collectionName = "documents";
const int vectorSize = 1536;
const string embeddingModel = "text-embedding-ada-002";
const string completionModel = "gpt-4o-mini";

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var openAiApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY", EnvironmentVariableTarget.Machine);
var qdrantHost = configuration["Qdrant:Host"];

var openAIClient = new OpenAIClient(openAiApiKey);
var embeddingsClient = openAIClient.GetEmbeddingClient(embeddingModel);
var chatCompletionClient = openAIClient.GetChatClient(completionModel);

//var qdrantClient = new QdrantClient(qdrantHost, 6333, false);
var qdrantClient = new QdrantClient(qdrantHost, port: 6334, https: false);
await CreateCollectionIfNotExist();

//load docs
await LoadAndStoreDocumentsAsync();

Console.WriteLine("\nDocuments loaded into Qdrant. Ask a question or type 'stop' to exit:\n\n");

while(true)
{
    Console.WriteLine("\nYour Question: ");
    string question = Console.ReadLine();
    if (question?.Trim().ToLower() == "stop")
        break;

    var ans = await AnswerQuestionAsync(question);
    Console.WriteLine($"Answer:\n{ans}");
}

Console.WriteLine("\n\n--------END------\n\n");
Console.ReadLine();


async Task CreateCollectionIfNotExist()
{
    var coll = await qdrantClient.CollectionExistsAsync(collectionName);
    if(!coll)
        await qdrantClient.CreateCollectionAsync(collectionName, new VectorParams { Size = vectorSize, Distance = Distance.Cosine });
}

async Task LoadAndStoreDocumentsAsync()
{
    int i = 1;
    foreach (var filePath in Directory.GetFiles(@"D:\garbage\qdrant-test", "*.txt"))
    {
        string content = await File.ReadAllTextAsync(filePath);
        var vector = await GenerateEmbeddingsAsync(content);

        var point = new PointStruct()
        {
            Id = (ulong)i,
            Vectors = vector.ToArray(),
            Payload =
            {
                ["content"] = content
            }

        };

        await qdrantClient.UpsertAsync(collectionName, new[] { point });

        i++;
    }
}

async Task<ReadOnlyMemory<float>> GenerateEmbeddingsAsync(string input)
{
    OpenAIEmbedding embedding = await embeddingsClient.GenerateEmbeddingAsync(input);
    ReadOnlyMemory<float> vector = embedding.ToFloats();
    return vector;
}

async Task<string> AnswerQuestionAsync(string question)
{
    var questionEmbedding = await GenerateEmbeddingsAsync(question);

    var searchResult = await qdrantClient.SearchAsync(collectionName,
        questionEmbedding,
        limit: 1);

    if (searchResult.Count == 0)
    {
        return "No relevant content found.";
    }

    var closestContentPayload = searchResult[0].Payload["content"];
    var closestContnt = closestContentPayload.ToString();

    // Generate an answer using OpenAI's completion API

    var prompt = $"Info: {closestContnt}\n\nQuestion: {question}\n\nAnswer:";

    var completionResponse =  chatCompletionClient.CompleteChat(prompt);
    return completionResponse.Value.Content[0].Text;

}