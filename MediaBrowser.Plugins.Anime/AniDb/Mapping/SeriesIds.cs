using Functional.Maybe;

namespace MediaBrowser.Plugins.Anime.AniDb.Mapping
{
    public class SeriesIds
    {
        public SeriesIds(int aniDbSeriesId, Maybe<int> tvDbSeriesId, Maybe<int> imdbSeriesId, Maybe<int> tmDbSeriesId)
        {
            AniDbSeriesId = aniDbSeriesId;
            TvDbSeriesId = tvDbSeriesId;
            ImdbSeriesId = imdbSeriesId;
            TmDbSeriesId = tmDbSeriesId;
        }

        public int AniDbSeriesId { get; }

        public Maybe<int> TvDbSeriesId { get; }

        public Maybe<int> ImdbSeriesId { get; }

        public Maybe<int> TmDbSeriesId { get; }
    }
}