using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Mapping.Data;

namespace MediaBrowser.Plugins.AniMetadata.Mapping
{
    public class SeriesMapping : ISeriesMapping
    {
        public SeriesMapping(SeriesIds ids, Either<AbsoluteTvDbSeason, TvDbSeason> defaultTvDbSeason, int defaultTvDbEpisodeIndexOffset,
            IEnumerable<EpisodeGroupMapping> episodeGroupMappings,
            IEnumerable<SpecialEpisodePosition> specialEpisodePositions)
        {
            Ids = ids;
            DefaultTvDbSeason = defaultTvDbSeason;
            DefaultTvDbEpisodeIndexOffset = defaultTvDbEpisodeIndexOffset;
            SpecialEpisodePositions = specialEpisodePositions ?? new List<SpecialEpisodePosition>();
            EpisodeGroupMappings = episodeGroupMappings ?? new List<EpisodeGroupMapping>();
        }

        public SeriesIds Ids { get; }

        public IEnumerable<EpisodeGroupMapping> EpisodeGroupMappings { get; }

        public Either<AbsoluteTvDbSeason, TvDbSeason> DefaultTvDbSeason { get; }

        public int DefaultTvDbEpisodeIndexOffset { get; }

        public IEnumerable<SpecialEpisodePosition> SpecialEpisodePositions { get; }

        public Option<EpisodeGroupMapping> GetEpisodeGroupMapping(IAniDbEpisodeNumber aniDbEpisodeNumber)
        {
            var mapping = EpisodeGroupMappings.FirstOrDefault(m =>
                m.AniDbSeasonIndex == aniDbEpisodeNumber.SeasonNumber &&
                m.CanMapAniDbEpisode(aniDbEpisodeNumber.Number));

            return mapping;
        }

        public Option<EpisodeGroupMapping> GetEpisodeGroupMapping(int tvDbEpisodeIndex, int tvDbSeasonIndex)
        {
            var mapping = EpisodeGroupMappings.FirstOrDefault(m =>
                m.TvDbSeasonIndex == tvDbSeasonIndex &&
                m.CanMapTvDbEpisode(tvDbEpisodeIndex));

            return mapping;
        }

        public Option<SpecialEpisodePosition> GetSpecialEpisodePosition(IAniDbEpisodeNumber aniDbEpisodeNumber)
        {
            if (aniDbEpisodeNumber.Type != EpisodeType.Special)
            {
                return Option<SpecialEpisodePosition>.None;
            }

            return SpecialEpisodePositions.FirstOrDefault(m => m.SpecialEpisodeIndex == aniDbEpisodeNumber.Number);
        }

        private static bool IsValidData(AniDbSeriesMappingData data)
        {
            return int.TryParse(data?.AnidbId, out int _) &&
            (int.TryParse(data?.DefaultTvDbSeason, out int _) ||
                data?.DefaultTvDbSeason == "a");
        }

        private static Either<AbsoluteTvDbSeason, TvDbSeason> GetTvDbSeasonResult(string defaultTvDbSeasonIndex)
        {
            if (defaultTvDbSeasonIndex == "a")
            {
                return new AbsoluteTvDbSeason();
            }

            var seasonIndex = int.Parse(defaultTvDbSeasonIndex);

            return new TvDbSeason(seasonIndex);
        }

        public static Option<SeriesMapping> FromData(AniDbSeriesMappingData data)
        {
            if (!IsValidData(data))
            {
                return Option<SeriesMapping>.None;
            }

            var ids = new SeriesIds(
                int.Parse(data.AnidbId),
                data.TvDbId.MaybeInt(),
                data.ImdbId.MaybeInt(),
                data.TmdbId.MaybeInt());

            var defaultTvDbSeason = GetTvDbSeasonResult(data.DefaultTvDbSeason);

            var defaultTvDbEpisodeIndexOffset = data.EpisodeOffset;

            var episodeGroupMappings = data.GroupMappingList?.Select(EpisodeGroupMapping.FromData)
                .Somes()
                .ToList() ?? new List<EpisodeGroupMapping>();

            var specialEpisodePositions = ParseSpecialEpisodePositionsString(data.SpecialEpisodePositionsString);

            return new SeriesMapping(ids, defaultTvDbSeason, defaultTvDbEpisodeIndexOffset,
                episodeGroupMappings, specialEpisodePositions);
        }

        private static IEnumerable<SpecialEpisodePosition> ParseSpecialEpisodePositionsString(
            string specialEpisodePositionsString)
        {
            return specialEpisodePositionsString?.Split(';')
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s =>
                {
                    var mappingComponents = s.Split('-');

                    if (mappingComponents.Length != 2)
                    {
                        return Option<SpecialEpisodePosition>.None;
                    }

                    var specialEpisodeIndex = mappingComponents[0].MaybeInt();
                    var followingStandardEpisodeIndex = mappingComponents[1].MaybeInt();

                    return specialEpisodeIndex.Bind(
                        index => followingStandardEpisodeIndex.Select(
                            followingIndex => new SpecialEpisodePosition(index, followingIndex)));
                })
                .Somes() ?? new List<SpecialEpisodePosition>();
        }
    }
}