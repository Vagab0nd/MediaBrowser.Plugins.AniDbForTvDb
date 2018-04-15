using System.Collections.Generic;
using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.AniList.Data;
using MediaBrowser.Plugins.AniMetadata.Process;

namespace MediaBrowser.Plugins.AniMetadata.AniList
{
    internal interface IAniListClient
    {
        Task<Either<ProcessFailedResult, IEnumerable<AniListSeriesData>>> FindSeriesAsync(string title,
            ProcessResultContext resultContext);
    }
}