﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StreamMusicBot.Services;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace StreamMusicBot
{
    public class StreamMusicBotClient
    {
        private readonly DiscordSocketClient _discordClient;
        private readonly CommandService _cmdService;
        private IServiceProvider _services;

        private readonly ILogger _logger;
        private IConfiguration _config;
        private MusicService _musicService;

        public StreamMusicBotClient(IConfiguration config,
                                    ILogger<StreamMusicBotClient> logger,
                                    IServiceProvider services,
                                    DiscordSocketClient discordClient,
                                    CommandService cmdService,
                                    MusicService musicService)
        {
            _config = config;
            _logger = logger;
            _services = services;
            _discordClient = discordClient;
            _cmdService = cmdService;
            _musicService = musicService;
        }

        public async Task InitializeAsync()
        {
            await _discordClient.LoginAsync(TokenType.Bot, _config["discord_token"]);
            await _discordClient.StartAsync();
            _discordClient.Log += (logMessage) => Task.Run(() => _logger.LogDebug($"Discord client: {logMessage.Message}"));
            _discordClient.UserVoiceStateUpdated += LeaveEmptyVoicechannel;
            _discordClient.UserVoiceStateUpdated += FollowUser;

            var cmdHandler = new CommandHandler(_discordClient, _cmdService, _services);
            await cmdHandler.InitializeAsync();

            //?
            await Task.Delay(-1);
        }

        //Let bot leave from an empty voicechannel
        private async Task LeaveEmptyVoicechannel(SocketUser socketUser, SocketVoiceState oldVoiceState, SocketVoiceState newVoiceState)
        {
            if (!socketUser.IsBot)
            {
                var user = _discordClient.GetUser(socketUser.Id);

                var oldUserVoice = oldVoiceState.VoiceChannel;
                var newUserVoice = newVoiceState.VoiceChannel;

                var id = _discordClient.CurrentUser.Id;
                var bot = (oldUserVoice != null) ? 
                            oldUserVoice.Guild.GetUser(id) : 
                            newUserVoice.Guild.GetUser(id);
                var botVoice = bot.VoiceChannel;

                var wasInSameVoice = oldUserVoice != null && 
                                     oldUserVoice.Users.Any((user) => user.Id == bot.Id);
                var voiceIsEmpty = oldUserVoice != null && 
                                  !oldUserVoice.Users.Where((user) => !user.IsBot).Any();

                if (wasInSameVoice && voiceIsEmpty)
                {
                    await _musicService.LeaveAsync(botVoice);

                    _logger.LogInformation($"{bot.Username} left {botVoice.Name} (voicehannel is empty)");
                }
            }
        }

        private async Task FollowUser(SocketUser socketUser, SocketVoiceState oldVoiceState, SocketVoiceState newVoiceState)
        {

        }
        }
}
