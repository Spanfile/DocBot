using System;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace DocBot.Services.Documentation
{
    internal abstract class DocumentationProvider
    {
        public abstract string FriendlyName { get; }
        public abstract string[] Aliases { get; }
        // ReSharper disable once InconsistentNaming
        public abstract string SearchURLFormat { get; }

        protected abstract IEnumerable<DocumentationArticle> GetDocumentationArticles(HtmlDocument doc);
    }
}
