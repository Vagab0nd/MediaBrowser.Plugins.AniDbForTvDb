using LanguageExt;
using static LanguageExt.Prelude;

namespace MediaBrowser.Plugins.AniMetadata
{
    public static class MaybeParsers
    {
        public static Option<int> MaybeInt(this string value)
        {
            return parseInt(value);
        }
    }
}