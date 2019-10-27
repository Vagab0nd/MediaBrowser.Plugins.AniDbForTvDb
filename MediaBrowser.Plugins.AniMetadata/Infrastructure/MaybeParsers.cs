using LanguageExt;
using static LanguageExt.Prelude;

namespace Emby.AniDbMetaStructure.Infrastructure
{
    public static class MaybeParsers
    {
        public static Option<int> MaybeInt(this string value)
        {
            return parseInt(value);
        }
    }
}