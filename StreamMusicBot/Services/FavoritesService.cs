using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria;

namespace StreamMusicBot.Services
{
    public class FavoritesService
    {
        public List<LavaTrack> Favorites { get; set; } = new();

        public string GetFavorites()
        {
            var display = Favorites.Select((x, i) => $"**{i+1}**: {x.Title} \n");

            var result = "Favorites is empty";
            if (display.Count() > 0) result = string.Join("", display);

            return result;
        }
        public string AddFavorite(LavaTrack track) 
        {
            Favorites.Add(track);

            return $"Added {track.Title} to favorites";
        }

        public string RemoveFavorite(LavaTrack track)
        {
            if (Favorites.Contains(track))
            {
                Favorites.Remove(track);
                return $"Removed: {track} from favorites";
            }
            else
            {
                return $"{track.Title} not found in favorites";
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
