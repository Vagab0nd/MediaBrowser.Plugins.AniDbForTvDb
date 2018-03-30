using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Plugins.AniMetadata.PropertyMapping;

namespace MediaBrowser.Plugins.AniMetadata.Configuration
{
    internal class MappingConfiguration : IMappingConfiguration
    {
        private readonly IEnumerable<ISourceMappingConfiguration> _sourceMappingConfigurations;

        public MappingConfiguration(IEnumerable<ISourceMappingConfiguration> sourceMappingConfigurations)
        {
            _sourceMappingConfigurations = sourceMappingConfigurations;
        }

        public IEnumerable<PropertyMappingDefinition> GetEpisodeMappingDefinitions()
        {
            return _sourceMappingConfigurations.SelectMany(c =>
                c.GetEpisodeMappingDefinitions());
        }

        public IPropertyMappingCollection GetEpisodeMappings(int maxGenres, bool addAnimeGenre,
            bool moveExcessGenresToTags, TitleType preferredTitleType, string metadataLanguage)
        {
            return new PropertyMappingCollection(_sourceMappingConfigurations.SelectMany(c =>
                c.GetEpisodeMappings(maxGenres, addAnimeGenre, moveExcessGenresToTags, preferredTitleType,
                    metadataLanguage)));
        }

        public IEnumerable<PropertyMappingDefinition> GetSeriesMappingDefinitions()
        {
            return _sourceMappingConfigurations.SelectMany(c =>
                c.GetSeriesMappingDefinitions());
        }

        public IPropertyMappingCollection GetSeriesMappings(int maxGenres,
            bool addAnimeGenre, bool moveExcessGenresToTags, TitleType preferredTitleType, string metadataLanguage)
        {
            return new PropertyMappingCollection(_sourceMappingConfigurations.SelectMany(c =>
                c.GetSeriesMappings(maxGenres, addAnimeGenre, moveExcessGenresToTags, preferredTitleType,
                    metadataLanguage)));
        }

        public IEnumerable<PropertyMappingDefinition> GetSeasonMappingDefinitions()
        {
            return _sourceMappingConfigurations.SelectMany(c =>
                c.GetSeasonMappingDefinitions());
        }

        public IPropertyMappingCollection GetSeasonMappings(int maxGenres, bool addAnimeGenre,
            TitleType preferredTitleType, string metadataLanguage)
        {
            return new PropertyMappingCollection(_sourceMappingConfigurations.SelectMany(c =>
                c.GetSeasonMappings(maxGenres, addAnimeGenre, preferredTitleType, metadataLanguage)));
        }
    }
}