using LanguageExt;

namespace MediaBrowser.Plugins.AniMetadata.AniDb.Mapping
{
    public interface IMappingList
    {
        Option<SeriesMapping> GetSeriesMapping(int aniDbSeriesId);
    }
}