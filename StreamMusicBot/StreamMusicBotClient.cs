using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using StreamMusicBot.Entities;
using StreamMusicBot.Services;
using System;
using System.Threading.Tasks;
using Victoria;

namespace StreamMusicBot
{
    public class StreamMusicBotClient
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _cmdService;
        private IServiceProvider _services;
        private readonly LogService _logService;
        private readonly ConfigService _configService;
        private readonly Config _config;

        public StreamMusicBotClient()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                AlwaysDownloadUsers = true,
                MessageCacheSize = 50,
                LogLevel = LogSeverity.Debug
            });

            _cmdService = new CommandService(new CommandServiceConfig
            {
                LogLevel = LogSeverity.Verbose,
                CaseSensitiveCommands = false
            });

            _logService = new LogService();
            _configService = new ConfigService();
            _config = _configService.GetConfig();
        }

        public async Task InitializeAsync()
        {
            await _client.LoginAsync(TokenType.Bot, "OTE4NTkwMTU5MzEyODE0MDgw.YbJdwA.0wiwKrAj1ykWc1tmaD5j1-o7DMY");
            await _client.StartAsync();
            _client.Log += LogAsync;
            _services = SetupServices();

            var cmdHandler = new CommandHandler(_client, _cmdService, _services);
            await cmdHandler.InitializeAsync();

            //?
            await Task.Delay(-1);
        }

        private async Task LogAsync(LogMessage logMessage)
        {
            await _logService.LogAsync(logMessage);
        }

        private IServiceProvider SetupServices()
        {
            return new ServiceCollection()
              .AddSingleton(_client)
              .AddSingleton(_cmdService)
              .AddSingleton(_logService)
              .AddSingleton<MusicService>()
              .AddLavaNode(x => { x.SelfDeaf = false; })
              .BuildServiceProvider();
        }
    }
}
