using System.Collections.Generic;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.AniDb.Series.Data;

namespace MediaBrowser.Plugins.AniMetadata.Providers.AniDb
{
    internal interface IEpisodeMatcher
    {
        /// <summary>
        ///     Finds an episode in the collection that best matches the criteria
        /// </summary>
        Option<EpisodeData> FindEpisode(IEnumerable<EpisodeData> episodes, Option<int> seasonIndex,
            Option<int> episodeIndex, Option<string> title);
    }
}