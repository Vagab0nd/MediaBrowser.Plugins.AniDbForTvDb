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

        public IPropertyMappingCollection GetSeriesMappings(int maxGenres, bool moveExcessGenresToTags,
            bool addAnimeGenre)
        {
            return new PropertyMappingCollection(_sourceMappingConfigurations.SelectMany(c =>
                c.GetSeriesMappings(maxGenres, addAnimeGenre, moveExcessGenresToTags)));
        }

        public IPropertyMappingCollection GetSeasonMappings(int maxGenres, bool addAnimeGenre)
        {
            return new PropertyMappingCollection(_sourceMappingConfigurations.SelectMany(c =>
                c.GetSeasonMappings(maxGenres, addAnimeGenre)));
        }

        public IPropertyMappingCollection GetEpisodeMappings()
        {
            return new PropertyMappingCollection(_sourceMappingConfigurations.SelectMany(c =>
                c.GetEpisodeMappings()));
        }
    }
}