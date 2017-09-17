using LanguageExt;

namespace MediaBrowser.Plugins.AniMetadata.AniDb.Mapping
{
    public interface IMappingList
    {
        Option<SeriesMapping> GetSeriesMappingFromAniDb(int aniDbSeriesId);

        Option<SeriesMapping> GetSeriesMappingFromTvDb(int aniDbSeriesId);
    }
}