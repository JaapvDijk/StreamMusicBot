﻿using System.Threading.Tasks;
using Victoria;
using StreamMusicBot.Extensions;
using System.Collections.Generic;

namespace StreamMusicBot.Services
{
    public class TrackFactory
    {
        private readonly LavaNode _lavaNode;
        private readonly LogService _logService;
        private readonly SpotifyService _spotifyService;

        public TrackFactory(LavaNode lavaNode,
                            LogService logService,
                            SpotifyService spotifyService)
        {
            _lavaNode = lavaNode;
            _logService = logService;
            _spotifyService = spotifyService;
        }

        public async Task<IEnumerable<LavaTrack>> GetTrack(string query)
        {
            var tracks = new List<LavaTrack>();

            switch (query)
            {
                case var a when a.Contains("open.spotify") && a.Contains("track"):
                    var trackReponse = await _lavaNode.SearchSpotifyAsync(query, await _spotifyService.GetClient());
                    tracks.Add(trackReponse.GetFirstLavaTrack());
                    return tracks;

                case var a when a.Contains("open.spotify") && a.Contains("playlist"):
                    var playListReponse = await _lavaNode.SearchSpotifyPlaylistAsync(query, await _spotifyService.GetClient());
                    tracks.AddRange(playListReponse.GetFirstLavaTracks());
                    return tracks;

                case var a when a.Contains("soundcloud."):
                    trackReponse = await _lavaNode.SearchSoundCloudAsync(query);
                    tracks.Add(trackReponse.GetFirstLavaTrack());
                    return tracks;

                default:
                    trackReponse = await _lavaNode.SearchYouTubeAsync(query);
                    tracks.Add(trackReponse.GetFirstLavaTrack());
                    return tracks;
            }
        }

    }
}
