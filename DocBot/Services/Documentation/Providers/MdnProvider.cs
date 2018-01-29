using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DocBot.Services.Documentation.Providers
{
    internal class MdnProvider : JsonDocumentationProvider
    {
        public override string FriendlyName => "Mozilla Developer Network";
        public override string[] Aliases => new[] {"mdn", "mozilla", "js", "javacsript"};
        public override string SearchUrlFormat => "https://developer.mozilla.org/en-US/search.json?q={0}";
        public override string BaseUrl => "https://developer.mozilla.org/en-US/";
        public override bool IsAvailable => true;

        public MdnProvider(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override async Task<IEnumerable<DocumentationArticle>> InternalGetArticlesAsync(JsonTextReader reader)
        {
            var documents = (await JObject.LoadAsync(reader))["documents"];

            var resultsList = (from jsonDoc in documents
                let name = jsonDoc["title"].ToString()
                let url = jsonDoc["url"].ToString()
                let description = jsonDoc["excerpt"].ToString()
                select new DocumentationArticle(name, url, description)).ToList();

            return resultsList.AsEnumerable();
        }
    }
}
