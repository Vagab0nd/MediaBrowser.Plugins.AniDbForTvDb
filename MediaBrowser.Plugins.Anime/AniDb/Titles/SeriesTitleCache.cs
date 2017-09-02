using System;
using System.Collections.Generic;
using System.Linq;
using Functional.Maybe;
using MediaBrowser.Model.Logging;

namespace MediaBrowser.Plugins.Anime.AniDb.Titles
{
    internal class SeriesTitleCache : ISeriesTitleCache
    {
        private readonly IAniDbDataCache _aniDbDataCache;
        private readonly ILogger _log;
        private readonly ITitleNormaliser _titleNormaliser;
        private readonly Lazy<IDictionary<string, TitleListItemData>> _titles;

        public SeriesTitleCache(IAniDbDataCache aniDbDataCache, ITitleNormaliser titleNormaliser,
            ILogManager logManager)
        {
            _aniDbDataCache = aniDbDataCache;
            _titleNormaliser = titleNormaliser;
            _log = logManager.GetLogger(nameof(SeriesTitleCache));
            _titles = new Lazy<IDictionary<string, TitleListItemData>>(GetTitles);
        }

        public Maybe<TitleListItemData> FindSeriesByTitle(string title)
        {
            var match = FindExactTitleMatch(title).Or(() => FindComparableMatch(title));

            return match;
        }

        private Maybe<TitleListItemData> FindExactTitleMatch(string title)
        {
            _titles.Value.TryGetValue(title, out var match);

            var foundTitle = match.ToMaybe();

            foundTitle.Match(t => _log.Debug($"Found exact title match for '{title}'"),
                () => _log.Debug($"Failed to find exact title match for '{title}'"));

            return foundTitle;
        }

        private Maybe<TitleListItemData> FindComparableMatch(string title)
        {
            title = _titleNormaliser.GetNormalisedTitle(title);

            _titles.Value.TryGetValue(title, out var match);

            var foundTitle = match.ToMaybe();

            foundTitle.Match(t => _log.Debug($"Found comparable title match for '{title}'"),
                () => _log.Debug($"Failed to find comparable title match for '{title}'"));

            return foundTitle;
        }

        private IDictionary<string, TitleListItemData> GetTitles()
        {
            var titles = new Dictionary<string, TitleListItemData>(StringComparer.OrdinalIgnoreCase);

            var titlesAgainstItems = _aniDbDataCache.TitleList.SelectMany(i => i.Titles.Select(t => new
            {
                t.Title,
                ComparableTitle = _titleNormaliser.GetNormalisedTitle(t.Title),
                Item = i
            }));

            foreach (var titlesAgainstItem in titlesAgainstItems)
            {
                AddIfMissing(titles, titlesAgainstItem.Title, titlesAgainstItem.Item);
                AddIfMissing(titles, titlesAgainstItem.ComparableTitle, titlesAgainstItem.Item);
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