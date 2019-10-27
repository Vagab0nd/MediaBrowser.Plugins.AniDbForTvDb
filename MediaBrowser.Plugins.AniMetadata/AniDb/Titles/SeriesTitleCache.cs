using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using MediaBrowser.Model.Logging;

namespace Emby.AniDbMetaStructure.AniDb.Titles
{
    internal class SeriesTitleCache : ISeriesTitleCache
    {
        private readonly IAniDbDataCache aniDbDataCache;
        private readonly ILogger log;
        private readonly ITitleNormaliser titleNormaliser;
        private readonly Lazy<IDictionary<string, TitleListItemData>> titles;

        public SeriesTitleCache(IAniDbDataCache aniDbDataCache, ITitleNormaliser titleNormaliser,
            ILogManager logManager)
        {
            this.aniDbDataCache = aniDbDataCache;
            this.titleNormaliser = titleNormaliser;
            this.log = logManager.GetLogger(nameof(SeriesTitleCache));
            this.titles = new Lazy<IDictionary<string, TitleListItemData>>(this.GetTitles);
        }

        public Option<TitleListItemData> FindSeriesByTitle(string title)
        {
            var match = this.FindExactTitleMatch(title).Match(t => t, () => this.FindComparableMatch(title));

            return match;
        }

        private Option<TitleListItemData> FindExactTitleMatch(string title)
        {
            this.titles.Value.TryGetValue(title, out var match);

            Option<TitleListItemData> foundTitle = match;

            foundTitle.Match(t => this.log.Debug($"Found exact title match for '{title}'"),
                () => this.log.Debug($"Failed to find exact title match for '{title}'"));

            return foundTitle;
        }

        private Option<TitleListItemData> FindComparableMatch(string title)
        {
            title = this.titleNormaliser.GetNormalisedTitle(title);

            this.titles.Value.TryGetValue(title, out var match);

            Option<TitleListItemData> foundTitle = match;

            foundTitle.Match(t => this.log.Debug($"Found comparable title match for '{title}'"),
                () => this.log.Debug($"Failed to find comparable title match for '{title}'"));

            return foundTitle;
        }

        private IDictionary<string, TitleListItemData> GetTitles()
        {
            var titles = new Dictionary<string, TitleListItemData>(StringComparer.OrdinalIgnoreCase);

            var titlesAgainstItems = this.aniDbDataCache.TitleList.SelectMany(i => i.Titles.Select(t => new
            {
                t.Title,
                ComparableTitle = this.titleNormaliser.GetNormalisedTitle(t.Title),
                Item = i
            }));

            foreach (var titlesAgainstItem in titlesAgainstItems)
            {
                this.AddIfMissing(titles, titlesAgainstItem.Title, titlesAgainstItem.Item);
                this.AddIfMissing(titles, titlesAgainstItem.ComparableTitle, titlesAgainstItem.Item);
            }

            return titles;
        }

        private void AddIfMissing(IDictionary<string, TitleListItemData> dictionary, string key,
            TitleListItemData value)
        {
            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, value);
            }
        }
    }
}