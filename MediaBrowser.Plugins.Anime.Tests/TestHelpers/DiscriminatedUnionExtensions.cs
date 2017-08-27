using System;
using FunctionalSharp.DiscriminatedUnions;

namespace MediaBrowser.Plugins.Anime.Tests.TestHelpers
{
    internal static class DiscriminatedUnionExtensions
    {
        public static Type ResultType<T1, T2>(this DiscriminatedUnion<T1, T2> union)
        {
            Type result = null;

            union.Match(
                t1 => result = typeof(T1),
                t2 => result = typeof(T2)
            );

            return result;
        }

        public static Type ResultType<T1, T2, T3>(this DiscriminatedUnion<T1, T2, T3> union)
        {
            Type result = null;

            union.Match(
                t1 => result = typeof(T1),
                t2 => result = typeof(T2),
                t3 => result = typeof(T3)
            );

            return result;
        }
    }
}