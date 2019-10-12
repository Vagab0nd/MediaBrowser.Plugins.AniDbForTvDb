using static LanguageExt.Prelude;

namespace MediaBrowser.Plugins.AniMetadata.Infrastructure
{
    using LanguageExt;

    public static class MaybeParsers
    {
        public static Option<int> MaybeInt(this string value)
        {
            return parseInt(value);
        }
    }
}