using MediaBrowser.Plugins.AniMetadata.PropertyMapping;

namespace MediaBrowser.Plugins.AniMetadata.Configuration
{
    /// <summary>
    ///     The global mapping configuration, provides mappings that map data based on all available sources
    /// </summary>
    public interface IMappingConfiguration
    {
        IPropertyMappingCollection GetSeriesMappings(int maxGenres, bool addAnimeGenre, bool moveExcessGenresToTags);

        IPropertyMappingCollection GetSeasonMappings(int maxGenres, bool addAnimeGenre);

        IPropertyMappingCollection GetEpisodeMappings();
    }
}