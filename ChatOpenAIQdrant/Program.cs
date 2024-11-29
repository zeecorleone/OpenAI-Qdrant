

using ChatOpenAIQdrant.Interfaces;
using ChatOpenAIQdrant.Services;
using ChatOpenAIQdrant.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var services = new ServiceCollection();

services.AddSingleton<IConfiguration>(configuration);
services.AddSingleton<IQdrantService, QdrantService>();
services.AddSingleton<IOpenAIService, OpenAIService>();
services.AddSingleton<FileUtil>(sp => new(@"D:\garbage\qdrant-test"));
services.AddSingleton<ApplicationService>();

var serviceProvider = services.BuildServiceProvider();

var appService = serviceProvider.GetService<ApplicationService>();

await appService.ProcessFilesAsync();

while (true)
{
    Console.Write("\nYour Question: ");
    var question = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(question) || question.Trim().ToLower() == "stop")
        break;

    var answer = await appService.AskQuestionAsync(question);
    Console.WriteLine($"\nAnswer:\n{answer}");
}

Console.WriteLine("\n\n--------END------\n\n");

