using System;
using System.Linq;
using FunctionalSharp.DiscriminatedUnions;
using MediaBrowser.Plugins.Anime.Mapping.Lists;

namespace MediaBrowser.Plugins.Anime.Mapping
{
    internal class AnimeMappings
    {
        private readonly AnimeMappingList _animeMappingList;

        public AnimeMappings(AnimeMappingList animeMappingList)
        {
            _animeMappingList = animeMappingList;
        }

        public DiscriminatedUnion<TvDbSeriesId, NonTvDbSeriesId, UnknownSeriesId>
            GetMappedTvDbSeriesId(int aniDbSeriesId)
        {
            var mapping =
                _animeMappingList.AnimeSeriesMapping.FirstOrDefault(m => m.AnidbId == aniDbSeriesId.ToString());
            DiscriminatedUnion<TvDbSeriesId, NonTvDbSeriesId, UnknownSeriesId> result;

            if (mapping == null || string.Equals(mapping.TvDbId, "Unknown", StringComparison.CurrentCultureIgnoreCase))
            {
                result = new DiscriminatedUnion<TvDbSeriesId, NonTvDbSeriesId, UnknownSeriesId>(new UnknownSeriesId());
            }
            else if (int.TryParse(mapping.TvDbId, out int tvDbSeriesId))
            {
                result =
                    new DiscriminatedUnion<TvDbSeriesId, NonTvDbSeriesId, UnknownSeriesId>(
                        new TvDbSeriesId(tvDbSeriesId));
            }
            else
            {
                result =
                    new DiscriminatedUnion<TvDbSeriesId, NonTvDbSeriesId, UnknownSeriesId>(
                        new NonTvDbSeriesId(mapping.TvDbId));
            }

            return result;
        }

        /// <summary>
        ///     A TvDb series Id
        /// </summary>
        internal class TvDbSeriesId
        {
            public TvDbSeriesId(int tvDbSeriesId)
            {
                Id = tvDbSeriesId;
            }

            public int Id { get; }
        }

        /// <summary>
        ///     A series Id for a series that will never be in TvDb
        /// </summary>
        internal class NonTvDbSeriesId
        {
            public NonTvDbSeriesId(string aniDbType)
            {
                AniDbType = aniDbType;
            }

            public string AniDbType { get; }
        }

        /// <summary>
        ///     A series Id for an unknown series not (yet) present in TvDb
        /// </summary>
        internal class UnknownSeriesId
        {
        }
    }
}