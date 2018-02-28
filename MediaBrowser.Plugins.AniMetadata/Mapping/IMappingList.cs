using System.Collections.Generic;
using LanguageExt;

namespace MediaBrowser.Plugins.AniMetadata.Mapping
{
    public interface IMappingList
    {
        Option<ISeriesMapping> GetSeriesMappingFromAniDb(int aniDbSeriesId);

        Option<IEnumerable<ISeriesMapping>> GetSeriesMappingsFromTvDb(int tvDbSeriesId);
    }
}