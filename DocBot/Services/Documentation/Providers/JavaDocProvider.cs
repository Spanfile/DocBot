using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DocBot.Services.Documentation.Providers
{
    internal class JavaDocProvider : JsonIndexDocumentationProvider
    {
        private struct IndexObject
        {
            [JsonProperty(PropertyName = "full_name")]
            public string FullName;
            [JsonProperty(PropertyName = "url")]
            public string Url;
            [JsonProperty(PropertyName = "object_type")]
            public string ObjectType;
            [JsonProperty(PropertyName = "description")]
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

            var sanitisedQuery = query.Trim('.');
            var lowerQuery = sanitisedQuery.ToLowerInvariant();
            var matches = new List<(int, IndexObject)>();
            foreach (var indexObj in index)
            {
                var fullLowerName = indexObj.FullName.ToLowerInvariant();
                var foundIndex = fullLowerName.IndexOf(lowerQuery, StringComparison.Ordinal);

                if (foundIndex == -1)
                    continue;

                // calculate the closest period to the left. the closer it is, the more accurate the matched query is
                var periodIndex = foundIndex - 1;
                while (periodIndex >= 0)
                {
                    if (fullLowerName[periodIndex] == '.')
                        break;

                    periodIndex -= 1;
                }

                var rank = indexObj.FullName.Length - (foundIndex + sanitisedQuery.Length) + (foundIndex - 1 - periodIndex);
                matches.Add((rank, indexObj));
            }

            await Logger.LogDebug($"{matches.Count} matches found", "JavaDocProvider");

            var sorted =
                from match in matches
                orderby match.Item1, ObjectTypeToInt(match.Item2.ObjectType), match.Item2.FullName
                select new DocumentationArticle(match.Item2.FullName, match.Item2.Url, match.Item2.Description,
                    match.Item2.ObjectType);

            return sorted;
        }

        private int ObjectTypeToInt(string objectType)
        {
            switch (objectType)
            {
                case "Class":
                    return 0;

                case "Interface":
                    return 1;

                case "Constructor":
                    return 2;

                case "Method":
                    return 3;

                case "Constant":
                    return 4;

                case "Variable":
                    return 5;

                default:
                    return int.MaxValue;
            }
        }
    }
}
