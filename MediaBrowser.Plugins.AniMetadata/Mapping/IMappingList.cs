using LanguageExt;

namespace MediaBrowser.Plugins.AniMetadata.Mapping
{
    public interface IMappingList
    {
        Option<ISeriesMapping> GetSeriesMappingFromAniDb(int aniDbSeriesId);

        Option<ISeriesMapping> GetSeriesMappingFromTvDb(int tvDbSeriesId);
    }
}