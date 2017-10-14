using LanguageExt;

namespace MediaBrowser.Plugins.AniMetadata.Mapping
{
    public class SeriesIds
    {
        public SeriesIds(int aniDbSeriesId, Option<int> tvDbSeriesId, Option<int> imdbSeriesId,
            Option<int> tmDbSeriesId)
        {
            AniDbSeriesId = aniDbSeriesId;
            TvDbSeriesId = tvDbSeriesId;
            ImdbSeriesId = imdbSeriesId;
            TmDbSeriesId = tmDbSeriesId;
        }

        public int AniDbSeriesId { get; }

        public Option<int> TvDbSeriesId { get; }

        public Option<int> ImdbSeriesId { get; }

        public Option<int> TmDbSeriesId { get; }
    }
}