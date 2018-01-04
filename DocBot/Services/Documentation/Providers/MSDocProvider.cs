using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DocBot.Services.Documentation.Providers
{
    internal class MsDocProvider : JsonDocumentationProvider
    {
        private struct MsDocApiResult
        {
            public Dictionary<string, string>[] Results;
            public int Count;
            public string NextLink;
        }

        public override string FriendlyName => ".NET API Browser";
        public override string[] Aliases => new[] {"msdoc", "ms", ".net", "dotnet", "csharp", "cs", "c#"};
        public override string SearchURLFormat => "https://docs.microsoft.com/api/apibrowser/dotnet/search?api-version=0.2&search={0}";
        public override string BaseURL => "https://docs.microsoft.com/en-us/dotnet/api/";
        public override TimeSpan CacheTTL => TimeSpan.FromDays(7);

        public MsDocProvider(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override IEnumerable<DocumentationArticle> InternalGetDocumentationArticles(JsonTextReader reader)
        {
            var serializer = new JsonSerializer();
            var apiResult = serializer.Deserialize<MsDocApiResult>(reader);

            return apiResult.Results.Select(r =>
                new DocumentationArticle(r["displayName"], r["url"], r["description"], r["itemKind"]));
        }
    }
}
