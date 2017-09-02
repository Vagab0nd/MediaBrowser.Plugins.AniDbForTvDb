using FunctionalSharp.DiscriminatedUnions;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Plugins.Anime.AniDb.Mapping;
using MediaBrowser.Plugins.Anime.AniDb.Series.Data;

namespace MediaBrowser.Plugins.Anime.Providers.AniDb
{
    public interface IEmbyMetadataFactory
    {
        MetadataResult<Series> NullSeriesResult { get; }

        MetadataResult<Season> NullSeasonResult { get; }

        MetadataResult<Episode> NullEpisodeResult { get; }

        MetadataResult<Series> CreateSeriesMetadataResult(AniDbSeriesData aniDbSeriesData, string metadataLanguage);

        MetadataResult<Season> CreateSeasonMetadataResult(AniDbSeriesData aniDbSeriesData, int seasonIndex,
            string metadataLanguage);

        MetadataResult<Episode> CreateEpisodeMetadataResult(EpisodeData episodeData,
            DiscriminatedUnion<TvDbEpisodeNumber, AbsoluteEpisodeNumber, UnmappedEpisodeNumber> tvDbEpisode,
            string metadataLanguage);
    }
}