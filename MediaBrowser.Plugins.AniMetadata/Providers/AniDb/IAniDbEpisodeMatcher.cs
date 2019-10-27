using System.Collections.Generic;
using Emby.AniDbMetaStructure.AniDb.SeriesData;
using LanguageExt;

namespace Emby.AniDbMetaStructure.Providers.AniDb
{
    internal interface IAniDbEpisodeMatcher
    {
        /// <summary>
        ///     Finds an episode in the collection that best matches the criteria
        /// </summary>
        Option<AniDbEpisodeData> FindEpisode(IEnumerable<AniDbEpisodeData> episodes, Option<int> seasonIndex,
            Option<int> episodeIndex, Option<string> title);
    }
}