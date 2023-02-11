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
            services.AddSingleton<IOpenAiAPI, OpenAiApi>();
            services.AddSingleton<IImagesApi, ImagesApi>();
            services.AddSingleton<IOpenApiEmbeddings, OpenApiEmbeddingsApi>();
            services.AddSingleton<IOpenApiFiles, OpenApiFiles>();
            return services;
        }
    }
}
