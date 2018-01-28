using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace DocBot.Services.Documentation.Providers
{
    internal class RustProvider : HtmlDocumentationProvider
    {
        public override string FriendlyName => "Rust";
        public override string[] Aliases => new[] {"rust", "rustdoc"};
        public override string SearchUrlFormat => "https://doc.rust-lang.org/std/?search={0}";
        public override string BaseUrl => "https://doc.rust-lang.org/std/";

        private readonly Dictionary<string, string> typeClassDictionary = new Dictionary<string, string> {
            {"mod", "Module"},
            {"struct", "Struct"},
            {"macro", "Macro"},
            {"tymethod", "Trait method signature"},
            {"fn", "Function"},
            {"trait", "Trait"},
            {"constant", "Constant"},
            {"method", "Method"}
        };

        public RustProvider(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override async Task<IEnumerable<DocumentationArticle>> InternalGetArticlesAsync(HtmlDocument doc)
        {
            var results = doc.DocumentNode.SelectNodes("//table[@class='search-results']/tbody/*");

            if (results == null || !results.Any())
                return null;

            var resultsList = new List<DocumentationArticle>();

            foreach (var result in results)
            {
                var name = result.ChildNodes[0].InnerText;
                var url = result.ChildNodes[0].SelectSingleNode("//a").GetAttributeValue("href", null);
                var description = result.ChildNodes[1].InnerText;
                var typeClass = result.GetClasses().First(c => c != "result");

                if (!typeClassDictionary.TryGetValue(typeClass, out var type))
                {
                    type = "";
                    await Logger.LogDebug($"Couldn't find matching type for class \"{typeClass}\"", "RustProvider");
                }

                resultsList.Add(new DocumentationArticle(name, url, description, type));
            }

            return resultsList.AsEnumerable();
        }
    }
}
