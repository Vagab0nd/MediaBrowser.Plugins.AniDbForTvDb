using System.Collections.Generic;
using MediaBrowser.Plugins.AniMetadata.PropertyMapping;

namespace MediaBrowser.Plugins.AniMetadata.Configuration
{
    /// <summary>
    ///     The global mapping configuration, provides mappings that map data based on all available sources
    /// </summary>
    public interface IMappingConfiguration
    {
        IEnumerable<PropertyMappingDefinition> GetSeriesMappingDefinitions();

        IPropertyMappingCollection GetSeriesMappings(int maxGenres, bool addAnimeGenre, bool moveExcessGenresToTags,
            TitleType preferredTitleType, string metadataLanguage);

        IEnumerable<PropertyMappingDefinition> GetSeasonMappingDefinitions();

        IPropertyMappingCollection GetSeasonMappings(int maxGenres, bool addAnimeGenre, TitleType preferredTitleType,
            string metadataLanguage);

        IEnumerable<PropertyMappingDefinition> GetEpisodeMappingDefinitions();

        IPropertyMappingCollection GetEpisodeMappings(int maxGenres, bool addAnimeGenre, bool moveExcessGenresToTags,
            TitleType preferredTitleType, string metadataLanguage);
    }
}