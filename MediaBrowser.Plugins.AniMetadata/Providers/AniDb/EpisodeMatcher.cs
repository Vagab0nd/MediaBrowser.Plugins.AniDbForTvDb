using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.AniMetadata.AniDb.Series;
using MediaBrowser.Plugins.AniMetadata.AniDb.Series.Data;
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

        public Option<EpisodeData> FindEpisode(IEnumerable<EpisodeData> episodes, Option<int> seasonIndex,
            Option<int> episodeIndex, Option<string> title)
        {
            return episodeIndex.Match(ei => FindEpisode(episodes, seasonIndex, ei, title),
                () =>
                {
                    _log.Warn($"No episode index found for title '{title.Match(t => t, () => "")}'");
                    return Option<EpisodeData>.None;
                });
        }

        private Option<EpisodeData> FindEpisode(IEnumerable<EpisodeData> episodes, Option<int> seasonIndex,
            int episodeIndex, Option<string> title)
        {
            return seasonIndex.Match(si => FindEpisodeByIndexes(episodes, si, episodeIndex),
                () =>
                {
                    _log.Debug("No season index specified, searching by title");
                    return title.Match(t =>
                        {
                            _log.Debug($"Searching by title '{t}'");
                            return FindEpisodeByTitle(episodes, t)
                                .Match(d => d,
                                    () =>
                                    {
                                        _log.Debug(
                                            $"No episode with matching title found for episode index {episodeIndex}, defaulting to season 1");
                                        return FindEpisodeByIndexes(episodes, 1, episodeIndex);
                                    });
                        },
                        () =>
                        {
                            _log.Debug(
                                $"No title specified for episode index {episodeIndex}, defaulting to season 1");
                            return FindEpisodeByIndexes(episodes, 1, episodeIndex);
                        });
                });
        }

        private Option<EpisodeData> FindEpisodeByIndexes(IEnumerable<EpisodeData> episodes, int seasonIndex,
            int episodeIndex)
        {
            var type = seasonIndex == 0 ? EpisodeType.Special : EpisodeType.Normal;

            var episode = episodes?.FirstOrDefault(e => e.EpisodeNumber.Type == type &&
                e.EpisodeNumber.Number == episodeIndex);

            return episode;
        }

        private Option<EpisodeData> FindEpisodeByTitle(IEnumerable<EpisodeData> episodes, string title)
        {
            var episode = episodes?.FirstOrDefault(
                e => e.Titles.Any(t => _titleNormaliser.GetNormalisedTitle(t.Title) ==
                    _titleNormaliser.GetNormalisedTitle(title)));

            return episode;
        }
    }
}