using System.Collections.Generic;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;

namespace MediaBrowser.Plugins.AniMetadata.Mapping
{
    public interface ISeriesMapping
    {
        int DefaultTvDbEpisodeIndexOffset { get; }

        Either<AbsoluteTvDbSeason, TvDbSeason> DefaultTvDbSeason { get; }

        IEnumerable<EpisodeGroupMapping> EpisodeGroupMappings { get; }

        SeriesIds Ids { get; }

        IEnumerable<SpecialEpisodePosition> SpecialEpisodePositions { get; }

        Option<EpisodeGroupMapping> GetEpisodeGroupMapping(IAniDbEpisodeNumber aniDbEpisodeNumber);

        Option<EpisodeGroupMapping> GetEpisodeGroupMapping(int tvDbEpisodeIndex, int tvDbSeasonIndex);

        Option<SpecialEpisodePosition> GetSpecialEpisodePosition(IAniDbEpisodeNumber aniDbEpisodeNumber);
    }
}