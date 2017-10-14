using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Plugins.AniMetadata.Providers;

namespace MediaBrowser.Plugins.AniMetadata.AniDb
{
    public interface IEpisodeMetadataFactory
    {
        MetadataResult<Episode> NullResult { get; }

        MetadataResult<Episode> CreateMetadata(EpisodeData episodeData, string metadataLanguage);
    }
}