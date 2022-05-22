namespace MasonMusicBot
{
    using System;
    using System.Linq;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.Interactivity;
    using Microsoft.Extensions.Configuration;
    using DSharpPlus.Interactivity.Extensions;
    using System.Collections;
    using System.Collections.Generic;
    using DSharpPlus.EventArgs;
    using MasonMusicBot.Commands;
    internal class Bot
    {
        public DiscordClient Client { get; set; }

        public CommandsNextExtension Commands { get; set; }

        public async Task Run()
        {
            IConfigurationRoot configFile = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json", optional: false, reloadOnChange: true)
                .Build();


            var config = new DiscordConfiguration {

                Token = configFile.GetValue<string>("discord:token"),
                TokenType = TokenType.Bot,
                AutoReconnect = true

            };
            Client = new DiscordClient(config);

            Client.Ready += OnClientReady;

            var commandsConfig = new CommandsNextConfiguration {
                StringPrefixes = new string[] { configFile.GetValue<string>("discord:CommandPrefix") },
                EnableDms = false,
                EnableMentionPrefix = true,

            };

            Commands = Client.UseCommandsNext(commandsConfig);

            Commands.RegisterCommands<BaseCommands>();


            await Client.ConnectAsync();

            await Task.Delay(-1);
        }

        private Task OnClientReady(DiscordClient sender, ReadyEventArgs e)
        {
            return Task.CompletedTask;
        }
    }
}
