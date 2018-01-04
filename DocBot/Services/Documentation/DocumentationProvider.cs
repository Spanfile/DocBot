using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace DocBot.Services.Documentation
{
    internal abstract class DocumentationProvider
    {
        public abstract string FriendlyName { get; }
        public abstract string[] Aliases { get; }
        // ReSharper disable once InconsistentNaming
        public abstract string SearchURLFormat { get; }
        // ReSharper disable once InconsistentNaming
        public abstract string BaseURL { get; }

        public abstract TimeSpan CacheTTL { get; }

        protected HtmlWeb HtmlWeb;

        protected DocumentationProvider(HtmlWeb htmlWeb)
        {
            HtmlWeb = htmlWeb;
        }

        public async Task<IReadOnlyList<DocumentationArticle>> SearchArticles(string query)
        {
            // TODO: make sure this is safe
            var url = string.Format(SearchURLFormat, query);
            var doc = await HtmlWeb.LoadFromWebAsync(url);
            return InternalGetDocumentationArticles(doc);
        }

        protected abstract IReadOnlyList<DocumentationArticle> InternalGetDocumentationArticles(HtmlDocument doc);
    }
}
