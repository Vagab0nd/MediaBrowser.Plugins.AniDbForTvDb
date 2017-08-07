using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Functional.Maybe;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.Anime.AniDb.Data;
using MediaBrowser.Plugins.Anime.AniDb.Mapping;

namespace MediaBrowser.Plugins.Anime.AniDb
{
    /// <summary>
    ///     Retrieves data from AniDb
    /// </summary>
    internal class AniDbClient : IAniDbClient
    {
        private readonly IAniDbDataCache _aniDbDataCache;
        private readonly IAnimeMappingListFactory _animeMappingListFactory;
        private readonly ILogger _log;
        private readonly Lazy<IDictionary<string, TitleListItem>> _titles;

        public AniDbClient(IAniDbDataCache aniDbDataCache, IAnimeMappingListFactory animeMappingListFactory,
            ILogManager logManager)
        {
            _aniDbDataCache = aniDbDataCache;
            _animeMappingListFactory = animeMappingListFactory;
            _log = logManager.GetLogger(nameof(AniDbClient));
            _titles = new Lazy<IDictionary<string, TitleListItem>>(GetTitles);
        }

        public Task<Maybe<AniDbSeries>> FindSeriesAsync(string title)
        {
            var matchedTitle = FindExactTitleMatch(title).Or(() => FindComparableMatch(title));

            var seriesTask = Task.FromResult(Maybe<AniDbSeries>.Nothing);

            matchedTitle.Match(
                t =>
                {
                    _log.Debug($"Found AniDb series Id '{t.AniDbId}' by title");

                    seriesTask = _aniDbDataCache.GetSeriesAsync(t.AniDbId, CancellationToken.None)
                        .ContinueWith(task => task.Result.ToMaybe());
                },
                () => _log.Debug("Failed to find AniDb series by title"));

            return seriesTask;
        }

        public Task<AniDbSeries> GetSeriesAsync(int aniDbSeriesId)
        {
            return _aniDbDataCache.GetSeriesAsync(aniDbSeriesId, CancellationToken.None);
        }

        public async Task<Maybe<AniDbSeries>> GetSeriesAsync(string aniDbSeriesIdString)
        {
            var aniDbSeries = !int.TryParse(aniDbSeriesIdString, out int aniDbSeriesId)
                ? Maybe<AniDbSeries>.Nothing
                : (await GetSeriesAsync(aniDbSeriesId)).ToMaybe();

            return aniDbSeries;
        }

        public async Task<AniDbMapper> GetMapperAsync()
        {
            var mappingList = await _animeMappingListFactory.CreateMappingListAsync(CancellationToken.None);

            return new AniDbMapper(mappingList);
        }

        private Maybe<TitleListItem> FindExactTitleMatch(string title)
        {
            _titles.Value.TryGetValue(title, out TitleListItem match);

            var foundTitle = match.ToMaybe();

            foundTitle.Match(t => _log.Debug($"Found exact title match for '{title}'"),
                () => _log.Debug($"Failed to find exact title match for '{title}'"));

            return foundTitle;
        }

        private Maybe<TitleListItem> FindComparableMatch(string title)
        {
            title = GetComparableTitle(title);

            _titles.Value.TryGetValue(title, out TitleListItem match);

            var foundTitle = match.ToMaybe();

            foundTitle.Match(t => _log.Debug($"Found comparable title match for '{title}'"),
                () => _log.Debug($"Failed to find comparable title match for '{title}'"));

            return foundTitle;
        }

        private IDictionary<string, TitleListItem> GetTitles()
        {
            var titles = new Dictionary<string, TitleListItem>(StringComparer.OrdinalIgnoreCase);

            var titlesAgainstItems = _aniDbDataCache.TitleList.SelectMany(i => i.Titles.Select(t => new
            {
                t.Title,
                ComparableTitle = GetComparableTitle(t.Title),
                Item = i
            }));

            foreach (var titlesAgainstItem in titlesAgainstItems)
            {
                AddIfMissing(titles, titlesAgainstItem.Title, titlesAgainstItem.Item);
                AddIfMissing(titles, titlesAgainstItem.Title, titlesAgainstItem.Item);
            }

            return titles;
        }

        private void AddIfMissing(IDictionary<string, TitleListItem> dictionary, string key, TitleListItem value)
        {
            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, value);
            }
        }

        private string GetComparableTitle(string title)
        {
            title = title.ToLower().Normalize(NormalizationForm.FormKD);

            var actions = new Func<string, string>[]
            {
                RemoveModifiersAndDiacritics,
                RemoveIgnoredCharacters,
                ReplaceSpaceCharacters,
                ExpandAmpersands,
                RemoveErroneousArticles,
                CollapseMultipleSpaces
            };

            foreach (var action in actions)
                title = action(title);

            return title.Trim();
        }

        private string RemoveModifiersAndDiacritics(string title)
        {
            return new string(title.Where(c => !(c >= 0x2B0 && c <= 0x0333)).ToArray());
        }

        private string RemoveIgnoredCharacters(string title)
        {
            var ignoredCharactersRegex = new Regex("\"'!`?");

            return ignoredCharactersRegex.Replace(title, "");
        }

        private string ReplaceSpaceCharacters(string title)
        {
            var spaceCharactersRegex = new Regex(Regex.Escape("/,.:;\\(){}[]+-_=–*"));

            return spaceCharactersRegex.Replace(title, " ");
        }

        private string ExpandAmpersands(string title)
        {
            return title.Replace("&", " and ");
        }

        private string CollapseMultipleSpaces(string title)
        {
            var multipleSpaceRegex = new Regex(" {2,}");

            return multipleSpaceRegex.Replace(title, " ");
        }

        private string RemoveErroneousArticles(string title)
        {
            return title.Replace(", the", "").Replace("the ", " ").Replace(" the ", " ");
        }
    }
}