using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DocBot.Services.Documentation.Providers
{
    internal class ArchWikiProvider : JsonDocumentationProvider
    {
        public override string FriendlyName => "ArchWiki";
        public override string[] Aliases => new[] {"arch", "archwiki", "linux"};

        public override string SearchUrlFormat =>
            "https://wiki.archlinux.org/api.php?action=opensearch&format=json&formatversion=2&search={0}&namespace=0&limit=10";

        public override string BaseUrl => "https://wiki.archlinux.org/";

        public ArchWikiProvider(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override async Task<IEnumerable<DocumentationArticle>> InternalGetDocumentationArticles(JsonTextReader reader)
        {
            var serialiser = new JsonSerializer();
            var apiResults = (JArray)serialiser.Deserialize(reader);

            var names = apiResults[1].ToObject<string[]>();
            var urls = apiResults[3].ToObject<string[]>();

            if (names == null)
            {
                await Logger.LogWarning("No names found", "ArchWikiProvider");
                return null;
            }

            if (urls == null)
            {
                await Logger.LogWarning("No URLs found", "ArchWikiProvider");
                return null;
            }

            return names.Zip(urls, (name, url) => new DocumentationArticle(name.ToString(), url.ToString()));
        }
    }
}
