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
        public virtual string FetchScript { get; } = "fetchPage.js";

        private readonly PhantomJsProvider phantomJs;

        protected HtmlDocumentationProvider(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            phantomJs = serviceProvider.GetRequiredService<PhantomJsProvider>();
        }

        public override async Task<IReadOnlyList<DocumentationArticle>> SearchArticles(string query)
        {
            string url;
            string html;
            if (string.IsNullOrEmpty(SearchURLFormat))
            {
                url = BaseURL;
                html = await phantomJs.FetchHtml(url, FetchScript, query);
            }
            else
            {
                url = string.Format(SearchURLFormat, query);
                html = await phantomJs.FetchHtml(url, FetchScript);
            }

            if (string.IsNullOrEmpty(html))
                throw new ArgumentException("PhantomJS returned no data");

            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            return ClearInvalidArticles(InternalGetDocumentationArticles(doc)).ToList().AsReadOnly();
        }

        protected abstract IEnumerable<DocumentationArticle> InternalGetDocumentationArticles(HtmlDocument doc);
    }
}
