using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamMusicBot.Services
{
    public class FavoritesService
    {
        public List<string> Favorites { get; set; }

        public void AddFavorite(string query) 
        {
            Favorites.Add(query);
        }

        public void RemoveFavorite(string query)
        {
            Favorites.Remove(query);
        }
        public void ClearFavorites()
        {
            Favorites.Clear();
        }
    }
}
