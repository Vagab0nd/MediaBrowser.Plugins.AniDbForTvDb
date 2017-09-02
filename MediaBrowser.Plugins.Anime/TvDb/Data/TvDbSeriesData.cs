using System.Collections.Generic;

namespace MediaBrowser.Plugins.AniMetadata.TvDb.Data
{
    public class TvDbSeriesData
    {
        public TvDbSeriesData(int id, string seriesName, IEnumerable<string> aliases, IEnumerable<string> genre,
            string overview)
        {
            Id = id;
            SeriesName = seriesName;
            Aliases = aliases ?? new List<string>();
            Genres = genre ?? new List<string>();
            Overview = overview;
        }

        public int Id { get; }

        public string SeriesName { get; }

        public IEnumerable<string> Aliases { get; }

        public IEnumerable<string> Genres { get; }

        public string Overview { get; }

        public IEnumerable<TvDbEpisodeData> Episodes { get; }
    }
}