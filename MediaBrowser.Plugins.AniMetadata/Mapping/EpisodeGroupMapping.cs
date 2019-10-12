using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.Mapping.Data;

namespace MediaBrowser.Plugins.AniMetadata.Mapping
{
    using Infrastructure;

    public class EpisodeGroupMapping
    {
        private readonly int endEpisodeIndex;
        private readonly int startEpisodeIndex;

        public EpisodeGroupMapping(int aniDbSeasonIndex, int tvDbSeasonIndex, int tvDbEpisodeIndexOffset,
            int? startEpisodeIndex, int? endEpisodeIndex, IEnumerable<EpisodeMapping> episodeMappings)
        {
            this.startEpisodeIndex = startEpisodeIndex.GetValueOrDefault();
            this.endEpisodeIndex = endEpisodeIndex.GetValueOrDefault();

            AniDbSeasonIndex = aniDbSeasonIndex;
            TvDbSeasonIndex = tvDbSeasonIndex;
            TvDbEpisodeIndexOffset = tvDbEpisodeIndexOffset;
            EpisodeMappings = episodeMappings;
        }

        public int AniDbSeasonIndex { get; }

        public int TvDbSeasonIndex { get; }

        public int TvDbEpisodeIndexOffset { get; }

        public IEnumerable<EpisodeMapping> EpisodeMappings { get; }

        private static bool IsValidData(AnimeEpisodeGroupMappingData data)
        {
            return data != null;
        }

        public static Option<EpisodeGroupMapping> FromData(AnimeEpisodeGroupMappingData data)
        {
            return IsValidData(data)
                ? new EpisodeGroupMapping(data.AnidbSeason, data.TvDbSeason, data.Offset, data.Start, data.End,
                    ParseEpisodeMappingString(data.EpisodeMappingString))
                : Option<EpisodeGroupMapping>.None;
        }

        public bool CanMapAniDbEpisode(int aniDbEpisodeIndex)
        {
            return aniDbEpisodeIndex >= this.startEpisodeIndex && aniDbEpisodeIndex <= this.endEpisodeIndex ||
                EpisodeMappings.Any(em => em.AniDbEpisodeIndex == aniDbEpisodeIndex);
        }

        public bool CanMapTvDbEpisode(int tvDbEpisodeIndex)
        {
            return (tvDbEpisodeIndex >= (this.startEpisodeIndex + TvDbEpisodeIndexOffset)) &&
                (tvDbEpisodeIndex <= (this.endEpisodeIndex + TvDbEpisodeIndexOffset)) ||
                EpisodeMappings.Any(em => em.TvDbEpisodeIndex == tvDbEpisodeIndex);
        }

        private static IEnumerable<EpisodeMapping> ParseEpisodeMappingString(string episodeMappingString)
        {
            return episodeMappingString?.Split(';')
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s =>
                {
                    var mappingComponents = s.Split('-');

                    if (mappingComponents.Length != 2)
                    {
                        return Option<EpisodeMapping>.None;
                    }

                    var aniDbEpisodeIndex = mappingComponents[0].MaybeInt();
                    var tvDbEpisodeIndex = mappingComponents[1].MaybeInt();

                    return aniDbEpisodeIndex.Bind(aniDbId =>
                        tvDbEpisodeIndex.Select(tvDbId => new EpisodeMapping(aniDbId, tvDbId)));
                })
                .Somes() ?? new List<EpisodeMapping>();
        }
    }
}