using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenAI.SDK.Abstractions;
using OpenAI.SDK.Configuration;
using OpenAI.SDK.Models.Chat;
using System.Text.Json;

namespace OpenAI.SDK.SampleConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var apiKey = string.Empty;

            if (System.Diagnostics.Debugger.IsAttached)
            {

                var builder = new ConfigurationBuilder()
                    .AddUserSecrets<Program>();

                var configuration = builder.Build();

                apiKey = configuration.GetValue<string>("OpenAI:Api:ApiKey");
            }

            var services = new ServiceCollection();
            services.Configure<OpenAiOptions>(options => 
                {
                    options.ApiKey = apiKey;
                    options.BaseUrl = "https://api.openai.com/v1/";
                });
            services.AddHttpClient();
            services.AddOpenAiApi();

            var serviceProvider = services.BuildServiceProvider();

            var embeddingsApi = serviceProvider.GetRequiredService<IEmbeddingsApi>();
            var embeddingsResult = embeddingsApi.CreateEmbeddingsAsync(new Models.Embeddings.EmbeddingsRequest
            {
                Model = "text-embedding-ada-002",
                Input = new[] { "Just a test string" },
                User = "VB"
            }, CancellationToken.None).Result;


            Console.WriteLine(JsonSerializer.Serialize(embeddingsResult));
            Console.WriteLine("------------------------------------------------------");
            var modelsApi = serviceProvider.GetRequiredService<IModelsApi>();

            var models = modelsApi.GetModelsAsync(CancellationToken.None).Result;
            Console.WriteLine(JsonSerializer.Serialize(models));
            Console.WriteLine("------------------------------------------------------");

            var chatCompletionsApi = serviceProvider.GetRequiredService<IChatApi>();
            var chatCompletionResult = chatCompletionsApi.CreateCompletionAsync(new Models.Chat.ApiChatCompletionRequest
            {
                Model = "gpt-3.5-turbo",
                Messages = new List<ChatMessage>
                {
                    new ChatMessage{ Role ="user", Content = "Hello!"}
                }
            }, CancellationToken.None).Result;
            Console.WriteLine(JsonSerializer.Serialize(chatCompletionResult));
            Console.WriteLine("------------------------------------------------------");

            var imagesApi = serviceProvider.GetRequiredService<IImagesApi>();
            var imageResult = imagesApi.CreateImageAsync(new Models.CreateImage.CreateImageRequest
            {
                Prompt = "Green rabbit jumping through the burning circle.",
                N = 3
            }, CancellationToken.None).Result;
            Console.WriteLine(JsonSerializer.Serialize(imageResult));
            Console.WriteLine("------------------------------------------------------");
        }
    }
}