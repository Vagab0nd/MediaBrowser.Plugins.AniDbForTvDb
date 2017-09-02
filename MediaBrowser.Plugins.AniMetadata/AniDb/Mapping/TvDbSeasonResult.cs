using FunctionalSharp.DiscriminatedUnions;

namespace MediaBrowser.Plugins.AniMetadata.AniDb.Mapping
{
    public class TvDbSeasonResult : DiscriminatedUnion<TvDbSeason, AbsoluteTvDbSeason>
    {
        public TvDbSeasonResult(TvDbSeason item) : base(item)
        {
        }

        public TvDbSeasonResult(AbsoluteTvDbSeason item) : base(item)
        {
        }
    }
}