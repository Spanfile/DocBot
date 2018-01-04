using System;
using System.Text;
using Discord;

namespace DocBot.Services.Documentation
{
    public class DocumentationArticle
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

        public void AddToEmbed(EmbedBuilder builder)
        {
            var valueBuilder = new StringBuilder();

            valueBuilder
                .Append(!string.IsNullOrWhiteSpace(Type) ? $"[{Type}]" : "[Link]")
                .Append($"({URL})");

            if (!string.IsNullOrWhiteSpace(Description))
                valueBuilder
                    .Append(" - ")
                    .Append(Description);

            builder.AddField(f =>
            {
                f.Name = Name;
                f.Value = valueBuilder.ToString();
            });
        }
    }
}
