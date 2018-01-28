using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DocBot.Services.Documentation
{
    internal abstract class JsonDocumentationProvider : DocumentationProvider
    {
        protected JsonDocumentationProvider(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        public override async Task<IReadOnlyList<DocumentationArticle>> SearchArticlesAsync(string query)
        {
            var uri = string.Format(SearchUrlFormat, query);
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", Config["useragent"]);
                using (var stream = await httpClient.GetStreamAsync(uri))
                using (var textReader = new StreamReader(stream))
                using (var reader = new JsonTextReader(textReader))
                    return ClearInvalidArticles(await InternalGetArticlesAsync(reader))?.ToList()
                        .AsReadOnly();
            }
        }

        protected abstract Task<IEnumerable<DocumentationArticle>> InternalGetArticlesAsync(JsonTextReader reader);
    }
}
