using System.Threading.Tasks;
using Victoria;
using StreamMusicBot.Extensions;
using System.Collections.Generic;

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

        public async Task<IEnumerable<LavaTrack>> GetTrack(string query)
        {
            var  tracks = new List<LavaTrack>();
            switch (query)
            {
                case var a when a.Contains("open.spotify") && a.Contains("track"):
                    var trackReponse = await _lavaNode.SearchSpotifyAsync(query);
                    tracks.Add(trackReponse.GetFirstLavaTrack());
                    return tracks;

                case var a when a.Contains("open.spotify") && a.Contains("playlist"):
                    var playListReponse = await _lavaNode.SearchSpotifyPlaylistAsync(query);
                    tracks.AddRange(playListReponse.GetFirstLavaTracks());
                    return tracks;

                case var a when a.Contains("soundcloud."):
                    trackReponse = await _lavaNode.SearchSoundCloudAsync(query);
                    tracks.Add(trackReponse.GetFirstLavaTrack());
                    return tracks;

                default:
                    trackReponse = await _lavaNode.SearchYouTubeAsync(query);
                    tracks.Add(trackReponse.GetFirstLavaTrack());
                    return tracks;
            }
        }

    }
}
