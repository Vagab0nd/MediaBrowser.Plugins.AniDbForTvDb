using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Plugins.Anime.AniDb.Data;
using MediaBrowser.Plugins.Anime.AniDb.Mapping;

namespace MediaBrowser.Plugins.Anime.AniDb
{
    /// <summary>
    ///     Retrieves data from AniDb
    /// </summary>
    internal class AniDbClient : IAniDbClient
    {
        private readonly AniDbDataCache _aniDbDataCache;
        private readonly AnimeMappingListFactory _animeMappingListFactory;
        private readonly Lazy<IDictionary<string, TitleListItem>> _titles;

        public AniDbClient(AniDbDataCache aniDbDataCache, AnimeMappingListFactory animeMappingListFactory)
        {
            _aniDbDataCache = aniDbDataCache;
            _animeMappingListFactory = animeMappingListFactory;
            _titles = new Lazy<IDictionary<string, TitleListItem>>(GetTitles);
        }

        public async Task<IOption<AniDbSeries>> FindSeriesAsync(string title)
        {
            var match = FindExactTitleMatch(title) ?? FindComparableMatch(title);

            var series = await _aniDbDataCache.GetSeriesAsync(match.AniDbId, CancellationToken.None);

            return Option.Optionify(series);
        }

        public Task<AniDbSeries> GetSeriesAsync(int aniDbSeriesId)
        {
            return _aniDbDataCache.GetSeriesAsync(aniDbSeriesId, CancellationToken.None);
        }

        public async Task<AniDbMapper> GetMapperAsync()
        {
            var mappingList = await _animeMappingListFactory.CreateMappingListAsync(CancellationToken.None);

            return new AniDbMapper(mappingList);
        }

        private TitleListItem FindExactTitleMatch(string title)
        {
            _titles.Value.TryGetValue(title, out TitleListItem match);

            return match;
        }

        private TitleListItem FindComparableMatch(string title)
        {
            title = GetComparableTitle(title);

            _titles.Value.TryGetValue(title, out TitleListItem match);

            return match;
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
            var spaceCharactersRegex = new Regex("/,.:;\\(){}[]+-_=–*");

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