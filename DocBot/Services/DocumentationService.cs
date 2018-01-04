using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using DocBot.Services.Documentation;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DocBot.Services
{
    public class DocumentationService
    {
        private readonly CommandService commands;
        private readonly IConfigurationRoot config;
        private readonly LoggingService logger;
        private readonly DocumentationCacheService cache;
        private readonly IServiceProvider provider;

        public DocumentationService(CommandService commands, IConfigurationRoot config, LoggingService logger, DocumentationCacheService cache, IServiceProvider provider)
        {
            this.commands = commands;
            this.config = config;
            this.logger = logger;
            this.cache = cache;
            this.provider = provider;
        }

        public async Task StartAsync()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsClass && t.Namespace == "DocBot.Services.Documentation.Providers" && t.IsSubclassOf(typeof(DocumentationProvider)));

            // The module alias is left empty as all commands inside the module will be prefixed with it
            await commands.CreateModuleAsync("", async builder =>
            {
                foreach (var type in types)
                {
                    var instance = (DocumentationProvider)Activator.CreateInstance(type, provider.GetRequiredService<HtmlWeb>());

                    builder.AddCommand(instance.Aliases[0], async (context, args, provider, commandInfo) =>
                        await FindInDocs(instance, (string)args[0], context), cmdBuilder =>
                    {
                        cmdBuilder
                            .AddAliases(instance.Aliases.Skip(1).ToArray())
                            .WithSummary($"Search the {instance.FriendlyName}")
                            .AddParameter<string>("query", paramBuilder =>
                            {
                                paramBuilder
                                    .WithIsRemainder(true)
                                    .WithSummary("Search query");
                            });
                    });

                    await logger.LogDebug($"Found documentation provider {type.Name}\n\t" +
                                          $"Friendly name: {instance.FriendlyName}\n\t" +
                                          $"Aliases ({instance.Aliases.Length}): {string.Join(", ", instance.Aliases)}\n\t" +
                                          $"Search URL: {instance.SearchURLFormat}\n\t" +
                                          $"Base URL: {instance.BaseURL}\n\t" +
                                          $"Cache TTL: {instance.CacheTTL:d' days 'hh':'mm':'ss}", "DocumentationService");
                }
            });
        }

        private async Task FindInDocs(DocumentationProvider docProvider, string query, ICommandContext context)
        {
            var articles = cache.Get(query);

            if (articles == null)
            {
                articles = await docProvider.SearchArticles(query);
                cache.Add(query, articles, docProvider.CacheTTL);
            }

            var builder = new EmbedBuilder()
                .WithColor(100, 149, 237);

            foreach (var article in articles)
                article.AddToEmbed(builder);

            await context.Channel.SendMessageAsync("", embed: builder.Build());
        }
    }
}
