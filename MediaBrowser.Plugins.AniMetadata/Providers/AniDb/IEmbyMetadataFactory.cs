using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Plugins.AniMetadata.AniDb.Mapping;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;

namespace MediaBrowser.Plugins.AniMetadata.Providers.AniDb
{
    public interface IEmbyMetadataFactory
    {
        MetadataResult<Series> NullSeriesResult { get; }

        MetadataResult<Season> NullSeasonResult { get; }

        MetadataResult<Episode> NullEpisodeResult { get; }

        MetadataResult<Series> CreateSeriesMetadataResult(AniDbSeriesData aniDbSeriesData, string metadataLanguage);

        MetadataResult<Season> CreateSeasonMetadataResult(AniDbSeriesData aniDbSeriesData, int seasonIndex,
            string metadataLanguage);

        MetadataResult<Episode> CreateEpisodeMetadataResult(EpisodeData episodeData, MappedEpisodeResult tvDbEpisode,
            string metadataLanguage);
    }
}