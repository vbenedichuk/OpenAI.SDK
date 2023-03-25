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
            var embeddingsResult = embeddingsApi.CreateEmbeddings(new Models.Embeddings.EmbeddingsRequest
            {
                Model = "text-embedding-ada-002",
                Input = new[] { "Just a test string" },
                User = "VB"
            }).Result;


            Console.WriteLine(JsonSerializer.Serialize(embeddingsResult));
            Console.WriteLine("------------------------------------------------------");
            var modelsApi = serviceProvider.GetRequiredService<IModelsApi>();

            var models = modelsApi.GetModelsAsync().Result;
            Console.WriteLine(JsonSerializer.Serialize(models));
            Console.WriteLine("------------------------------------------------------");

            var completionsApi = serviceProvider.GetRequiredService<ICompletionsApi>();

            var completionResult = completionsApi.CreateCompletionAsync(new Models.Completions.ApiCompletionRequest
            {
                Model = "davinci",
                Prompt =
                new List<string>()
                {
                                @"I celebrate myself, and sing myself,
            And what I assume you shall assume,",
                                @"I sing the body electric,
            The armies of those I love engirth me and I engirth them,"
                },
                Temperatue = 0.2
            }).Result;
            foreach (var edit in completionResult.Choices)
            {
                Console.WriteLine(edit.Text);
            }
            Console.WriteLine(JsonSerializer.Serialize(completionResult));
            Console.WriteLine("------------------------------------------------------");

            var chatCompletionsApi = serviceProvider.GetRequiredService<IChatApi>();
            var chatCompletionResult = chatCompletionsApi.CreateCompletionAsync(new Models.Chat.ApiChatCompletionRequest
            {
                Model = "gpt-3.5-turbo",
                Messages = new List<ChatMessage>
                {
                    new ChatMessage{ Role ="user", Content = "Hello!"}
                }
            }).Result;
            Console.WriteLine(JsonSerializer.Serialize(chatCompletionResult));
            Console.WriteLine("------------------------------------------------------");

            var editsApi = serviceProvider.GetRequiredService<IEditsApi>();
            var editResult = editsApi.CreateEdit(new Models.Edits.ApiCreateEditRequest
            {
                Model = "text-davinci-edit-001",

                Input = "Iis it possible fix spelling hre?",
                Instruction = "Fix spelling and grammar",
                Temperatue = 0.2
            }).Result;
            foreach (var edit in editResult.Choices)
            {
                Console.WriteLine(edit.Text);
            }
            Console.WriteLine(JsonSerializer.Serialize(editResult));
            Console.WriteLine("------------------------------------------------------");

            var imagesApi = serviceProvider.GetRequiredService<IImagesApi>();
            var imageResult = imagesApi.CreateImageAsync(new Models.CreateImage.CreateImageRequest
            {
                Prompt = "Green rabbit jumping through the burning circle.",
                N = 3
            }).Result;
            Console.WriteLine(JsonSerializer.Serialize(imageResult));
            Console.WriteLine("------------------------------------------------------");

            var imageStream = File.OpenRead("BlackCircle.png");
            var imageVariationsResult = imagesApi.CreateImageVariation(imageStream, "image.png", 3, "1024x1024").Result;
            Console.WriteLine(JsonSerializer.Serialize(imageVariationsResult));
            Console.WriteLine("------------------------------------------------------");

        }
    }
}