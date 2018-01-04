using System;

namespace DocBot.Services.Documentation
{
    internal class DocumentationArticle
    {
        public string Name { get; }
        public string URL { get; }
        public string Description { get; }
        public string Type { get; }

        public DocumentationArticle(string name, string url, string description = null, string type = null)
        {
            Name = name;
            URL = url;
            Description = description;
            Type = type;
        }
    }
}
