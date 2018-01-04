﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DocBot.Services.Documentation
{
    internal abstract class DocumentationProvider
    {
        public abstract string FriendlyName { get; }
        public abstract string[] Aliases { get; }
        // ReSharper disable once InconsistentNaming
        public abstract string SearchURLFormat { get; }
        // ReSharper disable once InconsistentNaming
        public abstract string BaseURL { get; }
        // ReSharper disable once InconsistentNaming
        public abstract TimeSpan CacheTTL { get; }

        protected IConfigurationRoot Config;
        protected IServiceProvider ServiceProvider;

        protected DocumentationProvider(IServiceProvider serviceProvider)
        {
            Config = serviceProvider.GetRequiredService<IConfigurationRoot>();
            ServiceProvider = serviceProvider;
        }

        public abstract Task<IReadOnlyList<DocumentationArticle>> SearchArticles(string query);
    }
}