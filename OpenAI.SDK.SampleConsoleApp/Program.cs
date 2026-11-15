using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenAI.SDK.Abstractions;
using OpenAI.SDK.Configuration;
using OpenAI.SDK.Exceptions;
using OpenAI.SDK.Models.Chat;
using System.Text.Json;

namespace OpenAI.SDK.SampleConsoleApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .Build();

            var apiKey = configuration["OpenAI:Api:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            }

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException(
                    "OpenAI API key is not configured. Set it with 'dotnet user-secrets set \"OpenAI:Api:ApiKey\" \"YOUR_KEY\"' " +
                    "or provide the OPENAI_API_KEY environment variable.");
            }

            var services = new ServiceCollection();
            services.Configure<OpenAiOptions>(options =>
            {
                options.ApiKey = apiKey;
                options.BaseUrl = configuration["OpenAI:Api:BaseUrl"] ?? "https://api.openai.com/v1/";
            });
            services.AddOpenAiApi();

            using var serviceProvider = services.BuildServiceProvider();

            try
            {
                var embeddingsApi = serviceProvider.GetRequiredService<IEmbeddingsApi>();
                var embeddingsResult = await embeddingsApi.CreateEmbeddingsAsync(new Models.Embeddings.EmbeddingsRequest
                {
                    Model = "text-embedding-3-small",
                    Input = new[] { "Just a test string" }
                }, CancellationToken.None);

                Console.WriteLine(JsonSerializer.Serialize(embeddingsResult));
                Console.WriteLine("------------------------------------------------------");

                var modelsApi = serviceProvider.GetRequiredService<IModelsApi>();
                var models = await modelsApi.GetModelsAsync(CancellationToken.None);
                Console.WriteLine(JsonSerializer.Serialize(models));
                Console.WriteLine("------------------------------------------------------");

                var chatCompletionsApi = serviceProvider.GetRequiredService<IChatApi>();
                var chatCompletionResult = await chatCompletionsApi.CreateCompletionAsync(new Models.Chat.ApiChatCompletionRequest
                {
                    Model = "gpt-4o-mini",
                    Messages = new List<ChatMessage>
                    {
                        new ChatMessage { Role = "user", Content = "Hello!" }
                    }
                }, CancellationToken.None);
                Console.WriteLine(JsonSerializer.Serialize(chatCompletionResult));
                Console.WriteLine("------------------------------------------------------");

                var imagesApi = serviceProvider.GetRequiredService<IImagesApi>();
                var imageResult = await imagesApi.CreateImageAsync(new Models.CreateImage.CreateImageRequest
                {
                    Model = "gpt-image-2",
                    Prompt = "A green rabbit jumping through a burning circle.",
                    N = 1
                }, CancellationToken.None);
                Console.WriteLine(JsonSerializer.Serialize(imageResult));
                Console.WriteLine("------------------------------------------------------");
            }
            catch (OpenAiException ex)
            {
                // Catch the SDK base type so API errors, timeouts and response-format
                // failures are all reported instead of escaping as an unhandled exception.
                Console.Error.WriteLine($"OpenAI request failed: {ExceptionReport.ToText(ex)}");
            }
        }
    }
}