using OneOf;

namespace MediaBrowser.Plugins.AniMetadata.Providers
{
    public class SeriesData : OneOfBase<AniDbOnlySeriesData, CombinedSeriesData, NoSeriesData>
    {
        protected SeriesData(int index, AniDbOnlySeriesData value0 = null, CombinedSeriesData value1 = null,
            NoSeriesData value2 = null) : base(index, value0, value1, value2)
        {
        }

        public static implicit operator SeriesData(AniDbOnlySeriesData value)
        {
            return new SeriesData(0, value);
        }

        public static implicit operator SeriesData(CombinedSeriesData value)
        {
            return new SeriesData(1, null, value);
        }

        public static implicit operator SeriesData(NoSeriesData value)
        {
            return new SeriesData(2, null, null, value);
        }
    }
}