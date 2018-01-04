using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace DocBot.Services.Documentation
{
    public class DocumentationCacheService
    {
        public int Articles => cacheContainers.Values.Sum(c => c.Articles.Count);
        public int Queries => cacheContainers.Keys.Count;

        private readonly IConfigurationRoot config;
        private readonly LoggingService logger;

        private readonly Dictionary<string, DocumentationCacheContainer> cacheContainers;

        public DocumentationCacheService(IConfigurationRoot config, LoggingService logger)
        {
            this.config = config;
            this.logger = logger;

            cacheContainers = new Dictionary<string, DocumentationCacheContainer>();
        }

        public async Task Load()
        {
            
        }

        public async Task Save()
        {
            
        }

        public IReadOnlyList<DocumentationArticle> Get(string search)
        {
            if (cacheContainers.TryGetValue(search, out var container))
            {
                if (!container.Expired)
                    return container.Articles;

                cacheContainers.Remove(search);
            }

            return null;
        }

        public void Add(string search, IReadOnlyList<DocumentationArticle> articles, TimeSpan ttl)
        {
            cacheContainers.TryAdd(search, new DocumentationCacheContainer(articles, ttl));
        }
    }
}
