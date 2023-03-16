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
            services.AddSingleton<IModelsApi, ModelsApi>();
            services.AddSingleton<ICompletionsApi, CompletionsApi>();
            services.AddSingleton<IEditsApi, EditsApi>();
            services.AddSingleton<IImagesApi, ImagesApi>();
            services.AddSingleton<IEmbeddingsApi, EmbeddingsApi>();
            services.AddSingleton<IFilesApi, FilesApi>();
            services.AddSingleton<IFineTunesApi, FineTunesApi>();
            services.AddSingleton<IChat, ChatApi>();
            return services;
        }
    }
}
