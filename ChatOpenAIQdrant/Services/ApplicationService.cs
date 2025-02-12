

using ChatOpenAIQdrant.Interfaces;
using ChatOpenAIQdrant.Utils;
using OpenAI.Chat;

namespace ChatOpenAIQdrant.Services;

public class ApplicationService
{
    private readonly IQdrantService _qdrantService;
    private readonly IOpenAIService _openAIService;
    private readonly FileUtil _fileUtil;
    private readonly List<ChatMessage> _chatHistory;
    private readonly string _collectionName = "documents";

    public ApplicationService(
        IQdrantService qdrantService,
        IOpenAIService openAIService,
        FileUtil fileUtil)
    {
        _qdrantService = qdrantService;
        _openAIService = openAIService;
        _fileUtil = fileUtil;
        _chatHistory = new();
    }

    public async Task ProcessFilesAsync()
    {
        var filePaths = _fileUtil.GetFilePaths();

        ulong id = 1;
        foreach (var filePath in filePaths)
        {
            Console.WriteLine($"Processing file: {filePath}");

            var content = await _fileUtil.ReadFileContentAsync(filePath);
            var embeddings = await _openAIService.GenerateEmbeddingsAsync(content);

            await _qdrantService.AddDocumentAsync(embeddings, _collectionName, id, content);

            Console.WriteLine($"File processed and saved to Qdrant: {filePath}");
            id++;
        }
    }

    public async Task<string> AskQuestionAsync(string question)
    {
        if(_chatHistory.Count == 0)
            _chatHistory.Add(ChatMessage.CreateSystemMessage("You are a helpful assistant. The user will ask questions about the content of their files. Relevant context will be provided at the start of each question, enclosed between <i> and </i> tags (e.g., <i>context</i>). Your responses should be accurate, concise, and to the point, covering all key information. Avoid unnecessary elaboration, but ensure the response is complete. If you need clarification, feel free to ask the user."));
       
        var questionEmbeddings = await _openAIService.GenerateEmbeddingsAsync(question);
        var context = await _qdrantService.SearchClosestDocumentAsync(_collectionName, questionEmbeddings);
        if(string.IsNullOrWhiteSpace(context))
        {
            var noContentFound = "I couldn't find relevant content in the provided files. Could you try rephrasing or providing more details?";
            _chatHistory.Add(ChatMessage.CreateAssistantMessage(noContentFound));
            return noContentFound;
        }

        var prompt = $"<i>{context.Substring(0, Math.Min(1000, context.Length))}</i>\n\nQuestion: {question}\n\nAnswer:";
        _chatHistory.Add(ChatMessage.CreateUserMessage(prompt));
        var response = await _openAIService.GenerateCompletionAsync(prompt);
        _chatHistory.Add(ChatMessage.CreateAssistantMessage(response));
        return response;
    }
}
