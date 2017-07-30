using System.Text.RegularExpressions;

namespace MediaBrowser.Plugins.Anime.Providers.AniDB.Converter
{
    public class AnidbEpisodeIdentity
    {
        private static readonly Regex Regex = new Regex(@"(?<series>\d+):(?<type>[S])?(?<epno>\d+)(-(?<epnoend>\d+))?");

        public string SeriesId { get; }
        public int EpisodeNumber { get; }
        public int? EpisodeNumberEnd { get; }
        public string EpisodeType { get; }
        
        public AnidbEpisodeIdentity(string seriesId, int episodeNumber, int? episodeNumberEnd, string episodeType)
        {
            SeriesId = seriesId;
            EpisodeNumber = episodeNumber;
            EpisodeNumberEnd = episodeNumberEnd;
            EpisodeType = episodeType;
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}{2}",
                SeriesId,
                EpisodeType ?? "",
                EpisodeNumber + (EpisodeNumberEnd != null ? "-" + EpisodeNumberEnd.Value : ""));
        }

        public static AnidbEpisodeIdentity Parse(string id)
        {
            var match = Regex.Match(id);
            if (match.Success)
            {
                return new AnidbEpisodeIdentity(
                    match.Groups["series"].Value,
                    int.Parse(match.Groups["epno"].Value),
                    match.Groups["epnoend"].Success ? int.Parse(match.Groups["epnoend"].Value) : (int?)null,
                    match.Groups["type"].Success ? match.Groups["type"].Value : null);
            }

            return null;
        }
    }
}