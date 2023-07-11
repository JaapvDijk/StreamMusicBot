using Serilog;
using StreamMusicBot.Helpers;
using StreamMusicBot.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Victoria;
using Victoria.Responses.Search;

namespace StreamMusicBot.MyExtensions
{
    public static class Extensions
    {
        private static SpotifyService _spotifyService;
        public static SpotifyService SpotifyService
        {
            get
            {
                return _spotifyService;
            }
        }

        public static void Initialize(SpotifyService spotifyService)
        {
            _spotifyService = spotifyService;
        }

        public static string ToHumanReadableString(this TimeSpan t)
        {
            if (t.TotalSeconds <= 1)
            {
                return $@"{t:s\.ff} sec";
            }
            if (t.TotalMinutes <= 1)
            {
                return $@"{t:%s} sec";
            }
            if (t.TotalHours <= 1)
            {
                return $@"{t:%m} min";
            }
            if (t.TotalDays <= 1)
            {
                return $@"{t:%h} hr";
            }

            return $@"{t:%d} day(s)";
        }

        public static async Task<SearchResponse> SearchSpotifyAsync(this LavaNode lavaNode, string query)
        {
            var spotifyClient = await _spotifyService.GetClient();

            var trackId = Helper.getSpotifyID(query);
            var track = await spotifyClient.Tracks.Get(trackId);
            var artistNames = String.Join(" ", track.Artists.Select(a => a.Name));

            var trackFullName = $"{artistNames} {track.Album.Name} {track.Name}";

            var result = await lavaNode.SearchYouTubeAsync(trackFullName);

            return result;
        }

        public static async Task<List<SearchResponse>> SearchSpotifyPlaylistAsync(this LavaNode lavaNode, string query)
        {
            var playlistId = Helper.getSpotifyID(query);
            var spotifyClient = await _spotifyService.GetClient();

            var spotifyResult = await spotifyClient.Playlists.Get(Helper.getSpotifyID(query)); //Note: is max 100 tracks
            var tasks = spotifyResult.Tracks.Items.Select(playlistTrack =>
            {
                var track = playlistTrack.Track as SpotifyAPI.Web.FullTrack;

                return lavaNode.SearchSpotifyAsync(track.Href);
            });
            
            var result = (await Task.WhenAll(tasks)).ToList();

            return result;
        }

        public static LavaTrack GetFirstLavaTrack(this SearchResponse searchResponse)
        {
            return searchResponse.Tracks.FirstOrDefault();
        }

        public static IEnumerable<LavaTrack> GetLavaTracks(this IEnumerable<SearchResponse> searchResponseList)
        {
            var lavatrackList = searchResponseList.Select(x => x.Tracks.FirstOrDefault());
            return lavatrackList;
        }
    }
}
