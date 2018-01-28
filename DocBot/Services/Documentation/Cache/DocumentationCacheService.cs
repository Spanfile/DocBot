using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace DocBot.Services.Documentation.Cache
{
    public class DocumentationCacheService
    {
        internal const string CacheFile = "cache.json";

        public int Articles => cacheContainers.Values.Sum(c => c.Articles.Count);
        public int Queries => cacheContainers.Keys.Count;
        public long FileSize
        {
            get
            {
                try
                {
                    return new FileInfo(compress ? CacheFile + ".gz" : CacheFile).Length;
                }
                catch (FileNotFoundException)
                {
                    return 0;
                }
            }
        }

        private readonly IConfigurationRoot config;
        private readonly LoggingService logger;
        private readonly bool compress;

        private Dictionary<string, DocumentationCacheContainer> cacheContainers;

        public DocumentationCacheService(IConfigurationRoot config, LoggingService logger)
        {
            this.config = config;
            this.logger = logger;

            cacheContainers = new Dictionary<string, DocumentationCacheContainer>();

#if DEBUG
            compress = false;
#else
            compress = true;
#endif
        }

        public async Task Load()
        {
            var file = CacheFile;

            if (compress)
                file += ".gz";

            if (!File.Exists(file))
            {
                await logger.LogDebug($"No existing cache found ({file})", "DocumentationCacheService");
                return;
            }

            using (var fileStream = File.OpenRead(file))
            {
                Stream stream = fileStream;

                if (compress)
                    stream = new GZipStream(fileStream, CompressionMode.Decompress);

                using (var textReader = new StreamReader(stream))
                using (var jsonReader = new JsonTextReader(textReader))
                {
                    var serialiser = new JsonSerializer();
                    cacheContainers =
                        serialiser.Deserialize<Dictionary<string, DocumentationCacheContainer>>(jsonReader);
                }

                if (compress)
                    stream.Dispose();
            }

            await logger.LogDebug($"{cacheContainers.Count} cache containers loaded. Articles in total: {Articles}", "DocumentationCacheService");
        }

        public async Task Save()
        {
            //await File.WriteAllTextAsync(CacheFile, JsonConvert.SerializeObject(cacheContainers));

            var file = CacheFile;

            if (compress)
                file += ".gz";

            using (var fileStream = File.OpenWrite(file))
            {
                Stream stream = fileStream;

                if (compress)
                    stream = new GZipStream(fileStream, CompressionLevel.Optimal);

                using (var textWriter = new StreamWriter(stream))
                {
                    var serialiser = new JsonSerializer();
                    await Task.Factory.StartNew(writer => serialiser.Serialize(writer as TextWriter, cacheContainers), textWriter);
                }

                if (compress)
                    stream.Dispose();
            }
        }

        public IReadOnlyList<DocumentationArticle> Get(string doc, string search)
        {
            var searchValue = doc + search;
            if (!cacheContainers.TryGetValue(searchValue, out var container))
                return null;

            if (!container.Expired)
                return container.Articles;

            cacheContainers.Remove(searchValue);

            return null;
        }

        public async Task Add(string doc, string search, IReadOnlyList<DocumentationArticle> articles, TimeSpan ttl)
        {
            var searchValue = doc + search;
            cacheContainers.TryAdd(searchValue, new DocumentationCacheContainer(articles, ttl));
            await Save();
        }
    }
}
