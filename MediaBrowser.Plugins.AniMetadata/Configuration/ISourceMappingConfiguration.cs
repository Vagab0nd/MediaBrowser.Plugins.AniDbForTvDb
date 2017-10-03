using System.Collections.Generic;
using MediaBrowser.Plugins.AniMetadata.PropertyMapping;

namespace MediaBrowser.Plugins.AniMetadata.Configuration
{
    /// <summary>
    ///     Provides a set of mappings for based on a single data source
    /// </summary>
    internal interface ISourceMappingConfiguration
    {
        IEnumerable<PropertyMappingDefinition> GetSeriesMappingDefinitions();

        IEnumerable<IPropertyMapping> GetSeriesMappings(int maxGenres, bool addAnimeGenre, bool moveExcessGenresToTags,
            TitleType preferredTitleType, string metadataLanguage);

        IEnumerable<PropertyMappingDefinition> GetSeasonMappingDefinitions();

        IEnumerable<IPropertyMapping> GetSeasonMappings(int maxGenres, bool addAnimeGenre, TitleType preferredTitleType,
            string metadataLanguage);

        IEnumerable<PropertyMappingDefinition> GetEpisodeMappingDefinitions();

        IEnumerable<IPropertyMapping> GetEpisodeMappings();
    }
}