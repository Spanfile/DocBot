using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DocBot.Services.Documentation.Providers
{
    internal class CppReferenceProvider : JsonDocumentationProvider
    {
        public override string FriendlyName => "C++ reference";
        public override string[] Aliases => new[] {"cpp", "c++", "cplusplus", "cppreference", "cppref"};
        public override string SearchUrlFormat => "http://en.cppreference.com/mwiki/api.php?action=query&list=search&format=json&srsearch=thread&srlimit=50&srprops=title";
        public override string BaseUrl => "http://en.cppreference.com/w/";
        public override bool IsAvailable => File.Exists("cpp.zip");

        public CppReferenceProvider(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override async Task<IEnumerable<DocumentationArticle>> InternalGetArticlesAsync(JsonTextReader reader)
        {
            var searchResults = (await JObject.LoadAsync(reader))["query"]["search"];
            var resultsList = new List<DocumentationArticle>();

            foreach (var result in searchResults)
            {
                var relativeUrl = result["title"];
                var absoluteUrl = BaseUrl + relativeUrl;

            }
        }


    }
}
