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

        public IPropertyMappingCollection GetSeriesMappings(int maxGenres,
            bool addAnimeGenre, bool moveExcessGenresToTags)
        {
            return new PropertyMappingCollection(_sourceMappingConfigurations.SelectMany(c =>
                c.GetSeriesMappings(maxGenres, addAnimeGenre, moveExcessGenresToTags)).Apply(AddNullMappings));
        }

        public IPropertyMappingCollection GetSeasonMappings(int maxGenres, bool addAnimeGenre)
        {
            return new PropertyMappingCollection(_sourceMappingConfigurations.SelectMany(c =>
                c.GetSeasonMappings(maxGenres, addAnimeGenre)).Apply(AddNullMappings));
        }

        public IPropertyMappingCollection GetEpisodeMappings()
        {
            return new PropertyMappingCollection(_sourceMappingConfigurations.SelectMany(c =>
                c.GetEpisodeMappings()).Apply(AddNullMappings));
        }

        private IEnumerable<IPropertyMapping> AddNullMappings(IEnumerable<IPropertyMapping> propertyMappings)
        {
            return propertyMappings.GroupBy(m => m.TargetPropertyName)
                .SelectMany(g => g.Concat(new[] { new NullMapping(g.Key) }));
        }
    }
}