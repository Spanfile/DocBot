using System;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace DocBot.Services.Documentation.Providers
{
    internal class MSDocProvider : DocumentationProvider
    {
        public override string FriendlyName => ".NET API Browser";
        public override string[] Aliases => new[] {"msdoc", "ms", ".net", "dotnet", "csharp", "cs", "c#"};
        public override string SearchURLFormat => "https://docs.microsoft.com/en-us/dotnet/api/?term={}";

        protected override IEnumerable<DocumentationArticle> GetDocumentationArticles(HtmlDocument doc)
        {
            throw new NotImplementedException();
        }
    }
}
