using FunctionalSharp.DiscriminatedUnions;

namespace MediaBrowser.Plugins.Anime.AniDb.Mapping
{
    public class MappedEpisodeResult
        : DiscriminatedUnion<TvDbEpisodeNumber, AbsoluteEpisodeNumber,
            UnmappedEpisodeNumber>
    {
        public MappedEpisodeResult(TvDbEpisodeNumber item) : base(item)
        {
        }

        public MappedEpisodeResult(AbsoluteEpisodeNumber item) : base(item)
        {
        }

        public MappedEpisodeResult(UnmappedEpisodeNumber item) : base(item)
        {
        }
    }
}