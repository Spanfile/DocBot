using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;

namespace DocBot.Services.Documentation
{
    internal abstract class HtmlDocumentationProvider : DocumentationProvider
    {
        public virtual string FetchScript { get; } = "fetchPage.js";
        public override bool IsAvailable => File.Exists(FetchScript) && phantomJs.ExecutableExists;

        private readonly PhantomJsProvider phantomJs;

        protected HtmlDocumentationProvider(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            phantomJs = serviceProvider.GetRequiredService<PhantomJsProvider>();
        }

        public override async Task<IReadOnlyList<DocumentationArticle>> SearchArticlesAsync(string query)
        {
            string url;
            string html;
            if (string.IsNullOrEmpty(SearchUrlFormat))
            {
                url = BaseUrl;
                await Logger.LogDebug($"Using base URL {url}");
                html = await phantomJs.FetchHtml(url, FetchScript, query);
            }
            else
            {
                url = string.Format(SearchUrlFormat, query);
                await Logger.LogDebug($"Using search URL {url}");
                html = await phantomJs.FetchHtml(url, FetchScript);
            }

            if (string.IsNullOrEmpty(html))
                throw new ArgumentException("PhantomJS returned no data");

            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            return ClearInvalidArticles(await InternalGetArticlesAsync(doc))?.ToList().AsReadOnly();
        }

        protected abstract Task<IEnumerable<DocumentationArticle>> InternalGetArticlesAsync(HtmlDocument doc);
    }
}
