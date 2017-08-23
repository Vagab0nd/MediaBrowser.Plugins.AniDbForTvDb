using System.Collections.Generic;
using System.Linq;
using Functional.Maybe;
using MediaBrowser.Plugins.Anime.AniDb.Mapping.Data;

namespace MediaBrowser.Plugins.Anime.AniDb.Mapping
{
    public class SeriesMapping
    {
        public SeriesMapping(SeriesIds ids, TvDbSeasonResult defaultTvDbSeason, int defaultTvDbEpisodeIndexOffset,
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

        public TvDbSeasonResult DefaultTvDbSeason { get; }

        public int DefaultTvDbEpisodeIndexOffset { get; }

        public IEnumerable<SpecialEpisodePosition> SpecialEpisodePositions { get; }

        private static bool IsValidData(AniDbSeriesMappingData data)
        {
            return int.TryParse(data?.AnidbId, out int _) &&
            (int.TryParse(data?.DefaultTvDbSeason, out int _) ||
                data?.DefaultTvDbSeason == "a");
        }

        private static TvDbSeasonResult GetTvDbSeasonResult(string defaultTvDbSeasonIndex)
        {
            if (defaultTvDbSeasonIndex == "a")
            {
                return new TvDbSeasonResult(new AbsoluteTvDbSeason());
            }

            var seasonIndex = int.Parse(defaultTvDbSeasonIndex);

            return new TvDbSeasonResult(new TvDbSeason(seasonIndex));
        }

        public static Maybe<SeriesMapping> FromData(AniDbSeriesMappingData data)
        {
            if (!IsValidData(data))
            {
                return Maybe<SeriesMapping>.Nothing;
            }

            var ids = new SeriesIds(
                int.Parse(data.AnidbId),
                data.TvDbId.MaybeInt(),
                data.ImdbId.MaybeInt(),
                data.TmdbId.MaybeInt());

            var defaultTvDbSeason = GetTvDbSeasonResult(data.DefaultTvDbSeason);

            var defaultTvDbEpisodeIndexOffset = data.EpisodeOffset;

            var episodeGroupMappings = data.GroupMappingList?.Select(EpisodeGroupMapping.FromData)
                .SelectWhereValueExist(m => m).ToList() ?? new List<EpisodeGroupMapping>();

            var specialEpisodePositions = ParseSpecialEpisodePositionsString(data.SpecialEpisodePositionsString);

            return new SeriesMapping(ids, defaultTvDbSeason, defaultTvDbEpisodeIndexOffset,
                episodeGroupMappings, specialEpisodePositions).ToMaybe();
        }

        private static IEnumerable<SpecialEpisodePosition> ParseSpecialEpisodePositionsString(
            string specialEpisodePositionsString)
        {
            return specialEpisodePositionsString?.Split(';').Where(s => !string.IsNullOrWhiteSpace(s)).Select(s =>
            {
                var mappingComponents = s.Split('-');

                if (mappingComponents.Length != 2)
                {
                    return Maybe<SpecialEpisodePosition>.Nothing;
                }

                var specialEpisodeIndex = mappingComponents[0].MaybeInt();
                var followingStandardEpisodeIndex = mappingComponents[1].MaybeInt();

                return specialEpisodeIndex.Select(
                    index => followingStandardEpisodeIndex.Select(
                        followingIndex => new SpecialEpisodePosition(index, followingIndex)));
            }).SelectWhereValueExist(em => em) ?? new List<SpecialEpisodePosition>();
        }
    }
}