﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
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
        private readonly LogService _logService;

        public MusicService(LavaNode lavaRestClient, 
                            LogService logService)
        {
            _lavaRestClient = lavaRestClient;
            _logService = logService;

            //TODO: Add events..
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
            var results = await _lavaRestClient.SearchYouTubeAsync(query);

            //TODO: no matches (LoadType notfound)
            if (false) //results.LoadType == LoadType.NoMatches || results.LoadType == LoadType.LoadFailed
            {
                return "No matches found.";
            }

            var track = results.Tracks.FirstOrDefault();

            if (_player.PlayerState.Equals(PlayerState.Playing))
            {
                _player.Queue.Enqueue(track);
                return $"{track.Title} has been added to the queue. -Position **[{_player.Queue.Count()}]**";
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

            return (_isPlaying) ? $"**[Current]** " +
                $"[**{_track.Position.ToHumanReadableString()} - " +
                $"{_track.Duration.ToHumanReadableString()}**] " +
                $"{_track.Title} \n" : 
                "I'm not playing any music";
        }

        public async Task<string> ForwardAsync(IGuild guild, int seconds)
        {
            var _player = _lavaRestClient.GetPlayer(guild);
            var _remaining = _player.Track.Duration.Subtract(_player.Track.Position);

            var _secondsForward = (seconds < _remaining.TotalSeconds) ? seconds : _remaining.TotalSeconds;
            var _newPosition = _player.Track.Position.Add(new TimeSpan(0,0, (int)_secondsForward));

            await _player.SeekAsync(_newPosition);

            return $"Forwarded {_secondsForward} seconds \n" +
                await NowPlayingAsync(guild);
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
            await _logService.LogAsync(logMessage);
        }
    }
}
