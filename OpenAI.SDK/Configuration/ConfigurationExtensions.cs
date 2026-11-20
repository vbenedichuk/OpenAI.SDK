using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenAI.SDK.Abstractions;
using OpenAI.SDK.Logic;
using System;

namespace OpenAI.SDK.Configuration
{
    public static class ConfigurationExtensions
    {
        public const string HttpClientName = "OpenAI";

        public static IServiceCollection AddOpenAiApi(this IServiceCollection services)
        {
            services.AddHttpClient(HttpClientName, client => client.Timeout = System.Threading.Timeout.InfiniteTimeSpan);

            // Hosts call IStartupValidator during startup, so invalid options fail fast
            // instead of surfacing on the first API resolution.
            services.AddSingleton<IStartupValidator>(
                provider => new OpenAiOptionsStartupValidator(provider.GetRequiredService<IOptions<OpenAiOptions>>()));

            services.AddSingleton<IModelsApi, ModelsApi>();
#pragma warning disable CS0612 // Retired APIs stay registered for backward compatibility.
            services.AddSingleton<ICompletionsApi, CompletionsApi>();
            services.AddSingleton<IEditsApi, EditsApi>();
#pragma warning restore CS0612
            services.AddSingleton<IImagesApi, ImagesApi>();
            services.AddSingleton<IEmbeddingsApi, EmbeddingsApi>();
            services.AddSingleton<IFilesApi, FilesApi>();
#pragma warning disable CS0612 // Retired APIs stay registered for backward compatibility.
            services.AddSingleton<IFineTunesApi, FineTunesApi>();
#pragma warning restore CS0612
            services.AddSingleton<IChatApi, ChatApi>();
            services.AddSingleton<IModerationsApi, ModerationsApi>();
#pragma warning disable CS0612 // Retired APIs stay registered for backward compatibility.
            services.AddSingleton<IEnginesApi, EnginesApi>();
#pragma warning restore CS0612
            return services;
        }
    }
}
