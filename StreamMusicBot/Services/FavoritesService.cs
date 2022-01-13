using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamMusicBot.Services
{
    public class FavoritesService
    {
        public List<string> Favorites { get; set; } = new List<string>();

        public string GetFavorites()
        {
            var display = Favorites.Select((x, i) => $"**{i+1}**: {x} \n");

            var result = "Empty";
            if (display.Count() > 0) result = string.Join("", display);

            return result;
        }
        public string AddFavorite(string query) 
        {
            Favorites.Add(query);

            return $"Added {query} to favorites";
        }

        public string RemoveFavorite(string query)
        {
            if (Favorites.Contains(query))
            {
                Favorites.Remove(query);
                return $"Removed: {query} from favorites";
            }
            else
            {
                return $"{query} does not exist";
            }
        }
        public string ClearFavorites()
        {
            var favoriteCount = Favorites.Count();

            Favorites.Clear();

            return $"Cleared: {favoriteCount} tracks from favorites";
        }
    }
}
