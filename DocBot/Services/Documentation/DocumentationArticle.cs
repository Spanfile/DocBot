using System;
using System.Text;
using Discord;

namespace DocBot.Services.Documentation
{
    public class DocumentationArticle
    {
        public string Name { get; }
        public string Url { get; }
        public string Description { get; }
        public string Type { get; }

        public DocumentationArticle(string name, string url, string description = null, string type = null)
        {
            Name = name;
            Url = url;
            Description = description;
            Type = type;
        }

        public void AddToEmbed(EmbedBuilder builder)
        {
            var nameBuilder = new StringBuilder(Name);
            var valueBuilder = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(Type))
                nameBuilder
                    .Append(" - ")
                    .Append(Type);

            valueBuilder.Append($"[Link]({Url})");

            if (!string.IsNullOrWhiteSpace(Description))
                valueBuilder
                    .Append(" - ")
                    .Append(Sanitise(Description));

            builder.AddField(f =>
            {
                f.Name = nameBuilder.ToString();
                f.Value = valueBuilder.ToString();
            });
        }

        private static string Sanitise(string text)
        {
            return text
                .Replace("\r", "")
                .Replace('\n', ' ')
                .Replace('\t', ' ');
        }
    }
}
