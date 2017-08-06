using System;
using System.Collections.Generic;
using System.Linq;
using FunctionalSharp.DiscriminatedUnions;
using MediaBrowser.Plugins.Anime.AniDb.Data;
using MediaBrowser.Plugins.Anime.Mapping.Data;

namespace MediaBrowser.Plugins.Anime.AniDb.Mapping
{
    public class AniDbMapper
    {
        private readonly AnimeMappingList _animeMappingList;

        public AniDbMapper(AnimeMappingList animeMappingList)
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

        public DiscriminatedUnion<TvDbEpisodeNumber, AbsoluteEpisodeNumber, UnmappedEpisodeNumber>
            GetMappedTvDbEpisodeId(int aniDbSeriesId, IAniDbEpisodeNumber aniDbEpisodeNumber)
        {
            var seriesMapping =
                _animeMappingList.AnimeSeriesMapping.First(m => m.AnidbId == aniDbSeriesId.ToString());

            var episodeMapping = GetEpisodeGroupMapping(seriesMapping.GroupMappingList, aniDbEpisodeNumber);

            var tvDbEpisodeNumber = Option.Optionify<TvDbEpisodeNumber>(null);

            episodeMapping.Match(
                m => tvDbEpisodeNumber = Option.Optionify(GetTvDbEpisodeNumber(m, aniDbEpisodeNumber)),
                () => tvDbEpisodeNumber = GetTvDbEpisodeNumber(seriesMapping, aniDbEpisodeNumber));

            var absoluteEpisodeNumber = GetAbsoluteEpisodeNumber(seriesMapping, aniDbEpisodeNumber);

            object matchedEpisodeNumber = new UnmappedEpisodeNumber();

            tvDbEpisodeNumber.Match(
                n => matchedEpisodeNumber = n,
                () =>
                    absoluteEpisodeNumber.Match(n => matchedEpisodeNumber = n,
                        () => { }));

            return new DiscriminatedUnion<TvDbEpisodeNumber, AbsoluteEpisodeNumber, UnmappedEpisodeNumber>(
                matchedEpisodeNumber);
        }

        private IOption<AnimeEpisodeGroupMapping> GetEpisodeGroupMapping(IEnumerable<AnimeEpisodeGroupMapping> mappings,
            IAniDbEpisodeNumber aniDbEpisodeNumber)
        {
            if (aniDbEpisodeNumber == null)
            {
                throw new ArgumentNullException(nameof(aniDbEpisodeNumber));
            }

            var mapping = mappings.FirstOrDefault(m =>
                m.AnidbSeason == (aniDbEpisodeNumber.Type == EpisodeType.Special ? 0 : 1) &&
                m.Start <= aniDbEpisodeNumber.Number &&
                m.End >= aniDbEpisodeNumber.Number);

            return Option.Optionify(mapping);
        }

        private IOption<AbsoluteEpisodeNumber> GetAbsoluteEpisodeNumber(AnimeSeriesMapping animeSeriesMapping,
            IAniDbEpisodeNumber aniDbEpisodeNumber)
        {
            AbsoluteEpisodeNumber episodeNumber = null;

            if (animeSeriesMapping.DefaultTvDbSeason == "a")
            {
                episodeNumber = new AbsoluteEpisodeNumber(aniDbEpisodeNumber.Number);
            }

            return Option.Optionify(episodeNumber);
        }

        private IOption<TvDbEpisodeNumber> GetTvDbEpisodeNumber(AnimeSeriesMapping animeSeriesMapping,
            IAniDbEpisodeNumber aniDbEpisodeNumber)
        {
            var tvDbEpisodeIndex = aniDbEpisodeNumber.Number + animeSeriesMapping.EpisodeOffset;

            var tvDbEpisodeNumber = int.TryParse(animeSeriesMapping.DefaultTvDbSeason, out int tvDbSeasonIndex)
                ? new TvDbEpisodeNumber(tvDbSeasonIndex, tvDbEpisodeIndex)
                : null;

            return Option.Optionify(tvDbEpisodeNumber);
        }

        private TvDbEpisodeNumber GetTvDbEpisodeNumber(AnimeEpisodeGroupMapping episodeGroupMapping,
            IAniDbEpisodeNumber aniDbEpisodeNumber)
        {
            var episodeMapping =
                episodeGroupMapping.ParsedMappings?.FirstOrDefault(m => m.AniDb == aniDbEpisodeNumber.Number);

            var tvDbEpisodeIndex = episodeMapping?.TvDb ?? aniDbEpisodeNumber.Number + episodeGroupMapping.Offset;

            return new TvDbEpisodeNumber(episodeGroupMapping.TvDbSeason, tvDbEpisodeIndex);
        }

        public class TvDbEpisodeNumber
        {
            public TvDbEpisodeNumber(int seasonIndex, int episodeIndex)
            {
                SeasonIndex = seasonIndex;
                EpisodeIndex = episodeIndex;
            }

            public int SeasonIndex { get; }

            public int EpisodeIndex { get; }
        }

        public class AbsoluteEpisodeNumber
        {
            public AbsoluteEpisodeNumber(int episodeIndex)
            {
                EpisodeIndex = episodeIndex;
            }

            public int EpisodeIndex { get; }
        }

        public class UnmappedEpisodeNumber
        {
        }

        /// <summary>
        ///     A TvDb series Id
        /// </summary>
        public class TvDbSeriesId
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
        public class NonTvDbSeriesId
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
        public class UnknownSeriesId
        {
        }
    }
}