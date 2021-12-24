using System;

namespace StreamMusicBot.Services
{
    public static class Extensions
    {
        public static string ToHumanReadableString(this TimeSpan t)
        {
            if (t.TotalSeconds <= 1)
            {
                return $@"{t:s\.ff} sec";
            }
            if (t.TotalMinutes <= 1)
            {
                return $@"{t:%s} sec";
            }
            if (t.TotalHours <= 1)
            {
                return $@"{t:%m} min";
            }
            if (t.TotalDays <= 1)
            {
                return $@"{t:%h} hr";
            }

            return $@"{t:%d} day(s)";
        }
    }
}
