using System.Collections.Generic;
using System.Linq;
using Emby.AniDbMetaStructure.AniDb.SeriesData;
using Emby.AniDbMetaStructure.Configuration;
using LanguageExt;
using MediaBrowser.Model.Logging;

namespace Emby.AniDbMetaStructure.AniDb
{
    internal class AniDbTitleSelector : IAniDbTitleSelector
    {
        private readonly ILogger log;

        public AniDbTitleSelector(ILogManager logManager)
        {
            this.log = logManager.GetLogger(nameof(AniDbTitleSelector));
        }

        public Option<ItemTitleData> SelectTitle(IEnumerable<ItemTitleData> titles, TitleType preferredTitleType,
            string metadataLanguage)
        {
            this.log.Debug(
                $"Selecting title from [{string.Join(", ", titles.Select(t => t.ToString()))}] available, preference for {preferredTitleType}, metadata language '{metadataLanguage}'");

            var preferredTitle = this.FindPreferredTitle(titles, preferredTitleType, metadataLanguage);

            preferredTitle.Match(
                t => this.log.Debug($"Found preferred title '{t.Title}'"),
                () =>
                {
                    var defaultTitle = this.FindDefaultTitle(titles);

                    defaultTitle.Match(
                        t => this.log.Debug($"Failed to find preferred title, falling back to default title '{t.Title}'"),
                        () => this.log.Debug("Failed to find any title"));

                    preferredTitle = defaultTitle;
                });

            return preferredTitle;
        }

        private Option<ItemTitleData> FindDefaultTitle(IEnumerable<ItemTitleData> titles)
        {
            var title = this.FindTitle(titles, "x-jat");

            title.Match(
                t => { },
                () => title = this.FindMainTitle(titles));

            return title;
        }

        private Option<ItemTitleData> FindPreferredTitle(IEnumerable<ItemTitleData> titles,
            TitleType preferredTitleType, string metadataLanguage)
        {
            switch (preferredTitleType)
            {
                case TitleType.Localized:
                    return this.FindTitle(titles, metadataLanguage);

                case TitleType.Japanese:
                    return this.FindTitle(titles, "ja");

                case TitleType.JapaneseRomaji:
                    return this.FindTitle(titles, "x-jat");
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