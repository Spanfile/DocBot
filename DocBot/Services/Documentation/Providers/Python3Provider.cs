﻿using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace DocBot.Services.Documentation.Providers
{
    internal class Python3Provider : HtmlDocumentationProvider
    {
        public override string FriendlyName => "Python 3 documentation";
        public override string[] Aliases => new[] {"python", "py", "py3"};
        public override string SearchURLFormat => "https://docs.python.org/3/search.html?q={0}";
        public override string BaseURL => "https://docs.python.org/3/";
        public override TimeSpan CacheTTL => TimeSpan.FromDays(7);

        public Python3Provider(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override IEnumerable<DocumentationArticle> InternalGetDocumentationArticles(HtmlDocument doc)
        {
            var results = doc.DocumentNode.SelectNodes("//ul[@class='search']/*");

            if (results == null || !results.Any())
                return null;

            var resultsList = (from item in results
                               let aTag = item.SelectSingleNode("a")
                               let url = aTag.Attributes["href"].Value
                               let name = aTag.InnerText
                               let type = item.SelectSingleNode("span").InnerText.Split(' ')[0].Trim(',')
                               select new DocumentationArticle(name, url, type: type)).ToList();

            return resultsList.AsEnumerable();
        }
    }
}