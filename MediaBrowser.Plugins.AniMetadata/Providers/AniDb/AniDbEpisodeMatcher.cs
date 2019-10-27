using System.Collections.Generic;
using System.Linq;
using Emby.AniDbMetaStructure.AniDb.SeriesData;
using Emby.AniDbMetaStructure.AniDb.Titles;
using LanguageExt;
using MediaBrowser.Model.Logging;

namespace Emby.AniDbMetaStructure.Providers.AniDb
{
    internal class AniDbEpisodeMatcher : IAniDbEpisodeMatcher
    {
        private readonly ILogger log;
        private readonly ITitleNormaliser titleNormaliser;

        public AniDbEpisodeMatcher(ITitleNormaliser titleNormaliser, ILogManager logManager)
        {
            this.titleNormaliser = titleNormaliser;
            this.log = logManager.GetLogger(nameof(AniDbEpisodeMatcher));
        }

        public Option<AniDbEpisodeData> FindEpisode(IEnumerable<AniDbEpisodeData> episodes, Option<int> seasonIndex,
            Option<int> episodeIndex, Option<string> title)
        {
            return episodeIndex.Match(
                index => this.FindEpisodeByIndex(episodes, seasonIndex, index, title),
                () => this.FindEpisodeByTitle(episodes, title, episodeIndex));
        }

        private Option<AniDbEpisodeData> FindEpisodeByIndex(IEnumerable<AniDbEpisodeData> episodes,
            Option<int> seasonIndex,
            int episodeIndex, Option<string> title)
        {
            return seasonIndex.Match(si => this.FindEpisodeByIndexes(episodes, si, episodeIndex),
                () =>
                {
                    this.log.Debug("No season index specified, searching by title");

                    return this.FindEpisodeByTitle(episodes, title, episodeIndex);
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
                    this.log.Debug($"Searching by title '{t}'");

                    return this.FindEpisodeByTitle(episodes, t)
                        .Match(d => d,
                            () =>
                            {
                                return episodeIndex.Match(index =>
                                {
                                    this.log.Debug(
                                        $"No episode with matching title found for episode index {episodeIndex}, defaulting to season 1");
                                    return this.FindEpisodeByIndexes(episodes, 1, index);
                                }, () =>
                                {
                                    this.log.Info($"Failed to find episode data");
                                    return Option<AniDbEpisodeData>.None;
                                });
                            });
                },
                () =>
                {
                    return episodeIndex.Match(index =>
                    {
                        this.log.Debug(
                            $"No title specified for episode index {episodeIndex}, defaulting to season 1");

                        return this.FindEpisodeByIndexes(episodes, 1, index);
                    }, () =>
                    {
                        this.log.Info($"Failed to find episode data");
                        return Option<AniDbEpisodeData>.None;
                    });
                });
        }

        private Option<AniDbEpisodeData> FindEpisodeByTitle(IEnumerable<AniDbEpisodeData> episodes, string title)
        {
            var episode = episodes?.FirstOrDefault(
                e => e.Titles.Any(t => this.titleNormaliser.GetNormalisedTitle(t.Title) ==
                    this.titleNormaliser.GetNormalisedTitle(title)));

            return episode;
        }
    }
}