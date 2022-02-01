using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamMusicBot.Services
{
    public enum MusicPlatformSource
    {
        NotFound,
        Spotify,
        Youtube,
        Soundcloud
    }

    public interface IURLDetails
    {
        bool isPlaylist();
        string id();
        MusicPlatformSource source();
        string url();
    }
}
