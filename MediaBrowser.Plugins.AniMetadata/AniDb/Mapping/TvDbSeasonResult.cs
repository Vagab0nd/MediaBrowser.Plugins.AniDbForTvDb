using OneOf;

namespace MediaBrowser.Plugins.AniMetadata.AniDb.Mapping
{
    public class TvDbSeasonResult : OneOfBase<TvDbSeason, AbsoluteTvDbSeason>
    {
        protected TvDbSeasonResult(int index, TvDbSeason value0 = null, AbsoluteTvDbSeason value1 = null) : base(index,
            value0, value1)
        {
        }

        public static implicit operator TvDbSeasonResult(TvDbSeason value)
        {
            return new TvDbSeasonResult(0, value);
        }

        public static implicit operator TvDbSeasonResult(AbsoluteTvDbSeason value)
        {
            return new TvDbSeasonResult(1, null, value);
        }
    }
}