

using ChatOpenAIQdrant.Interfaces;
using ChatOpenAIQdrant.Utils;

namespace ChatOpenAIQdrant.Services;

public class ApplicationService
{
    private readonly IQdrantService _qdrantService;
    private readonly IOpenAIService _openAIService;
    private readonly FileUtil _fileUtil;
    private readonly string _collectionName = "documents";

    public ApplicationService(
        IQdrantService qdrantService,
        IOpenAIService openAIService,
        FileUtil fileUtil)
    {
        _qdrantService = qdrantService;
        _openAIService = openAIService;
        _fileUtil = fileUtil;
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
        var questionEmbeddings = await _openAIService.GenerateEmbeddingsAsync(question);
        var context = await _qdrantService.SearchClosestDocumentAsync(_collectionName, questionEmbeddings);
        if(string.IsNullOrWhiteSpace(context))
        {
            return "No relevant conent found";
        }

        var prompt = $"Info: {context}\n\nQuestion: {question}\n\nAnswer:";
        return await _openAIService.GenerateCompletionAsync(prompt);
    }
}
