using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Configuration;

namespace MediaBrowser.Plugins.AniMetadata.AniDb
{
    internal class AniDbTitleSelector : IAniDbTitleSelector
    {
        private readonly ILogger _log;

        public AniDbTitleSelector(ILogManager logManager)
        {
            _log = logManager.GetLogger(nameof(AniDbTitleSelector));
        }

        public Option<ItemTitleData> SelectTitle(IEnumerable<ItemTitleData> titles, TitleType preferredTitleType,
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

        private Option<ItemTitleData> FindDefaultTitle(IEnumerable<ItemTitleData> titles)
        {
            var title = FindTitle(titles, "x-jat");

            title.Match(
                t => { },
                () => title = FindMainTitle(titles));

            return title;
        }

        private Option<ItemTitleData> FindPreferredTitle(IEnumerable<ItemTitleData> titles,
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

            return Option<ItemTitleData>.None;
        }

        private Option<ItemTitleData> FindTitle(IEnumerable<ItemTitleData> titles, string metadataLanguage)
        {
            var title = titles
                .OrderBy(t => t.Priority)
                .FirstOrDefault(t => t.Language == metadataLanguage);

            return title;
        }

        private Option<ItemTitleData> FindMainTitle(IEnumerable<ItemTitleData> titles)
        {
            return titles.FirstOrDefault(t => t.Type == "main");
        }
    }
}