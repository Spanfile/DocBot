using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace DocBot.Services.Documentation.Providers
{
    internal class ArchWikiProvider : JsonDocumentationProvider
    {
        public override string FriendlyName => "ArchWiki";
        public override string[] Aliases => new[] {"arch", "archwiki", "linux"};

        public override string SearchURLFormat =>
            "https://wiki.archlinux.org/api.php?action=opensearch&format=json&formatversion=2&search={0}&namespace=0&limit=10";

        public override string BaseURL => "https://wiki.archlinux.org/";

        public ArchWikiProvider(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override IEnumerable<DocumentationArticle> InternalGetDocumentationArticles(JsonTextReader reader)
        {
            throw new NotImplementedException();
        }
    }
}
