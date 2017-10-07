using System.Collections.Generic;
using System.Linq;
using LanguageExt;
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
                    c.GetEpisodeMappingDefinitions())
                .Apply(AddNullMappingDefinitions);
        }

        public IPropertyMappingCollection GetEpisodeMappings(TitleType preferredTitleType, string metadataLanguage)
        {
            return new PropertyMappingCollection(_sourceMappingConfigurations.SelectMany(c =>
                    c.GetEpisodeMappings(preferredTitleType, metadataLanguage))
                .Apply(AddNullMappings));
        }

        public IEnumerable<PropertyMappingDefinition> GetSeriesMappingDefinitions()
        {
            return _sourceMappingConfigurations.SelectMany(c =>
                    c.GetSeriesMappingDefinitions())
                .Apply(AddNullMappingDefinitions);
        }

        public IPropertyMappingCollection GetSeriesMappings(int maxGenres,
            bool addAnimeGenre, bool moveExcessGenresToTags, TitleType preferredTitleType, string metadataLanguage)
        {
            return new PropertyMappingCollection(_sourceMappingConfigurations.SelectMany(c =>
                    c.GetSeriesMappings(maxGenres, addAnimeGenre, moveExcessGenresToTags, preferredTitleType,
                        metadataLanguage))
                .Apply(AddNullMappings));
        }

        public IEnumerable<PropertyMappingDefinition> GetSeasonMappingDefinitions()
        {
            return _sourceMappingConfigurations.SelectMany(c =>
                    c.GetSeasonMappingDefinitions())
                .Apply(AddNullMappingDefinitions);
        }

        public IPropertyMappingCollection GetSeasonMappings(int maxGenres, bool addAnimeGenre,
            TitleType preferredTitleType, string metadataLanguage)
        {
            return new PropertyMappingCollection(_sourceMappingConfigurations.SelectMany(c =>
                    c.GetSeasonMappings(maxGenres, addAnimeGenre, preferredTitleType, metadataLanguage))
                .Apply(AddNullMappings));
        }

        private IEnumerable<IPropertyMapping> AddNullMappings(IEnumerable<IPropertyMapping> propertyMappings)
        {
            return propertyMappings.GroupBy(m => m.TargetPropertyName)
                .SelectMany(g => g.Concat(new[] { new NullMapping(g.Key) }));
        }

        private IEnumerable<PropertyMappingDefinition> AddNullMappingDefinitions(
            IEnumerable<PropertyMappingDefinition> propertyMappings)
        {
            return propertyMappings.GroupBy(m => m.TargetPropertyName)
                .SelectMany(g => g.Concat(new[] { new PropertyMappingDefinition("None", g.Key) }));
        }
    }
}