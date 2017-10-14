using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;

namespace MediaBrowser.Plugins.AniMetadata.Mapping
{
    internal interface IDefaultSeasonEpisodeMapper
    {
        Task<Option<TvDbEpisodeData>> MapEpisodeAsync(int aniDbEpisodeIndex, ISeriesMapping seriesMapping);
    }
}