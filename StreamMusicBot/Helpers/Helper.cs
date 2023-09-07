using System.Linq;

namespace StreamMusicBot.Helpers
{
    public static class Helper
    {
        public static string getSpotifyID(string query)
        {
            var lastParam = query.Split("/").Last();
            var Id = lastParam.Split("?").First();

            return Id;
        }


        public static bool isSoundCloud(string url)
        {
            return url.Contains("soundcloud.");
        }

        public static bool isSpotifyTrack(string url)
        {
            return isSpotify(url) && url.Contains("track");
        }

        public static bool isSpotifyPlaylist(string url)
        {
            return isSpotify(url) && url.Contains("playlist");
        }

        private static bool isSpotify(string url) 
        {
            return url.Contains("open.spotify");
        }

        //public static SpotifyUrlDetails getSpotifyID(string query)
        //{
        //}
    }
}
