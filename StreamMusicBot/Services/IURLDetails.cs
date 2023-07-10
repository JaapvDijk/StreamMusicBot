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
