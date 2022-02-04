using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Victoria;
using StreamMusicBot.Services;

namespace StreamMusicBot.Extensions
{
    public static class Extensions
    {
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

        public static async Task<Victoria.Responses.Search.SearchResponse> SearchSpotifyAsync(this LavaNode lavaNode, string query, SpotifyClient spotify)
        {
            var trackId = Helper.getSpotifyID(query);
            var track = await spotify.Tracks.Get(trackId);
            var trackName = $"{track.Name} {track.Album.Name}";

            var result = await lavaNode.SearchYouTubeAsync(trackName);

            return result;
        }

        public static async Task<List<Victoria.Responses.Search.SearchResponse>> SearchSpotifyPlaylistAsync(this LavaNode lavaNode, string query, SpotifyClient spotify)
        {
            var playlistId = Helper.getSpotifyID(query);

            var spotifyResult = await spotify.Playlists.Get(playlistId); //Note: is max 100 tracks

            var tasks = spotifyResult.Tracks.Items.Select(playable =>
            {
                var track = (FullTrack)playable.Track;
                var trackName = $"{track.Name} {track.Album.Name}";

                return lavaNode.SearchYouTubeAsync(track.Name);
            });
            
            return (await Task.WhenAll(tasks)).ToList();
        }

        public static LavaTrack GetFirstLavaTrack(this Victoria.Responses.Search.SearchResponse searchResponse)
        {
            return searchResponse.Tracks.FirstOrDefault();
        }

        public static IEnumerable<LavaTrack> GetFirstLavaTracks(this IEnumerable<Victoria.Responses.Search.SearchResponse> searchResponseList)
        {
            var lavatrackList = searchResponseList.Select(x => x.Tracks.FirstOrDefault());
            return lavatrackList;
        }
    }
}
