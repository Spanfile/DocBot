using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace DocBot.Services.Documentation.Providers
{
    internal class MSDocProvider : DocumentationProvider
    {
        public override string FriendlyName => ".NET API Browser";
        public override string[] Aliases => new[] {"msdoc", "ms", ".net", "dotnet", "csharp", "cs", "c#"};
        public override string SearchURLFormat => "https://docs.microsoft.com/en-us/dotnet/api/?term={0}";
        public override string BaseURL => "https://docs.microsoft.com";
        public override TimeSpan CacheTTL => TimeSpan.FromHours(1);

        public MSDocProvider(HtmlWeb htmlWeb) : base(htmlWeb)
        {
        }

        protected override IReadOnlyList<DocumentationArticle> InternalGetDocumentationArticles(HtmlDocument doc)
        {
            var tableRows = doc.DocumentNode.SelectNodes(
                "//table[@class='api-search-results']/tbody/*");
            var results = (from row in tableRows.Take(5)
                           let nameData = row.SelectSingleNode("td[1]")
                           let name = nameData.SelectSingleNode("a").Attributes["ms.title"].Value
                           let url = nameData.SelectSingleNode("a").Attributes["href"].Value
                           let type = nameData.SelectSingleNode("span").InnerText
                           let description = row.SelectSingleNode("td[2]").InnerText
                           select new DocumentationArticle(name, url, description, type)).ToList();

            return results.AsReadOnly();
        }
    }
}
