using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DocBot.Services.Documentation
{
    internal abstract class JsonIndexDocumentationProvider : DocumentationProvider
    {
        public abstract string IndexLocation { get; }

        protected JsonIndexDocumentationProvider(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        public override Task<IReadOnlyList<DocumentationArticle>> SearchArticles(string query)
        {
            throw new NotImplementedException();
        }
    }
}
