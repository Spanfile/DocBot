using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace DocBot.Services.Documentation
{
    public class DocumentationCacheService
    {
        internal const string CacheFile = "cache.json";

        public int Articles => cacheContainers.Values.Sum(c => c.Articles.Count);
        public int Queries => cacheContainers.Keys.Count;
        public long FileSize => new FileInfo(CacheFile).Length;

        private readonly IConfigurationRoot config;
        private readonly LoggingService logger;

        private Dictionary<string, DocumentationCacheContainer> cacheContainers;

        public DocumentationCacheService(IConfigurationRoot config, LoggingService logger)
        {
            this.config = config;
            this.logger = logger;

            cacheContainers = new Dictionary<string, DocumentationCacheContainer>();
        }

        public async Task Load()
        {
            if (!File.Exists(CacheFile))
            {
                await logger.LogDebug("No existing cache found", "DocumentationCacheService");
                return;
            }

            cacheContainers =
                JsonConvert.DeserializeObject<Dictionary<string, DocumentationCacheContainer>>(
                    await File.ReadAllTextAsync(CacheFile));
        }

        public async Task Save()
        {
            await File.WriteAllTextAsync(CacheFile, JsonConvert.SerializeObject(cacheContainers));
        }

        public IReadOnlyList<DocumentationArticle> Get(string search)
        {
            if (!cacheContainers.TryGetValue(search, out var container))
                return null;

            if (!container.Expired)
                return container.Articles;

            cacheContainers.Remove(search);

            return null;
        }

        public async Task Add(string search, IReadOnlyList<DocumentationArticle> articles, TimeSpan ttl)
        {
            cacheContainers.TryAdd(search, new DocumentationCacheContainer(articles, ttl));
            await Save();
        }
    }
}
