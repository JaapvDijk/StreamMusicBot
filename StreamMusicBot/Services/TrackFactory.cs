using System.Threading.Tasks;
using Victoria;
using StreamMusicBot.MyExtensions;
using System.Collections.Generic;
using StreamMusicBot.Helpers;

namespace StreamMusicBot.Services
{
    public class TrackFactory
    {
        private readonly LavaNode _lavaNode;

        public TrackFactory(LavaNode lavaNode)
        {
            _lavaNode = lavaNode;
        }
        public async Task<IEnumerable<LavaTrack>> GetTracks(string query)
        {
            switch (query)
            {
                case string x when Helper.isSpotifyTrack(x):
                    var trackReponse = await _lavaNode.SearchSpotifyAsync(query);
                    return new List<LavaTrack>() { trackReponse.GetFirstLavaTrack() };

                case string x when Helper.isSpotifyPlaylist(x):
                    var playListReponse = await _lavaNode.SearchSpotifyPlaylistAsync(query);
                    return playListReponse.GetLavaTracks();

                case string x when Helper.isSoundCloud(x):
                    trackReponse = await _lavaNode.SearchSoundCloudAsync(query);
                    return new List<LavaTrack>() { trackReponse.GetFirstLavaTrack() };

                default:
                    trackReponse = await _lavaNode.SearchYouTubeAsync(query);
                    return new List<LavaTrack>() { trackReponse.GetFirstLavaTrack() };
            }
        }
    }
}
