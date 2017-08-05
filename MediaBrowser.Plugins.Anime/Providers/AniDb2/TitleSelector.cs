using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Plugins.Anime.AniDb.Data;
using MediaBrowser.Plugins.Anime.Configuration;

namespace MediaBrowser.Plugins.Anime.Providers.AniDb2
{
    internal class TitleSelector : ITitleSelector
    {
        public ItemTitle SelectTitle(IEnumerable<ItemTitle> titles, TitlePreferenceType preferredTitleType,
            string metadataLanguage)
        {
            var preferredTitle = FindPreferredTitle(titles, preferredTitleType, metadataLanguage);

            return preferredTitle ?? FindDefaultTitle(titles);
        }

        private ItemTitle FindDefaultTitle(IEnumerable<ItemTitle> titles)
        {
            var title = FindTitle(titles, "x-jat");

            if (title == null)
            {
                title = FindMainTitle(titles);
            }

            return title ?? titles.FirstOrDefault();
        }

        private ItemTitle FindPreferredTitle(IEnumerable<ItemTitle> titles, TitlePreferenceType preferredTitleType,
            string metadataLanguage)
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

        private ItemTitle FindTitle(IEnumerable<ItemTitle> titles, string metadataLanguage)
        {
            return titles
                .Where(t => t.Priority.HasValue)
                .OrderBy(t => t.Priority.Value)
                .FirstOrDefault(t => t.Language == metadataLanguage);
        }

        private ItemTitle FindMainTitle(IEnumerable<ItemTitle> titles)
        {
            return titles.FirstOrDefault(t => t.Type == "main");
        }
    }
}