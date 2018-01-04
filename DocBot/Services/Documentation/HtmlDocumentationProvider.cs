using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;

namespace DocBot.Services.Documentation
{
    internal abstract class HtmlDocumentationProvider : DocumentationProvider
    {
        protected HtmlDocumentationProvider(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        public override Task<IReadOnlyList<DocumentationArticle>> SearchArticles(string query)
        {
            throw new InvalidOperationException();
        }

        protected abstract IReadOnlyList<DocumentationArticle> InternalGetDocumentationArticles(HtmlDocument doc);
    }
}
