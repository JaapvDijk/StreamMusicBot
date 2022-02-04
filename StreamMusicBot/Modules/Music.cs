using Discord.Commands;
using Discord.WebSocket;
using StreamMusicBot.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StreamMusicBot.Modules
{
    public class Music : ModuleBase<SocketCommandContext>
    {
        private MusicService _musicService;

        public Music(MusicService musicService)
        {
            _musicService = musicService;
        }

        [Command("Leave")]
        public async Task Leave()
        {
            var user = Context.User as SocketGuildUser;
            if (user.VoiceChannel is null)
            {
                await ReplyAsync("Please join the channel the bot is in to make it leave.");
            }
            else
            {
                await _musicService.LeaveAsync(user.VoiceChannel);
                await ReplyAsync($"Bot has now left {user.VoiceChannel.Name}");
            }
        }

        [Command("Join")]
        public async Task Join()
        {
            var user = Context.User as SocketGuildUser;
            if (user.VoiceChannel is null)
            {
                await ReplyAsync("You need to connect to a voice channel.");
                return;
            }
            else
            {
                await _musicService.ConnectAsync(user.VoiceChannel);
                await ReplyAsync($"Joined {user.VoiceChannel.Name} dankzij die {user.Username}");
            }
        }

        [Command("Play")]
        public async Task Play([Remainder] string query)
        {
            var user = Context.User as SocketGuildUser;
            if (user.VoiceChannel is null)
            {
                await ReplyAsync("You need to connect to a voice channel.");
                return;
            }
            else
            {
                await ReplyAsync(await _musicService.PlayAsync(query, Context, user.VoiceChannel)); ;
            }
        }

        [Command("Stop")]
        public async Task Stop()
            => await ReplyAsync(await _musicService.StopAsync(Context.Guild));

        [Command("Skip")]
        public async Task Skip(int amount = 1)
        {
            await ReplyAsync(await _musicService.SkipAsync(Context.Guild, amount));
        }

        [Command("Volume")]
        public async Task Volume(ushort vol)
            => await ReplyAsync(await _musicService.SetVolumeAsync(vol, Context.Guild));

        [Command("Pause")]
        public async Task Pause()
            => await ReplyAsync(await _musicService.PauseOrResumeAsync(Context.Guild));

        [Command("Resume")]
        public async Task Resume()
            => await ReplyAsync(await _musicService.ResumeAsync(Context.Guild));

        [Command("Queue")]
        public async Task Queue()
            => await ReplyAsync(await _musicService.QueueAsync(Context.Guild));

        [Command("np")]
        public async Task NowPlaying()
            => await ReplyAsync(await _musicService.NowPlayingAsync(Context.Guild));

        [Command("Forward")]
        public async Task FastForward(int seconds)
            => await ReplyAsync(await _musicService.ForwardAsync(Context.Guild, seconds));

        [Command("Help")]
        public async Task Help(string command = "")
            => await ReplyAsync(await _musicService.HelpAsyc(Context.Guild, command));

        [Command("Favorites")]
        public async Task Favorites(string operation = "", [Remainder] string query = "")
            => await ReplyAsync(await _musicService.FavoritesAsync(operation, query));
    }
}
