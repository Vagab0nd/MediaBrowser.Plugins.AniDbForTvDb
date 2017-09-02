using Functional.Maybe;

namespace MediaBrowser.Plugins.AniMetadata.AniDb.Mapping
{
    public interface IMappingList
    {
        Maybe<SeriesMapping> GetSeriesMapping(int aniDbSeriesId);
    }
}