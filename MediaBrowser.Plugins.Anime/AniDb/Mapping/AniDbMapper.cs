using System;
using System.Collections.Generic;
using System.Linq;
using Functional.Maybe;
using FunctionalSharp.DiscriminatedUnions;
using MediaBrowser.Plugins.Anime.AniDb.Mapping.Data;
using MediaBrowser.Plugins.Anime.AniDb.Series;
using MediaBrowser.Plugins.Anime.AniDb.Series.Data;

namespace MediaBrowser.Plugins.Anime.AniDb.Mapping
{
    public class AniDbMapper
    {
        private readonly AnimeMappingListData _animeMappingListData;

        public AniDbMapper(AnimeMappingListData animeMappingListData)
        {
            _animeMappingListData = animeMappingListData;
        }

        public Maybe<AniDbSeriesMap> GetMappedSeriesIds(int aniDbSeriesId)
        {
            var mapping =
                _animeMappingListData.AnimeSeriesMapping.FirstOrDefault(m => m.AnidbId == aniDbSeriesId.ToString())
                    .ToMaybe();

            var result = Maybe<AniDbSeriesMap>.Nothing;

            var intParser = MaybeFunctionalWrappers.Wrap<string, int>(int.TryParse);

            mapping.Match(m =>
                {
                    var tvDbSeriesId = intParser(m.TvDbId);
                    var imdbSeriesId = intParser(m.ImdbId);
                    var tmdbSeriesId = intParser(m.TmdbId);

                    result = new AniDbSeriesMap(tvDbSeriesId, imdbSeriesId, tmdbSeriesId).ToMaybe();
                },
                () => { });

            return result;
        }

        public DiscriminatedUnion<TvDbEpisodeNumber, AbsoluteEpisodeNumber, UnmappedEpisodeNumber>
            GetMappedTvDbEpisodeId(int aniDbSeriesId, IAniDbEpisodeNumber aniDbEpisodeNumber)
        {
            var seriesMapping =
                _animeMappingListData.AnimeSeriesMapping.First(m => m.AnidbId == aniDbSeriesId.ToString());

            var episodeMapping = GetEpisodeGroupMapping(seriesMapping.GroupMappingList, aniDbEpisodeNumber);

            var tvDbEpisodeNumber = Maybe<TvDbEpisodeNumber>.Nothing;

            episodeMapping.Match(
                m => tvDbEpisodeNumber = GetTvDbEpisodeNumber(m, aniDbEpisodeNumber).ToMaybe(),
                () => tvDbEpisodeNumber = GetTvDbEpisodeNumber(seriesMapping, aniDbEpisodeNumber));

            var absoluteEpisodeNumber = GetAbsoluteEpisodeNumber(seriesMapping, aniDbEpisodeNumber);

            object matchedEpisodeNumber = new UnmappedEpisodeNumber();

            tvDbEpisodeNumber.Match(
                n => matchedEpisodeNumber = n,
                () => absoluteEpisodeNumber.Do(n => matchedEpisodeNumber = n));

            return new DiscriminatedUnion<TvDbEpisodeNumber, AbsoluteEpisodeNumber, UnmappedEpisodeNumber>(
                matchedEpisodeNumber);
        }

        private Maybe<AnimeEpisodeGroupMappingData> GetEpisodeGroupMapping(IEnumerable<AnimeEpisodeGroupMappingData> mappings,
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

            return mapping.ToMaybe();
        }

        private Maybe<AbsoluteEpisodeNumber> GetAbsoluteEpisodeNumber(AniDbSeriesMappingData aniDbSeriesMappingData,
            IAniDbEpisodeNumber aniDbEpisodeNumber)
        {
            AbsoluteEpisodeNumber episodeNumber = null;

            if (aniDbSeriesMappingData.DefaultTvDbSeason == "a")
            {
                episodeNumber = new AbsoluteEpisodeNumber(aniDbEpisodeNumber.Number);
            }

            return episodeNumber.ToMaybe();
        }

        private Maybe<TvDbEpisodeNumber> GetTvDbEpisodeNumber(AniDbSeriesMappingData aniDbSeriesMappingData,
            IAniDbEpisodeNumber aniDbEpisodeNumber)
        {
            var tvDbEpisodeIndex = aniDbEpisodeNumber.Number + aniDbSeriesMappingData.EpisodeOffset;

            var tvDbEpisodeNumber = int.TryParse(aniDbSeriesMappingData.DefaultTvDbSeason, out int tvDbSeasonIndex)
                ? new TvDbEpisodeNumber(tvDbSeasonIndex, tvDbEpisodeIndex)
                : null;

            return tvDbEpisodeNumber.ToMaybe();
        }

        private TvDbEpisodeNumber GetTvDbEpisodeNumber(AnimeEpisodeGroupMappingData episodeGroupMappingData,
            IAniDbEpisodeNumber aniDbEpisodeNumber)
        {
            var episodeMapping =
                episodeGroupMappingData.ParsedMappings?.FirstOrDefault(m => m.AniDb == aniDbEpisodeNumber.Number);

            var tvDbEpisodeIndex = episodeMapping?.TvDb ?? aniDbEpisodeNumber.Number + episodeGroupMappingData.Offset;

            return new TvDbEpisodeNumber(episodeGroupMappingData.TvDbSeason, tvDbEpisodeIndex);
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

        public class AniDbSeriesMap
        {
            public AniDbSeriesMap(Maybe<int> tvDbSeriesId, Maybe<int> imdbSeriesId, Maybe<int> tmDbSeriesId)
            {
                TvDbSeriesId = tvDbSeriesId;
                ImdbSeriesId = imdbSeriesId;
                TmDbSeriesId = tmDbSeriesId;
            }

            public Maybe<int> TvDbSeriesId { get; }

            public Maybe<int> ImdbSeriesId { get; }

            public Maybe<int> TmDbSeriesId { get; }
        }
    }
}