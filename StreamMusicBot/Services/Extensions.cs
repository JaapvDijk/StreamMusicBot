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
            var track = query.Split("/").Last();
            var trackId = track.Split("?").First();

            var spotify = new SpotifyClient("");
            var spotifyTrack = await spotify.Tracks.Get(trackId);

            var result = await lavaNode.SearchYouTubeAsync(spotifyTrack.Name);

            return result;
        }

        //Task<List<Victoria.Responses.Search.SearchResponse>>
        public static async Task<string> SearchSpotifyPlaylistAsync(this LavaNode lavaNode, string query)
        {
            var playlist = query.Split("/").Last();
            var playlistId = playlist.Split("?").First();

            var spotify = new SpotifyClient("");
            try
            {
                var result = await spotify.Playlists.Get(playlistId);
                var playlistTracks = result.Tracks.Items.Select
                (
                    x => lavaNode.SearchYouTubeAsync(x.Track)
                );
            }
            catch (Exception e)
            {
                //
            }
            //var spotifyPlaylist = await spotify.Tracks.GetSeveral(new TracksRequest(new List<string> {
            //  "1s6ux0lNiTziSrd7iUAADH",
            //  "6YlOxoHWLjH6uVQvxUIUug"
            //}));

            //var result = await lavaNode.SearchYouTubeAsync(spotifyTrack.Name);

            return "";
        }

        public static LavaTrack GetFirstLavaTrack(this Victoria.Responses.Search.SearchResponse searchResponse)
        {
            return searchResponse.Tracks.FirstOrDefault();
        }
    }
}
