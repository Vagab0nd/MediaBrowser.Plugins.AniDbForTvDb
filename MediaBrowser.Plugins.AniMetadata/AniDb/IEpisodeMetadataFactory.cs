using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Plugins.AniMetadata.AniDb.Mapping;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;

namespace MediaBrowser.Plugins.AniMetadata.AniDb
{
    public interface IEpisodeMetadataFactory
    {
        MetadataResult<Episode> NullResult { get; }

        MetadataResult<Episode> CreateMetadata(AniDbEpisodeData aniDbEpisodeData,
            MappedEpisodeResult mappedEpisodeResult, string metadataLanguage);

        MetadataResult<Episode> CreateMetadata(AniDbEpisodeData aniDbEpisodeData, TvDbEpisodeData tvDbEpisodeData,
            MappedEpisodeResult mappedEpisodeResult, string metadataLanguage);
    }
}