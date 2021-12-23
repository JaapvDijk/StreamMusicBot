using Discord;
using Discord.Commands;
using Discord.WebSocket;
using StreamMusicBot.Services;
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
                await ReplyAsync(await _musicService.PlayAsync(query, Context, user.VoiceChannel));;
            }
        }

        [Command("Stop")]
        public async Task Stop()
            => await ReplyAsync(await _musicService.StopAsync(Context.Guild.Id));

        [Command("Skip")]
        public async Task Skip()
            => await ReplyAsync(await _musicService.SkipAsync(Context.Guild.Id));

        [Command("Volume")]
        public async Task Volume(int vol)
            => await ReplyAsync(await _musicService.SetVolumeAsync(vol, Context.Guild.Id));

        [Command("Pause")]
        public async Task Pause()
            => await ReplyAsync(await _musicService.PauseOrResumeAsync(Context.Guild.Id));

        [Command("Resume")]
        public async Task Resume()
            => await ReplyAsync(await _musicService.ResumeAsync(Context.Guild.Id));

        [Command("Queue")]
        public async Task Queue()
            => await ReplyAsync(await _musicService.QueueAsync(Context.Guild.Id));
    }
}
