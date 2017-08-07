using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.Anime.AniDb.Data;
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

        public IOption<ItemTitle> SelectTitle(IEnumerable<ItemTitle> titles, TitlePreferenceType preferredTitleType,
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

        private IOption<ItemTitle> FindDefaultTitle(IEnumerable<ItemTitle> titles)
        {
            var title = FindTitle(titles, "x-jat");

            title.Match(
                t => { },
                () => title = FindMainTitle(titles));

            return title;
        }

        private IOption<ItemTitle> FindPreferredTitle(IEnumerable<ItemTitle> titles,
            TitlePreferenceType preferredTitleType, string metadataLanguage)
        {
            switch (preferredTitleType)
            {
                case TitlePreferenceType.Localized:
                    return FindTitle(titles, metadataLanguage);

                case TitlePreferenceType.Japanese:
                    return FindTitle(titles, "ja");

                case TitlePreferenceType.JapaneseRomaji:
                    return FindTitle(titles, "x-jat");
            }

            return null;
        }

        private IOption<ItemTitle> FindTitle(IEnumerable<ItemTitle> titles, string metadataLanguage)
        {
            var title = titles
                .OrderBy(t => t.Priority)
                .FirstOrDefault(t => t.Language == metadataLanguage);

            return Option.Optionify(title);
        }

        private IOption<ItemTitle> FindMainTitle(IEnumerable<ItemTitle> titles)
        {
            return Option.Optionify(titles.FirstOrDefault(t => t.Type == "main"));
        }
    }
}