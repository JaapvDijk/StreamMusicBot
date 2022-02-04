using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        private IConfiguration _config;

        public StreamMusicBotClient()
        {
            _config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();
            //.AddUserSecrets<StreamMusicBotClient>(optional: true)
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                AlwaysDownloadUsers = true,
                MessageCacheSize = 25,
                LogLevel = LogSeverity.Debug
            });

            _cmdService = new CommandService(new CommandServiceConfig
            {
                LogLevel = LogSeverity.Verbose,
                CaseSensitiveCommands = false
            });

            _logService = new LogService();
        }

        public async Task InitializeAsync()
        {
            await _client.LoginAsync(TokenType.Bot, _config["token"]);
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
              .AddSingleton<IConfiguration>(_config)
              .AddSingleton<FavoritesService>()
              .AddLavaNode(x => { x.SelfDeaf = false; x.Port = 2333; x.Hostname = _config["lavahostname"]; })
              .AddSingleton<TrackFactory>()
              .AddSingleton<SpotifyService>()
              .BuildServiceProvider();
        }
    }
}
