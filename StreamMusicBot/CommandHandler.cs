using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Victoria;

namespace StreamMusicBot
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _cmdService;
        private readonly IServiceProvider _services;
        private readonly LavaNode _lavaNode;

        public CommandHandler(DiscordSocketClient client,
                              CommandService cmdService,
                              IServiceProvider services)
        {
            _client = client;
            _cmdService = cmdService;
            _services = services;

            _lavaNode = (LavaNode)services.GetService(typeof(LavaNode));
            _client.Ready += OnReadyAsync;
        }

        private async Task OnReadyAsync()
        {
            if (!_lavaNode.IsConnected)
            {
                _lavaNode.ConnectAsync();
            }
            // Other ready related stuff
        }

        public async Task InitializeAsync()
        {
            await _cmdService.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            _cmdService.Log += LogAsync;
            _client.MessageReceived += HandleMessageAsync;
        }

        private async Task HandleMessageAsync(SocketMessage socketMessage)
        {
            var argPos = 0;
            var CommandPrefixChar = "!";

            if (socketMessage.Author.IsBot) return;

            var userMessage = socketMessage as SocketUserMessage;
            if (userMessage is null)
                return;

            //TODO "!" prefix
            if (!userMessage.HasMentionPrefix(_client.CurrentUser, ref argPos) &&
                !userMessage.HasStringPrefix(CommandPrefixChar, ref argPos, StringComparison.OrdinalIgnoreCase))
                return;

            var context = new SocketCommandContext(_client, userMessage);
            await _cmdService.ExecuteAsync(context, argPos, _services);
        }

        private Task LogAsync(LogMessage logMessage)
        {
            Console.WriteLine(logMessage.Message);
            return Task.CompletedTask;
        }
    }
}
