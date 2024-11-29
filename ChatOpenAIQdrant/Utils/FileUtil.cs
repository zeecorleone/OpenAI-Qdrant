
namespace ChatOpenAIQdrant.Utils;

public class FileUtil
{
    private readonly string _directoryPath;

    public FileUtil(string directoryPath)
    {
        _directoryPath = directoryPath;
    }

    public IEnumerable<string> GetFilePaths(string searchPattern = "*.txt")
    {
        if (!Directory.Exists(_directoryPath))
        {
            throw new DirectoryNotFoundException($"Directory not found: {_directoryPath}");
        }

        return Directory.GetFiles(_directoryPath, searchPattern);
    }

    public async Task<string> ReadFileContentAsync(string filePath)
    {
        return await File.ReadAllTextAsync(filePath);
    }
}
