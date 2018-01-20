using System;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace DocBot.Services.Documentation.Providers
{
    internal class JavaDocProvider : HtmlDocumentationProvider
    {
        public override string FriendlyName => "Java SE 9 & JDK 9";
        public override string[] Aliases => new[] {"java", "javase", "javajdk", "java9", "javase9", "javajdk9"};
        public override string SearchURLFormat => null;
        public override string BaseURL => "https://docs.oracle.com/javase/9/docs/api/overview-summary.html";
        public override string FetchScript => "fetchJavaDoc.js";

        public JavaDocProvider(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override IEnumerable<DocumentationArticle> InternalGetDocumentationArticles(HtmlDocument doc)
        {
            var resultNodeContainer = doc.DocumentNode.SelectSingleNode("//*[@id='ui-id-1']");
            var currentType = "";
            foreach (var child in resultNodeContainer.ChildNodes)
            {
                if (child.HasClass("ui-autocomplete-category"))
                    currentType = child.InnerText.Trim('s');

                if (child.HasClass("resultItem"))
                    yield return new DocumentationArticle(child.InnerText, BaseURL, type: currentType);
            }
        }
    }
}
