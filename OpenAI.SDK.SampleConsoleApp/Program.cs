using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenAI.SDK.Abstractions;
using OpenAI.SDK.Configuration;
using OpenAI.SDK.Logic;
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
            services.Configure<OpenApiOptions>(options => 
                {
                    options.ApiKey = apiKey;
                    options.BaseUrl = "https://api.openai.com/v1/";
                });
            services.AddHttpClient();
            services.AddOpenAiApi();

            var serviceProvider = services.BuildServiceProvider();
            var api = serviceProvider.GetRequiredService<IOpenAiAPI>();

            //var models = api.GetModelsAsync().Result;
            //Console.WriteLine(JsonSerializer.Serialize(models));
            var editResult = api.CreateEdit(new Models.Edits.ApiCreateEditRequest
            {
                Model = "code-davinci-edit-001",
                Input= @"    public class ApiCreateEditRequest
    {
        [JsonPropertyName(""model"")]
        public string Model { get; set; }
        [JsonPropertyName(""input"")]
        public string Input { get; set; }
        [JsonPropertyName(""instruction"")]
        public string Instruction { get; set; }
        [JsonPropertyName(""temperature"")]
        public double Temperatue { get; set; }
        [JsonPropertyName(""top_p"")]
        public int? TopP { get; set; }
        [JsonPropertyName(""n"")]
        public int? N { get; set; }
    }",
                Instruction = "this class is a part of chatgpt responses model. Could you please add doc comments to it.",
                Temperatue = 0.5
            }).Result;
            foreach(var edit in editResult.Choices)
            {
                Console.WriteLine(edit.Text);
            }
            Console.Write(JsonSerializer.Serialize(editResult));
        }
    }
}