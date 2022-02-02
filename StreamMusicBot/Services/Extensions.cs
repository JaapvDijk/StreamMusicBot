using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Victoria;

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

        public static async Task<Victoria.Responses.Search.SearchResponse> SearchSpotifyAsync(this LavaNode lavaNode, string query)
        {
            var trackId = Helper.getSpotifyID(query);

            var spotify = new SpotifyClient("");
            var spotifyTrack = await spotify.Tracks.Get(trackId);

            var result = await lavaNode.SearchYouTubeAsync(spotifyTrack.Name);

            return result;
        }

        public static async Task<List<Victoria.Responses.Search.SearchResponse>> SearchSpotifyPlaylistAsync(this LavaNode lavaNode, string query)
        {
            var playlistId = Helper.getSpotifyID(query);

            //Needs to be disposed?
            //TODO: to seperate class
            var config = SpotifyClientConfig
              .CreateDefault()
              .WithAuthenticator(new ClientCredentialsAuthenticator("", "")); //client, secret

            var spotify = new SpotifyClient(config);

            //Note: is max 100 tracks
            var spotifyResult = await spotify.Playlists.Get(playlistId);

            var tasks = spotifyResult.Tracks.Items.Select(playable =>
            {
                var track = (FullTrack)playable.Track;
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
