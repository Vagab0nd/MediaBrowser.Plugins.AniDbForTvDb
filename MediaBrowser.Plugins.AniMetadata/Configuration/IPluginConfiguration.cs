using MediaBrowser.Plugins.AniMetadata.PropertyMapping;

namespace MediaBrowser.Plugins.AniMetadata.Configuration
{
    public interface IPluginConfiguration
    {
        bool AddAnimeGenre { get; set; }

        int MaxGenres { get; set; }

        bool MoveExcessGenresToTags { get; set; }

        TitleType TitlePreference { get; set; }

        LibraryStructure LibraryStructure { get; set; }

        string TvDbApiKey { get; set; }

        IPropertyMappingCollection GetSeriesMetadataMapping();

        IPropertyMappingCollection GetSeasonMetadataMapping();

        IPropertyMappingCollection GetEpisodeMetadataMapping();
    }
}