using System.Collections.Generic;
using MediaBrowser.Plugins.AniMetadata.PropertyMapping;

namespace MediaBrowser.Plugins.AniMetadata.Configuration
{
    /// <summary>
    ///     Provides a set of mappings for based on a single data source
    /// </summary>
    internal interface ISourceMappingConfiguration
    {
        IEnumerable<IPropertyMapping> GetSeriesMappings(int maxGenres, bool addAnimeGenre, bool moveExcessGenresToTags);

        IEnumerable<IPropertyMapping> GetSeasonMappings(int maxGenres, bool addAnimeGenre);

        IEnumerable<IPropertyMapping> GetEpisodeMappings();
    }
}