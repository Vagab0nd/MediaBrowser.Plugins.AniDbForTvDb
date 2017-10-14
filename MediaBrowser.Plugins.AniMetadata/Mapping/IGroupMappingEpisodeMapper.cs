using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;

namespace MediaBrowser.Plugins.AniMetadata.Mapping
{
    internal interface IGroupMappingEpisodeMapper
    {
        Task<Option<TvDbEpisodeData>> MapEpisodeAsync(int aniDbEpisodeIndex, EpisodeGroupMapping episodeGroupMapping, int tvDbSeriesId);
    }
}