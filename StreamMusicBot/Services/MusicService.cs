using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Linq;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;
using Victoria.EventArgs;

namespace StreamMusicBot.Services
{
    public class MusicService
    {
        private readonly LavaNode _lavaRestClient;
        private readonly DiscordSocketClient _client;
        private readonly LogService _logService;

        public MusicService(LavaNode lavaRestClient, 
                            DiscordSocketClient client, 
                            LogService logService)
        {
            _client = client;
            _lavaRestClient = lavaRestClient;
            _logService = logService;

            //TODO: Add events..
            _lavaRestClient.OnTrackEnded += TrackFinished;
        }

        public async Task LeaveAsync(SocketVoiceChannel voiceChannel)
            => await _lavaRestClient.LeaveAsync(voiceChannel);

        public async Task ConnectAsync(SocketVoiceChannel voiceChannel, ITextChannel textChannel)
            => await _lavaRestClient.JoinAsync(voiceChannel, textChannel);

        public async Task<string> PlayAsync(string query, SocketCommandContext context, SocketVoiceChannel voiceChannel)
        {
            await _lavaRestClient.ConnectAsync();

            var _player = _lavaRestClient.GetPlayer(context.Guild);
            var results = await _lavaRestClient.SearchYouTubeAsync(query);
               
            //TODO: no matches (LoadType notfound)
            if (false) //results.LoadType == LoadType.NoMatches || results.LoadType == LoadType.LoadFailed
            {
                return "No matches found.";
            }

            var track = results.Tracks.FirstOrDefault();

            if (_player.PlayerState.Equals(PlayerState.Paused))
            {
                _player.Queue.Enqueue(track);
                return $"{track.Title} has been added to the queue.";
            }
            else
            {
                await _player.PlayAsync(track);
                return $"Now Playing: {track.Title}";
            }
        }

        public async Task<string> StopAsync(IGuild guild)
        {
            var _player = _lavaRestClient.GetPlayer(guild);
            if (_player is null)
                return "Error with Player";
            await _player.StopAsync();
            return "Music Playback Stopped.";
        }

        public async Task<string> SkipAsync(IGuild guild)
        {
            var _player = _lavaRestClient.GetPlayer(guild);
            if (_player is null || _player.Queue.Count() is 0)
                return "Nothing in queue.";

            var oldTrack = _player.Track;
            await _player.SkipAsync();
            return $"Skiped: {oldTrack.Title} \nNow Playing: {_player.Track.Title}";
        }

        public async Task<string> SetVolumeAsync(int vol, IGuild guild)
        {
            var _player = _lavaRestClient.GetPlayer(guild);
            if (_player is null)
                return "Player isn't playing.";

            if (vol > 100 || vol <= 2)
            {
                return "Please use a number between 2 - 100";
            }

            //TODO: setvolume (volume is readonly)
            //_player.Volume = vol;
            return $"Volume set to: {vol}";
        }

        public async Task<string> PauseOrResumeAsync(IGuild guild)
        {
            var _player = _lavaRestClient.GetPlayer(guild);
            if (_player is null)
                return "Player isn't playing.";

            if (_player.PlayerState.Equals(PlayerState.Paused))
            {
                await _player.PauseAsync();
                return "Player is Paused.";
            }
            else
            {
                await _player.ResumeAsync();
                return "Playback resumed.";
            }
        }

        public async Task<string> ResumeAsync(IGuild guild)
        {
            var _player = _lavaRestClient.GetPlayer(guild);
            if (_player is null)
                return "Player isn't playing.";

            if (_player.PlayerState.Equals(PlayerState.Paused))
            {
                await _player.ResumeAsync();
                return "Playback resumed.";
            }

            return "Player is not paused.";
        }

        public async Task<string> QueueAsync(IGuild guild)
        {
            var _player = _lavaRestClient.GetPlayer(guild);
            if (_player is null || _player.Queue.Count() is 0)
                return "Nothing in queue.";

            var tracks = $@"Current: {_player.Track.Title}";
            foreach (var track in _player.Queue) tracks += track.Title;

            return tracks;
        }

        private async Task TrackFinished(TrackEndedEventArgs args)
        {
            //TODO: check args.Reason value
            if (!args.Reason.Equals(TrackEndReason.Finished))
            {
                return;
            }
            //await player.PlayAsync(nextTrack);
        }

        private async Task LogAsync(LogMessage logMessage)
        {
            await _logService.LogAsync(logMessage);
        }
    }
}
