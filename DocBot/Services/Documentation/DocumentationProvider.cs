using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DocBot.Services.Documentation
{
    public abstract class DocumentationProvider
    {
        public abstract string FriendlyName { get; }
        public abstract string[] Aliases { get; }
        public abstract string SearchUrlFormat { get; }
        public abstract string BaseUrl { get; }
        public virtual TimeSpan CacheTtl { get; } = TimeSpan.FromDays(7);
        public abstract bool IsAvailable { get; }

        protected IConfigurationRoot Config;
        protected IServiceProvider ServiceProvider;
        protected LoggingService Logger;

        protected DocumentationProvider(IServiceProvider serviceProvider)
        {
            Config = serviceProvider.GetRequiredService<IConfigurationRoot>();
            Logger = serviceProvider.GetRequiredService<LoggingService>();
            ServiceProvider = serviceProvider;
        }

        public abstract Task<IReadOnlyList<DocumentationArticle>> SearchArticlesAsync(string query);

        protected IReadOnlyList<DocumentationArticle> ClearInvalidArticles(IEnumerable<DocumentationArticle> articles) => articles?.Where(article => article.IsValid()).ToList().AsReadOnly();
    }
}
