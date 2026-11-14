using Microsoft.Extensions.DependencyInjection;
using OpenAI.SDK.Abstractions;
using OpenAI.SDK.Logic;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAI.SDK.Configuration
{
    public static class ConfigurationExtensions
    {
        public static IServiceCollection AddOpenAiApi(this IServiceCollection services)
        {
            services.AddHttpClient();

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
