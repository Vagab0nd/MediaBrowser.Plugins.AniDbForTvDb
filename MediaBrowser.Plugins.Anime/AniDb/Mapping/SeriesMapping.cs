using System.Collections.Generic;
using System.Linq;
using Functional.Maybe;
using MediaBrowser.Plugins.Anime.AniDb.Mapping.Data;

namespace MediaBrowser.Plugins.Anime.AniDb.Mapping
{
    public class SeriesMapping
    {
        public SeriesMapping(SeriesIds ids, TvDbSeasonResult defaultTvDbSeason, int defaultTvDbEpisodeIndexOffset,
            IEnumerable<EpisodeGroupMapping> episodeGroupMappings)
        {
            Ids = ids;
            DefaultTvDbSeason = defaultTvDbSeason;
            DefaultTvDbEpisodeIndexOffset = defaultTvDbEpisodeIndexOffset;
            EpisodeGroupMappings = episodeGroupMappings ?? new List<EpisodeGroupMapping>();
        }

        public SeriesIds Ids { get; }

        public IEnumerable<EpisodeGroupMapping> EpisodeGroupMappings { get; }

        public TvDbSeasonResult DefaultTvDbSeason { get; }

        public int DefaultTvDbEpisodeIndexOffset { get; }

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

            return new SeriesMapping(ids, defaultTvDbSeason, defaultTvDbEpisodeIndexOffset,
                episodeGroupMappings).ToMaybe();
        }
    }
}