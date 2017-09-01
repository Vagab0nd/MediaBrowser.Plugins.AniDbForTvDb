using System.Collections.Generic;
using System.Linq;
using Functional.Maybe;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.Anime.AniDb.Series;
using MediaBrowser.Plugins.Anime.AniDb.Series.Data;
using MediaBrowser.Plugins.Anime.Configuration;

namespace MediaBrowser.Plugins.Anime.Providers.AniDb2
{
    internal class TitleSelector : ITitleSelector
    {
        private readonly ILogger _log;

        public TitleSelector(ILogManager logManager)
        {
            _log = logManager.GetLogger(nameof(TitleSelector));
        }

        public Maybe<ItemTitleData> SelectTitle(IEnumerable<ItemTitleData> titles, TitleType preferredTitleType,
            string metadataLanguage)
        {
            _log.Debug(
                $"Selecting title from [{string.Join(", ", titles.Select(t => t.ToString()))}] available, preference for {preferredTitleType}, metadata language '{metadataLanguage}'");

            var preferredTitle = FindPreferredTitle(titles, preferredTitleType, metadataLanguage);

            preferredTitle.Match(
                t => _log.Debug($"Found preferred title '{t.Title}'"),
                () =>
                {
                    var defaultTitle = FindDefaultTitle(titles);

                    defaultTitle.Match(
                        t => _log.Debug($"Failed to find preferred title, falling back to default title '{t.Title}'"),
                        () => _log.Debug("Failed to find any title"));

                    preferredTitle = defaultTitle;
                });

            return preferredTitle;
        }

        private Maybe<ItemTitleData> FindDefaultTitle(IEnumerable<ItemTitleData> titles)
        {
            var title = FindTitle(titles, "x-jat");

            title.Match(
                t => { },
                () => title = FindMainTitle(titles));

            return title;
        }

        private Maybe<ItemTitleData> FindPreferredTitle(IEnumerable<ItemTitleData> titles,
            TitleType preferredTitleType, string metadataLanguage)
        {
            switch (preferredTitleType)
            {
                case TitleType.Localized:
                    return FindTitle(titles, metadataLanguage);

                case TitleType.Japanese:
                    return FindTitle(titles, "ja");

                case TitleType.JapaneseRomaji:
                    return FindTitle(titles, "x-jat");
            }

            return Maybe<ItemTitleData>.Nothing;
        }

        private Maybe<ItemTitleData> FindTitle(IEnumerable<ItemTitleData> titles, string metadataLanguage)
        {
            var title = titles
                .OrderBy(t => t.Priority)
                .FirstOrDefault(t => t.Language == metadataLanguage);

            return title.ToMaybe();
        }

        private Maybe<ItemTitleData> FindMainTitle(IEnumerable<ItemTitleData> titles)
        {
            return titles.FirstOrDefault(t => t.Type == "main").ToMaybe();
        }
    }
}