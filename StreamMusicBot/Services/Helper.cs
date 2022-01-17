using System.Linq;

namespace StreamMusicBot
{
    public static class Helper
    {
        public static string getSpotifyID(string query) 
        {
            var lastParam = query.Split("/").Last();
            var Id = lastParam.Split("?").First();

            return Id;
        }
    }
}
