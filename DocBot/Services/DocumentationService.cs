using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Commands;
using DocBot.Services.Documentation;
using Microsoft.Extensions.Configuration;

namespace DocBot.Services
{
    public class DocumentationService
    {
        private readonly CommandService commands;
        private readonly IConfigurationRoot config;
        private readonly LoggingService logger;

        public DocumentationService(CommandService commands, IConfigurationRoot config, LoggingService logger)
        {
            this.commands = commands;
            this.config = config;
            this.logger = logger;
        }

        public async Task StartAsync()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsClass && t.Namespace == "DocBot.Services.Documentation.Providers" && t.IsSubclassOf(typeof(DocumentationProvider)));

            await commands.CreateModuleAsync("", async builder =>
            {
                foreach (var type in types)
                {
                    var instance = (DocumentationProvider)Activator.CreateInstance(type);

                    builder.AddCommand(instance.Aliases[0], async (context, args, provider, commandInfo) =>
                    {
                        var query = (string)args[0];
                        await context.Channel.SendMessageAsync(
                            $"You searched for {query} on the {instance.FriendlyName}");
                    }, cmdBuilder =>
                    {
                        cmdBuilder
                            .AddAliases(instance.Aliases.Skip(1).ToArray())
                            .WithSummary($"Search the {instance.FriendlyName}")
                            .AddParameter<string>("query", paramBuilder =>
                            {
                                paramBuilder
                                    .WithIsRemainder(true)
                                    .WithSummary($"Search query");
                            });
                    });

                    await logger.LogDebug($"Found documentation provider {type.Name}\n\t" +
                                          $"Friendly name: {instance.FriendlyName}\n\t" +
                                          $"Aliases ({instance.Aliases.Length}): {string.Join(", ", instance.Aliases)}\n\t" +
                                          $"Search URL: {instance.SearchURLFormat}", "DocumentationService");
                }
            });
        }
    }
}
