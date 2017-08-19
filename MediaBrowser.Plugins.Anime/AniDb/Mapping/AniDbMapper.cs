using System.Collections.Generic;
using System.Linq;
using Functional.Maybe;
using FunctionalSharp.DiscriminatedUnions;
using MediaBrowser.Plugins.Anime.AniDb.Series;
using MediaBrowser.Plugins.Anime.AniDb.Series.Data;

namespace MediaBrowser.Plugins.Anime.AniDb.Mapping
{
    public class AniDbMapper
    {
        private readonly IMappingList _aniDbMappingList;

        public AniDbMapper(IMappingList aniDbMappingList)
        {
            _aniDbMappingList = aniDbMappingList;
        }

        public Maybe<SeriesIds> GetMappedSeriesIds(int aniDbSeriesId)
        {
            var mapping = _aniDbMappingList.GetSeriesMapping(aniDbSeriesId);

            return mapping.Select(m => m.Ids);
        }

        public MappedEpisodeResult GetMappedTvDbEpisodeId(int aniDbSeriesId, IAniDbEpisodeNumber aniDbEpisodeNumber)
        {
            var result = new MappedEpisodeResult(new UnmappedEpisodeNumber());

            if (aniDbEpisodeNumber == null)
            {
                return result;
            }

            var seriesMapping = _aniDbMappingList.GetSeriesMapping(aniDbSeriesId);

            seriesMapping.Do(sm =>
            {
                var episodeMapping = GetEpisodeGroupMapping(sm.EpisodeGroupMappings,
                    aniDbEpisodeNumber);

                episodeMapping.Match(
                    m => result = new MappedEpisodeResult(GetTvDbEpisodeNumber(m, aniDbEpisodeNumber)),
                    () => sm.DefaultTvDbSeason.Match(
                        tvDbSeason => result =
                            new MappedEpisodeResult(new TvDbEpisodeNumber(tvDbSeason.Index,
                                aniDbEpisodeNumber.Number + sm.DefaultTvDbEpisodeIndexOffset)),
                        absoluteTvDbSeason => result =
                            new MappedEpisodeResult(new AbsoluteEpisodeNumber(aniDbEpisodeNumber.Number))));
            });

            return result;
        }

        private Maybe<EpisodeGroupMapping> GetEpisodeGroupMapping(IEnumerable<EpisodeGroupMapping> mappings,
            IAniDbEpisodeNumber aniDbEpisodeNumber)
        {
            var mapping = mappings.FirstOrDefault(m =>
                m.AniDbSeasonIndex == (aniDbEpisodeNumber.Type == EpisodeType.Special ? 0 : 1) &&
                m.CanMapEpisode(aniDbEpisodeNumber.Number));

            return mapping.ToMaybe();
        }

        private TvDbEpisodeNumber GetTvDbEpisodeNumber(EpisodeGroupMapping episodeGroupMapping,
            IAniDbEpisodeNumber aniDbEpisodeNumber)
        {
            var episodeMapping =
                episodeGroupMapping.EpisodeMappings?.FirstOrDefault(m => m.AniDbEpisodeIndex ==
                    aniDbEpisodeNumber.Number);

            var tvDbEpisodeIndex = episodeMapping?.TvDbEpisodeIndex ??
                aniDbEpisodeNumber.Number + episodeGroupMapping.TvDbEpisodeIndexOffset;

            return new TvDbEpisodeNumber(episodeGroupMapping.TvDbSeasonIndex, tvDbEpisodeIndex);
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

        public class MappedEpisodeResult : DiscriminatedUnion<TvDbEpisodeNumber, AbsoluteEpisodeNumber,
            UnmappedEpisodeNumber>
        {
            public MappedEpisodeResult(TvDbEpisodeNumber item) : base(item)
            {
            }

            public MappedEpisodeResult(AbsoluteEpisodeNumber item) : base(item)
            {
            }

            public MappedEpisodeResult(UnmappedEpisodeNumber item) : base(item)
            {
            }
        }
    }
}