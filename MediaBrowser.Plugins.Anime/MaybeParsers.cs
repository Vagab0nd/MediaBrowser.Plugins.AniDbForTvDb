using System;
using Functional.Maybe;

namespace MediaBrowser.Plugins.AniMetadata
{
    public static class MaybeParsers
    {
        private static readonly Func<string, Maybe<int>> _maybeIntParser =
            MaybeFunctionalWrappers.Wrap<string, int>(int.TryParse);

        public static Maybe<int> MaybeInt(this string value)
        {
            return _maybeIntParser(value);
        }
    }
}