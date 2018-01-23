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

            var queryArgs = query.ToLowerInvariant().Split('.');
            var actualQuery = queryArgs[queryArgs.Length - 1];

            var matches = new List<(int, IndexObject)>();
            foreach (var indexObj in index)
            {
                var nameArgs = indexObj.FullName.ToLowerInvariant().Split('.');
                var exactIndex = Array.IndexOf(nameArgs, actualQuery);

                if (exactIndex == -1) // exact match not found, check for the last elements
                {
                    var lastName = nameArgs[nameArgs.Length - 1];
                    if (lastName.Contains(actualQuery))
                    {
                        // query found as substring - rank based on length difference: larger length difference = less equal match
                        matches.Add((lastName.Length - actualQuery.Length, indexObj));
                    }
                }
                else // exact index found, rank based on how close to the end the match is by characters
                {
                    matches.Add((nameArgs.Skip(exactIndex + 1).Sum(s => s.Length), indexObj));
                }
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
