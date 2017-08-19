using Functional.Maybe;

namespace MediaBrowser.Plugins.Anime.AniDb.Mapping
{
    public interface IMappingList
    {
        Maybe<SeriesMapping> GetSeriesMapping(int aniDbSeriesId);
    }
}