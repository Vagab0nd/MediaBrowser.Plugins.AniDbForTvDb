using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.AniDb.Titles;

namespace MediaBrowser.Plugins.AniMetadata.Providers.AniDb
{
    internal class EpisodeMatcher : IEpisodeMatcher
    {
        private readonly ILogger _log;
        private readonly ITitleNormaliser _titleNormaliser;

        public EpisodeMatcher(ITitleNormaliser titleNormaliser, ILogManager logManager)
        {
            _titleNormaliser = titleNormaliser;
            _log = logManager.GetLogger(nameof(EpisodeMatcher));
        }

        public Option<AniDbEpisodeData> FindEpisode(IEnumerable<AniDbEpisodeData> episodes, Option<int> seasonIndex,
            Option<int> episodeIndex, Option<string> title)
        {
            return episodeIndex.Match(
                index => FindEpisodeByIndex(episodes, seasonIndex, index, title),
                () => FindEpisodeByTitle(episodes, title, episodeIndex));
        }

        private Option<AniDbEpisodeData> FindEpisodeByIndex(IEnumerable<AniDbEpisodeData> episodes,
            Option<int> seasonIndex,
            int episodeIndex, Option<string> title)
        {
            return seasonIndex.Match(si => FindEpisodeByIndexes(episodes, si, episodeIndex),
                () =>
                {
                    _log.Debug("No season index specified, searching by title");

                    return FindEpisodeByTitle(episodes, title, episodeIndex);
                });
        }

        private Option<AniDbEpisodeData> FindEpisodeByIndexes(IEnumerable<AniDbEpisodeData> episodes, int seasonIndex,
            int episodeIndex)
        {
            var type = seasonIndex == 0 ? EpisodeType.Special : EpisodeType.Normal;

            var episode = episodes?.FirstOrDefault(e => e.EpisodeNumber.Type == type &&
                e.EpisodeNumber.Number == episodeIndex);

            return episode;
        }

        private Option<AniDbEpisodeData> FindEpisodeByTitle(IEnumerable<AniDbEpisodeData> episodes,
            Option<string> title, Option<int> episodeIndex)
        {
            return title.Match(t =>
                {
                    _log.Debug($"Searching by title '{t}'");

                    return FindEpisodeByTitle(episodes, t)
                        .Match(d => d,
                            () =>
                            {
                                return episodeIndex.Match(index =>
                                {
                                    _log.Debug(
                                        $"No episode with matching title found for episode index {episodeIndex}, defaulting to season 1");
                                    return FindEpisodeByIndexes(episodes, 1, index);
                                }, () =>
                                {
                                    _log.Info($"Failed to find episode data");
                                    return Option<AniDbEpisodeData>.None;
                                });
                            });
                },
                () =>
                {
                    return episodeIndex.Match(index =>
                    {
                        _log.Debug(
                            $"No title specified for episode index {episodeIndex}, defaulting to season 1");

                        return FindEpisodeByIndexes(episodes, 1, index);
                    }, () =>
                    {
                        _log.Info($"Failed to find episode data");
                        return Option<AniDbEpisodeData>.None;
                    });
                });
        }

        private Option<AniDbEpisodeData> FindEpisodeByTitle(IEnumerable<AniDbEpisodeData> episodes, string title)
        {
            var episode = episodes?.FirstOrDefault(
                e => e.Titles.Any(t => _titleNormaliser.GetNormalisedTitle(t.Title) ==
                    _titleNormaliser.GetNormalisedTitle(title)));

            return episode;
        }
    }
}