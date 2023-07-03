using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;
using Victoria.EventArgs;
using StreamMusicBot.Extensions;
using Microsoft.Extensions.Logging;

namespace StreamMusicBot.Services
{
    public class MusicService
    {
        private readonly LavaNode _lavaRestClient;
        private readonly ILogger _logger;
        private IConfiguration _config;
        private FavoritesService _favoritesService;
        private TrackFactory _trackFactory;

        public MusicService(LavaNode lavaRestClient,
                            FavoritesService favoritesService,
                            TrackFactory trackFactory,
                            ILogger<MusicService> logger,
                            IConfiguration config)
        {
            _config = config;
            _lavaRestClient = lavaRestClient;
            _logger = logger;
            _favoritesService = favoritesService;
            _trackFactory = trackFactory;

            _lavaRestClient.OnLog += LogAsync;
            _lavaRestClient.OnTrackEnded += TrackFinished;
        }

        public async Task LeaveAsync(SocketVoiceChannel voiceChannel)
            => await _lavaRestClient.LeaveAsync(voiceChannel);

        public async Task ConnectAsync(SocketVoiceChannel voiceChannel)
            => await _lavaRestClient.JoinAsync(voiceChannel);

        public async Task<string> PlayAsync(string query, SocketCommandContext context, SocketVoiceChannel voiceChannel)
        {
            await ConnectAsync(voiceChannel);

            var _player = _lavaRestClient.GetPlayer(context.Guild);

            var tracks = await _trackFactory.GetTrack(query);

            foreach (var track in tracks)
            {
                var isPlaying = _player.PlayerState.Equals(PlayerState.Playing);
                if (isPlaying)
                    _player.Queue.Enqueue(track);
                else
                    await _player.PlayAsync(track);
            }

            return $"Added {tracks.Count()} song(s) \n " +
                $"{await NowPlayingAsync(context.Guild)}";
        }

        public async Task<string> StopAsync(IGuild guild)
        {
            var _player = _lavaRestClient.GetPlayer(guild);
            if (_player is null)
                return "Error with Player";
            await _player.StopAsync();
            return "Music Playback Stopped.";
        }

        public async Task<string> SkipAsync(IGuild guild, int amount = 1)
        {
            var _player = _lavaRestClient.GetPlayer(guild);
            var nrTrackInQueue = _player.Queue.Count;

            amount = (amount <= nrTrackInQueue) ? amount : nrTrackInQueue;

            if (_player is null || _player.Queue.Count() is 0)
                return "Nothing in queue.";

            for (var i = 0; i < amount; i++) await _player.SkipAsync();

            return $"Skipped: **{amount}** track(s) \nNext: {_player.Track.Title}";
        }

        public async Task<string> SetVolumeAsync(ushort vol, IGuild guild)
        {
            const int min = 2;
            const int max = 100;

            var _player = _lavaRestClient.GetPlayer(guild);
            if (_player is null)
                return "Player isn't playing.";

            if (vol < min || vol > max)
            {
                return $"Please use a number between {min} - {max}";
            }

            await _player.UpdateVolumeAsync(vol);
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

            var _tracks = await NowPlayingAsync(guild);

            var i = 1;
            foreach (var track in _player.Queue)
            {
                _tracks += $"\n **[{i}.]** [**{track.Duration.ToHumanReadableString()}**] {track.Title}";
                i++;
            }

            return _tracks;
        }

        public async Task<string> NowPlayingAsync(IGuild guild)
        {
            var _player = _lavaRestClient.GetPlayer(guild);
            var _track = _player.Track;

            var _isPlaying = _player.PlayerState.Equals(PlayerState.Playing);

            var _nowPlaying =
                $"**[Current]** " +
                $"[**{_track.Position.ToHumanReadableString()} | " +
                $"{_track.Duration.ToHumanReadableString()}**] " +
                $"{_track.Title}";

            return (_isPlaying) ? $"{_nowPlaying} \n" : "I'm not playing any music";
        }

        public async Task<string> ForwardAsync(IGuild guild, int seconds)
        {
            var _player = _lavaRestClient.GetPlayer(guild);
            var _remaining = _player.Track.Duration.Subtract(_player.Track.Position);

            var _secondsForward = (seconds < _remaining.TotalSeconds) ? seconds : _remaining.TotalSeconds;
            var _newPosition = _player.Track.Position.Add(new TimeSpan(0, 0, (int)_secondsForward));

            await _player.SeekAsync(_newPosition);

            var nowPlaying = await NowPlayingAsync(guild);
            return $"Forwarded {_secondsForward} seconds \n {nowPlaying}";
        }

        public async Task<string> FavoritesAsync(string operation, string query)
        {
            var track = await _trackFactory.GetTrack(query);

            if (operation.Equals("", StringComparison.OrdinalIgnoreCase))
            {
                return _favoritesService.GetFavorites();
            }

            else if (operation.Equals("add", StringComparison.OrdinalIgnoreCase))
            {
                return _favoritesService.AddFavorite(track.Single());
            }

            else if (operation.Equals("remove", StringComparison.OrdinalIgnoreCase))
            {
                return _favoritesService.RemoveFavorite(track.Single());
            }

            else if (operation.Equals("clear", StringComparison.OrdinalIgnoreCase))
            {
                return _favoritesService.ClearFavorites();
            }

            return "I dont quite understand that mi lord";
        }
        
        public async Task<string> HelpAsyc(IGuild guild, string command)
        {
            switch (command.ToLower()) //TODO: move to appsettings
            {
                case "play":
                    return "play a song.\n " +
                           "supported: Spotify, Youtube, Soundcloud)\n" +
                           "Parameters: {url/title: required}";

                case "leave":
                    return "disconnect the bot from the current channel \n" +
                           "Parameters: none";

                case "stop":
                    return "stop playing music \n" +
                           "Parameters: none";

                case "skip":
                    return "start playing next track in the queue \n" +
                           "Parameters: {amount: optional}";

                case "volume":
                    return "set the play volume of the bot (between 2-100) \n" +
                           "Parameters: {percentage: required}";

                case "pause":
                    return "pause the current track at the current position \n" +
                           "Parameters: none";

                case "np":
                    return "Show the track that is currently playing \n" +
                           "Parameters: none";

                case "queue":
                    return "Use this command to obtain track queue info and to interact with it \n" +
                           "Parameters: none yet";

                case "forward":
                    return "Forward the current track for a specified number of seconds \n" +
                           "Parameters: {seconds: required} \n" +
                           "Additional info: 'forward -20', track will go back 20 seconds";

                case "favorites":
                    return "Manage your favorite tracks \n" +
                           "Parameters: {remove_add} {track} \n" +
                           "Samples info: 'favorites \n favorites add ikwilkaas \n favorites remove 1";

                default:
                    return "'help {command_name}' \n" +
                           "The following commands are available: \n" +
                           "Play, Leave, Stop, Skip, Volume, Pause, np, Forward, Queue, Favorites";

            }
        }

        private async Task TrackFinished(TrackEndedEventArgs args)
        {
            //TODO: check args.Reason value
            if (!args.Reason.Equals(TrackEndReason.Finished))
            {
                return;
            }
            var _player = args.Player;

            await _player.SkipAsync();

            var _nextTrack = _player.Queue.Peek();
            await _player.PlayAsync(_nextTrack);
        }

        private async Task LogAsync(LogMessage logMessage)
        {
            await Task.Run(() => _logger.LogDebug(logMessage.Message));
        }
    }
}