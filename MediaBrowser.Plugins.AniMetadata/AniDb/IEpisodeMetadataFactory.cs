using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Plugins.AniMetadata.AniDb.Mapping;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;

namespace MediaBrowser.Plugins.AniMetadata.AniDb
{
    public interface IEpisodeMetadataFactory
    {
        MetadataResult<Episode> NullEpisodeResult { get; }
        
        MetadataResult<Episode> CreateEpisodeMetadataResult(EpisodeData episodeData, MappedEpisodeResult tvDbEpisode,
            string metadataLanguage);
    }
}