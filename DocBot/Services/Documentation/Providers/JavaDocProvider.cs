using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DocBot.Services.Documentation.Providers
{
    internal class JavaDocProvider : JsonIndexDocumentationProvider
    {
        private struct IndexObject
        {
            public string FullName;
            public string Url;
            public string ObjectType;
            public string Description;

            public IndexObject(string fullName, string url, string objectType, string description)
            {
                FullName = fullName;
                Url = url;
                ObjectType = objectType;
                Description = description;
            }
        }

        public override string FriendlyName => "Java SE 9 & JDK 9";
        public override string[] Aliases => new[] {"java", "javase", "javajdk", "java9", "javase9", "javajdk9"};
        public override string SearchURLFormat => null;
        public override string BaseURL => "https://docs.oracle.com/javase/9/docs/api/overview-summary.html";
        public override string IndexLocation => "java-index.json";

        public JavaDocProvider(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override async Task<IEnumerable<DocumentationArticle>> InternalGetDocumentationArticle(JsonTextReader reader, string query)
        {
            var serialiser = new JsonSerializer();
            var index = serialiser.Deserialize<IndexObject[]>(reader);

            await Logger.LogDebug($"{index.Length} objects loaded from index", "JavaDocProvider");

            foreach (var indexObj in index)
            {
                
            }
        }
    }
}
