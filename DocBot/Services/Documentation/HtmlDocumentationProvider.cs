using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;

namespace DocBot.Services.Documentation
{
    internal abstract class HtmlDocumentationProvider : DocumentationProvider
    {
        private readonly PhantomJsProvider phantomJs;

        protected HtmlDocumentationProvider(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            phantomJs = serviceProvider.GetRequiredService<PhantomJsProvider>();
        }

        public override async Task<IReadOnlyList<DocumentationArticle>> SearchArticles(string query)
        {
            var url = string.Format(SearchURLFormat, query);
            var doc = new HtmlDocument();
            doc.LoadHtml(await phantomJs.FetchHtml(url));
            return InternalGetDocumentationArticles(doc).ToList().AsReadOnly();
        }

        protected abstract IEnumerable<DocumentationArticle> InternalGetDocumentationArticles(HtmlDocument doc);
    }
}
