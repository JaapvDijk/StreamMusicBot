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

        //public static SpotifyUrlDetails getSpotifyID(string query)
        //{
        //}
    }
}
