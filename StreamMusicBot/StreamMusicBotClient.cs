using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace StreamMusicBot
{
    public class StreamMusicBotClient
    {
        private readonly DiscordSocketClient _discordClient;
        private readonly CommandService _cmdService;
        private IServiceProvider _services;

        private readonly ILogger _logger;
        private IConfiguration _config;

        public StreamMusicBotClient(IConfiguration config,
                                    ILogger<StreamMusicBotClient> logger,
                                    IServiceProvider services,
                                    DiscordSocketClient discordClient,
                                    CommandService cmdService)
        {
            _config = config;
            _logger = logger;
            _services = services;
            _discordClient = discordClient;
            _cmdService = cmdService;
        }

        public async Task InitializeAsync()
        {
            await _discordClient.LoginAsync(TokenType.Bot, _config["token"]);
            await _discordClient.StartAsync();
            _discordClient.Log += LogAsync;

            var cmdHandler = new CommandHandler(_discordClient, _cmdService, _services);
            await cmdHandler.InitializeAsync();

            //?
            await Task.Delay(-1);
        }

        private async Task LogAsync(LogMessage logMessage)
        {
            await Task.Run(() => _logger.LogDebug(logMessage.Message));
        }
    }
}
