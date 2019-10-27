using System.Threading.Tasks;
using Emby.AniDbMetaStructure.TvDb.Data;
using LanguageExt;

namespace Emby.AniDbMetaStructure.Mapping
{
    internal interface IDefaultSeasonEpisodeMapper
    {
        Task<Option<TvDbEpisodeData>> MapEpisodeAsync(int aniDbEpisodeIndex, ISeriesMapping seriesMapping);
    }
}