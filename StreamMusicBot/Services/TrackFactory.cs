using System.Threading.Tasks;
using Victoria;
using StreamMusicBot.Extensions;

namespace StreamMusicBot.Services
{
    public class TrackFactory
    {
        private readonly LavaNode _lavaNode;
        private readonly LogService _logService;
        public TrackFactory(LavaNode lavaNode,
                            LogService logService)
        {
            _lavaNode = lavaNode;
            _logService = logService;
        }

        public async Task<LavaTrack> GetTrack(string query)
        {
            switch (query)
            {
                case var a when a.Contains("open.spotify") && a.Contains("track"):
                    var reponse = await _lavaNode.SearchSpotifyAsync(query);
                    return reponse.GetFirstLavaTrack();

                case var a when a.Contains("open.spotify") && a.Contains("playlist"):
                    var reponse2 = await _lavaNode.SearchSpotifyPlaylistAsync(query);
                    return new LavaTrack();

                case var a when a.Contains("soundcloud."):
                    reponse = await _lavaNode.SearchSoundCloudAsync(query);
                    return reponse.GetFirstLavaTrack();

                default:
                    reponse = await _lavaNode.SearchYouTubeAsync(query);
                    return reponse.GetFirstLavaTrack();
            }
        }

    }
}
