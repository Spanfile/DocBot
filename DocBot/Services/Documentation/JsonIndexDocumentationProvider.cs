using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DocBot.Services.Documentation
{
    internal abstract class JsonIndexDocumentationProvider : DocumentationProvider
    {
        public abstract string IndexLocation { get; }

        protected bool IndexInitialised = false;

        protected JsonIndexDocumentationProvider(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        private void InitialiseIndex()
        {
            if (IndexInitialised)
                return;


        }

        public override Task<IReadOnlyList<DocumentationArticle>> SearchArticles(string query)
        {
            throw new NotImplementedException();
        }
    }
}
