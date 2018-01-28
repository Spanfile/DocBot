using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Linq;
using Newtonsoft.Json;

namespace DocBot.Services.Documentation
{
    internal abstract class JsonIndexDocumentationProvider : DocumentationProvider
    {
        public abstract string IndexLocation { get; }
        public override bool IsAvailable => File.Exists(IndexLocation) || File.Exists(IndexLocation + ".gz");

        protected JsonIndexDocumentationProvider(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        public override async Task<IReadOnlyList<DocumentationArticle>> SearchArticles(string query)
        {            
            var file = IndexLocation + ".gz";
            var useCompressed = File.Exists(file);

            if (!useCompressed)
            {
                if (!File.Exists(IndexLocation))
                {
                    await Logger.LogError($"Invalid index location: {IndexLocation}", "JsonIndexDocumentationProvider");
                    throw new FileNotFoundException("", IndexLocation);
                }

                await Logger.LogWarning($"Using non-compressed index ({IndexLocation})",
                    "JsonIndexDocumentationProvider");
                file = IndexLocation;
            }

            using (var fileStream = File.OpenRead(file))
            {
                Stream stream = fileStream;

                if (useCompressed)
                    stream = new GZipStream(fileStream, CompressionMode.Decompress);

                IReadOnlyList<DocumentationArticle> articles;

                using (var textReader = new StreamReader(stream))
                using (var jsonReader = new JsonTextReader(textReader))
                    articles = ClearInvalidArticles(await InternalGetDocumentationArticle(jsonReader, query))?.ToList()
                        .AsReadOnly();

                if (useCompressed)
                    stream.Dispose();

                return articles;
            }
        }

        protected abstract Task<IEnumerable<DocumentationArticle>> InternalGetDocumentationArticle(JsonTextReader reader, string query);
    }
}
