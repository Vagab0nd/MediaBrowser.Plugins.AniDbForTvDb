using MediaBrowser.Plugins.AniMetadata.MetadataMapping;

namespace MediaBrowser.Plugins.AniMetadata.Configuration
{
    public interface IPluginConfiguration
    {
        bool AddAnimeGenre { get; set; }

        int MaxGenres { get; set; }

        bool MoveExcessGenresToTags { get; set; }

        PluginConfiguration.TargetPropertyMappings[] SeriesMappings { get; set; }

        TitleType TitlePreference { get; set; }

        string TvDbApiKey { get; set; }

        IMetadataMapping GetSeriesMetadataMapping();
    }
}