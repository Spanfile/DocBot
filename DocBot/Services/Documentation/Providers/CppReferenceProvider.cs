using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DocBot.Services.Documentation.Providers
{
    internal class CppReferenceProvider : JsonIndexDocumentationProvider
    {
        public override string FriendlyName => "C++ reference";
        public override string[] Aliases => new[] {"cpp", "c++", "cplusplus", "cppreference", "cppref"};
        public override string SearchUrlFormat => null;
        public override string BaseUrl => "http://en.cppreference.com/w/";
        public override string IndexLocation => "cpp-index.json";

        public CppReferenceProvider(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override async Task<IEnumerable<DocumentationArticle>> InternalGetDocumentationArticle(JsonTextReader reader, string query)
        {
            var serialiser = new JsonSerializer();
            var index = serialiser.Deserialize<DocumentationArticle[]>(reader);
            await Logger.LogDebug($"{index.Length} objects loaded from index", "CppReferenceProvider");

            var lowerQuery = query.ToLowerInvariant();
            var sortedMatches =
                from indexObj in index
                let fullLowerName = indexObj.Name.ToLowerInvariant()
                let foundIndex = fullLowerName.IndexOf(lowerQuery, StringComparison.Ordinal)
                where foundIndex != -1
                let trimmedName = fullLowerName.Trim('<', '>', '(', ')')
                let rank = trimmedName.Length - query.Length + foundIndex
                orderby rank, indexObj.Name
                select indexObj;

            return sortedMatches;
        }
    }
}
