using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using DocBot.Services.Documentation;
using Microsoft.Extensions.Configuration;

namespace DocBot.Services
{
    public class DocumentationService
    {
        public IReadOnlyList<DocumentationProvider> DocumentationProviders => documentationProviders.AsReadOnly();

        private readonly CommandService commands;
        private readonly IConfigurationRoot config;
        private readonly LoggingService logger;
        private readonly DocumentationCacheService cache;
        private readonly IServiceProvider provider;
        private readonly List<DocumentationProvider> documentationProviders;

        public DocumentationService(CommandService commands, IConfigurationRoot config, LoggingService logger, DocumentationCacheService cache, IServiceProvider provider)
        {
            this.commands = commands;
            this.config = config;
            this.logger = logger;
            this.cache = cache;
            this.provider = provider;

           documentationProviders = new List<DocumentationProvider>();
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
                    var instance = (DocumentationProvider)Activator.CreateInstance(type, provider);
                    documentationProviders.Add(instance);

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
            var builder = new EmbedBuilder()
                .WithColor(100, 149, 237)
                .WithDescription($"Searching the {docProvider.FriendlyName}...");
            var msg = await context.Channel.SendMessageAsync("", embed: builder.Build());

            var timer = Stopwatch.StartNew();
            var articles = cache.Get(docProvider.FriendlyName, query);
            var fromCache = true;

            if (articles != null)
                await logger.LogDebug("Query found in cache", "DocumentationService");
            else
            {
                fromCache = false;
                await logger.LogDebug("Query not in cache", "DocumentationService");
                try
                {
                    articles = await docProvider.SearchArticles(query);
                }
                catch (Exception ex)
                {
                    await logger.LogError("Query failed",
                        "DocumentationService", ex);

                    builder = new EmbedBuilder()
                        .WithColor(100, 149, 237)
                        .WithDescription("I couldn't find anything because of an error :(");
                    await msg.ModifyAsync(f => f.Embed = builder.Build());
                    throw;
                }
                finally
                {
                    timer.Stop();
                    await logger.LogVerbose($"Query completed in {Math.Round(timer.Elapsed.TotalMilliseconds)}ms",
                        "DocumentationService");
                }
                await cache.Add(docProvider.FriendlyName, query, articles, docProvider.CacheTTL);
            }

            await logger.LogDebug($"{articles?.Count ?? 0} articles found", "DocumentationService");

            builder = new EmbedBuilder()
                .WithColor(100, 149, 237)
                .WithFooter(
                fromCache
                    ? $"This query was retrieved from the cache in {Math.Round(timer.Elapsed.TotalMilliseconds)}ms"
                    : $"This query completed in {Math.Round(timer.Elapsed.TotalMilliseconds)}ms");

            if (!articles?.Any() ?? true)
                builder.WithDescription("No results");
            else
            {
                foreach (var article in articles.Take(3))
                    article.AddToEmbed(builder);
            }

            await msg.ModifyAsync(f => f.Embed = builder.Build());
        }
    }
}
