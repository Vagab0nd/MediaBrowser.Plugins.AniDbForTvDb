using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MediaBrowser.Plugins.Anime.AniDb.Titles
{
    internal class TitleNormaliser : ITitleNormaliser
    {
        public string GetNormalisedTitle(string title)
        {
            title = title.ToLower().Normalize(NormalizationForm.FormKD);

            var actions = new Func<string, string>[]
            {
                TrimEpisodeNumberPrefixes,
                RemoveModifiersAndDiacritics,
                RemoveIgnoredCharacters,
                ReplaceSpaceCharacters,
                ExpandAmpersands,
                RemoveErroneousArticles,
                CollapseMultipleSpaces
            };

            foreach (var action in actions)
                title = action(title);

            var comparableTitle = title.Trim();

            return comparableTitle.ToUpperInvariant();
        }

        private string RemoveModifiersAndDiacritics(string title)
        {
            return new string(title.Where(c => !(c >= 0x2B0 && c <= 0x0333)).ToArray());
        }

        private string RemoveIgnoredCharacters(string title)
        {
            var ignoredCharactersRegex = new Regex($"[{Regex.Escape(@"""'!`?")}]");

            return ignoredCharactersRegex.Replace(title, "");
        }

        private string ReplaceSpaceCharacters(string title)
        {
            var spaceCharactersRegex = new Regex(@"[/,\.:;\\\(\)\{}\[\]\+\-_\=\–\*]");

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

        private string TrimEpisodeNumberPrefixes(string title)
        {
            var episodePrefixRegex = new Regex(@"\d{1,3} - ");

            return episodePrefixRegex.Replace(title, "");
        }
    }
}