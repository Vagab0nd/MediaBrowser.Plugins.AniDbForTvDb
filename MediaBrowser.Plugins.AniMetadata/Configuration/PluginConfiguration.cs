using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.Process.Sources;
using MediaBrowser.Plugins.AniMetadata.TvDb;

namespace MediaBrowser.Plugins.AniMetadata.Configuration
{
    public class PluginConfiguration : BasePluginConfiguration, ITitlePreferenceConfiguration
    {
        private PropertyMappingDefinitionCollection[] _episodeMappings;
        private PropertyMappingDefinitionCollection[] _seasonMappings;
        private PropertyMappingDefinitionCollection[] _seriesMappings;

        public PluginConfiguration()
        {
            TitlePreference = TitleType.Localized;
            MaxGenres = 5;
            MoveExcessGenresToTags = true;
            AddAnimeGenre = true;
            ExcludedSeriesNames = "";
            LibraryStructureSourceName = SourceNames.AniDb;
            FileStructureSourceName = SourceNames.AniDb;

            var mappingConfiguration = new MappingConfiguration(new ISourceMappingConfiguration[]
            {
                new AniDbSourceMappingConfiguration(null, null),
                new TvDbSourceMappingConfiguration()
            });

            _seriesMappings = GetDefaultSeriesMappings(mappingConfiguration);
            _seasonMappings = GetDefaultSeasonMappings(mappingConfiguration);
            _episodeMappings = GetDefaultEpisodeMappings(mappingConfiguration);
        }

        public TitleType TitlePreference { get; set; }

        public string LibraryStructureSourceName { get; set; }

        public string FileStructureSourceName { get; set; }

        public int MaxGenres { get; set; }

        public bool MoveExcessGenresToTags { get; set; }

        public bool AddAnimeGenre { get; set; }

        public string ExcludedSeriesNames { get; set; }

        public string TvDbApiKey { get; set; }

        public PropertyMappingDefinitionCollection[] SeriesMappings
        {
            get => _seriesMappings;
            set => _seriesMappings = MergeMappings(_seriesMappings, value);
        }

        public PropertyMappingDefinitionCollection[] SeasonMappings
        {
            get => _seasonMappings;
            set => _seasonMappings = MergeMappings(_seasonMappings, value);
        }

        public PropertyMappingDefinitionCollection[] EpisodeMappings
        {
            get => _episodeMappings;
            set => _episodeMappings = MergeMappings(_episodeMappings, value);
        }

        private PropertyMappingDefinitionCollection[] GetDefaultSeriesMappings(
            IMappingConfiguration mappingConfiguration)
        {
            var propertyMappings = mappingConfiguration.GetSeriesMappingDefinitions();

            return ToCollection(propertyMappings);
        }

        private PropertyMappingDefinitionCollection[] GetDefaultSeasonMappings(
            IMappingConfiguration mappingConfiguration)
        {
            var propertyMappings = mappingConfiguration.GetSeasonMappingDefinitions();

            return ToCollection(propertyMappings);
        }

        private PropertyMappingDefinitionCollection[] GetDefaultEpisodeMappings(
            IMappingConfiguration mappingConfiguration)
        {
            var propertyMappings = mappingConfiguration.GetEpisodeMappingDefinitions();

            return ToCollection(propertyMappings);
        }

        private PropertyMappingDefinitionCollection[] ToCollection(
            IEnumerable<PropertyMappingDefinition> propertyMappings)
        {
            return propertyMappings
                .GroupBy(m => m.FriendlyName)
                .Select(g => new PropertyMappingDefinitionCollection(g.Key, g))
                .ToArray();
        }

        private PropertyMappingDefinitionCollection[] MergeMappings(PropertyMappingDefinitionCollection[] defaults,
            PropertyMappingDefinitionCollection[] configured)
        {
            if (configured == null)
            {
                return defaults;
            }

            return configured.Where(m => defaults.Any(dm => AreEquivalent(dm, m)))
                .Concat(defaults.Where(dm =>
                    !configured.Any(m => AreEquivalent(dm, m))))
                .Select(m => MergeMappings(defaults.Single(d => AreEquivalent(d, m)), m))
                .ToArray();
        }

        private PropertyMappingDefinitionCollection MergeMappings(PropertyMappingDefinitionCollection defaults,
            PropertyMappingDefinitionCollection configured)
        {
            var mergedMappings = configured.Mappings.Where(m => defaults.Mappings.Any(dm => AreEquivalent(dm, m)))
                .Concat(defaults.Mappings.Where(dm =>
                    !configured.Mappings.Any(m => AreEquivalent(dm, m))));

            configured.Mappings = mergedMappings.ToArray();

            return configured;
        }

        private bool AreEquivalent(PropertyMappingDefinition definitionA, PropertyMappingDefinition definitionB)
        {
            return definitionA.SourceName == definitionB.SourceName &&
                definitionA.TargetPropertyName == definitionB.TargetPropertyName;
        }

        private bool AreEquivalent(PropertyMappingDefinitionCollection definitionCollectionA,
            PropertyMappingDefinitionCollection definitionCollectionB)
        {
            return definitionCollectionA.FriendlyName == definitionCollectionB.FriendlyName;
        }
    }
}