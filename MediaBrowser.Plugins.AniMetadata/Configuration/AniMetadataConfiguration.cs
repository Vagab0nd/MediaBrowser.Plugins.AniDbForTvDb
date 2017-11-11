using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Plugins.AniMetadata.PropertyMapping;

namespace MediaBrowser.Plugins.AniMetadata.Configuration
{
    internal class AniMetadataConfiguration : IPluginConfiguration
    {
        private readonly IMappingConfiguration _mappingConfiguration;
        private readonly PluginConfiguration _pluginConfiguration;

        public AniMetadataConfiguration(PluginConfiguration pluginConfiguration,
            IMappingConfiguration mappingConfiguration)
        {
            _pluginConfiguration = pluginConfiguration;
            _mappingConfiguration = mappingConfiguration;
        }

        public bool AddAnimeGenre
        {
            get => _pluginConfiguration.AddAnimeGenre;
            set => _pluginConfiguration.AddAnimeGenre = value;
        }

        public int MaxGenres
        {
            get => _pluginConfiguration.MaxGenres;
            set => _pluginConfiguration.MaxGenres = value;
        }

        public bool MoveExcessGenresToTags
        {
            get => _pluginConfiguration.MoveExcessGenresToTags;
            set => _pluginConfiguration.MoveExcessGenresToTags = value;
        }

        public TitleType TitlePreference
        {
            get => _pluginConfiguration.TitlePreference;
            set => _pluginConfiguration.TitlePreference = value;
        }

        public LibraryStructure LibraryStructure
        {
            get => _pluginConfiguration.LibraryStructure;
        }

        public string TvDbApiKey
        {
            get => _pluginConfiguration.TvDbApiKey;
            set => _pluginConfiguration.TvDbApiKey = value;
        }

        public IPropertyMappingCollection GetSeriesMetadataMapping(string metadataLanguage)
        {
            return GetConfiguredPropertyMappings(_pluginConfiguration.SeriesMappings,
                _mappingConfiguration.GetSeriesMappings(MaxGenres, AddAnimeGenre, MoveExcessGenresToTags,
                    TitlePreference, metadataLanguage));
        }

        public IPropertyMappingCollection GetSeasonMetadataMapping(string metadataLanguage)
        {
            return GetConfiguredPropertyMappings(_pluginConfiguration.SeasonMappings,
                _mappingConfiguration.GetSeasonMappings(MaxGenres, AddAnimeGenre, TitlePreference, metadataLanguage));
        }

        public IPropertyMappingCollection GetEpisodeMetadataMapping(string metadataLanguage)
        {
            return GetConfiguredPropertyMappings(_pluginConfiguration.EpisodeMappings,
                _mappingConfiguration.GetEpisodeMappings(TitlePreference, metadataLanguage));
        }

        private IPropertyMappingCollection GetConfiguredPropertyMappings(
            IEnumerable<PropertyMappingDefinitionCollection> configuredMappings,
            IEnumerable<IPropertyMapping> availableMappings)
        {
            return new PropertyMappingCollection(configuredMappings.SelectMany(cm =>
                cm.Mappings.Join(availableMappings, ToKey, ToKey, (configured, available) => available)));
        }

        private string ToKey(IPropertyMapping propertyMapping)
        {
            return $"{propertyMapping.SourceName}|{propertyMapping.TargetPropertyName}";
        }

        private string ToKey(PropertyMappingDefinition propertyMappingDefinition)
        {
            return $"{propertyMappingDefinition.SourceName}|{propertyMappingDefinition.TargetPropertyName}";
        }
    }
}