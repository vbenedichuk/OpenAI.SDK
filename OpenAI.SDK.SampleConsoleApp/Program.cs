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
            services.AddTransient<IOpenAiAPI, OpenAiApi>();

            var serviceProvider = services.BuildServiceProvider();
            var api = serviceProvider.GetRequiredService<IOpenAiAPI>();
            var models = api.GetModels().Result;

            Console.WriteLine(JsonSerializer.Serialize(models));
        }
    }
}