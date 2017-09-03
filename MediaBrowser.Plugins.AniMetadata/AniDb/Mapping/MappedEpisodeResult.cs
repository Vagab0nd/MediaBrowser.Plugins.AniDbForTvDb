using OneOf;

namespace MediaBrowser.Plugins.AniMetadata.AniDb.Mapping
{
    public class MappedEpisodeResult
        : OneOfBase<TvDbEpisodeNumber, AbsoluteEpisodeNumber, UnmappedEpisodeNumber>
    {
        protected MappedEpisodeResult(int index, TvDbEpisodeNumber value0 = null, AbsoluteEpisodeNumber value1 = null,
            UnmappedEpisodeNumber value2 = null) : base(index, value0, value1, value2)
        {
        }

        public static implicit operator MappedEpisodeResult(TvDbEpisodeNumber value)
        {
            return new MappedEpisodeResult(0, value);
        }

        public static implicit operator MappedEpisodeResult(AbsoluteEpisodeNumber value)
        {
            return new MappedEpisodeResult(1, null, value);
        }

        public static implicit operator MappedEpisodeResult(UnmappedEpisodeNumber value)
        {
            return new MappedEpisodeResult(2, null, null, value);
        }
    }
}