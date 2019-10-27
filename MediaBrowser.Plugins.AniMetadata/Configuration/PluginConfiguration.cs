using System.Collections.Generic;
using System.Linq;
using Emby.AniDbMetaStructure.AniDb;
using Emby.AniDbMetaStructure.AniList;
using Emby.AniDbMetaStructure.Process.Sources;
using Emby.AniDbMetaStructure.TvDb;
using MediaBrowser.Model.Plugins;

namespace Emby.AniDbMetaStructure.Configuration
{
    public class PluginConfiguration : BasePluginConfiguration
    {
        private PropertyMappingDefinitionCollection[] episodeMappings;
        private PropertyMappingDefinitionCollection[] seasonMappings;
        private PropertyMappingDefinitionCollection[] seriesMappings;

        public PluginConfiguration()
        {
            this.TitlePreference = TitleType.Localized;
            this.MaxGenres = 5;
            this.MoveExcessGenresToTags = true;
            this.AddAnimeGenre = true;
            this.ExcludedSeriesNames = string.Empty;
            this.AniListAuthorisationCode = string.Empty;
            this.LibraryStructureSourceName = SourceNames.AniDb;
            this.FileStructureSourceName = SourceNames.AniDb;

            var mappingConfiguration = new MappingConfiguration(new ISourceMappingConfiguration[]
            {
                new AniDbSourceMappingConfiguration(null, null),
                new TvDbSourceMappingConfiguration(),
                new AniListSourceMappingConfiguration(null)
            });

            this.seriesMappings = this.GetDefaultSeriesMappings(mappingConfiguration);
            this.seasonMappings = this.GetDefaultSeasonMappings(mappingConfiguration);
            this.episodeMappings = this.GetDefaultEpisodeMappings(mappingConfiguration);
        }

        public string LibraryStructureSourceName { get; set; }

        public string FileStructureSourceName { get; set; }

        public int MaxGenres { get; set; }

        public bool MoveExcessGenresToTags { get; set; }

        public bool AddAnimeGenre { get; set; }

        public string ExcludedSeriesNames { get; set; }

        public string TvDbApiKey { get; set; }

        public string AniListAuthorisationCode { get; set; }

        public string AniListAccessToken { get; set; }

        public PropertyMappingDefinitionCollection[] SeriesMappings
        {
            get => this.seriesMappings;
            set => this.seriesMappings = this.MergeMappings(this.seriesMappings, value);
        }

        public PropertyMappingDefinitionCollection[] SeasonMappings
        {
            get => this.seasonMappings;
            set => this.seasonMappings = this.MergeMappings(this.seasonMappings, value);
        }

        public PropertyMappingDefinitionCollection[] EpisodeMappings
        {
            get => this.episodeMappings;
            set => this.episodeMappings = this.MergeMappings(this.episodeMappings, value);
        }

        public TitleType TitlePreference { get; set; }

        private PropertyMappingDefinitionCollection[] GetDefaultSeriesMappings(
            IMappingConfiguration mappingConfiguration)
        {
            var propertyMappings = mappingConfiguration.GetSeriesMappingDefinitions();

            return this.ToCollection(propertyMappings);
        }

        private PropertyMappingDefinitionCollection[] GetDefaultSeasonMappings(
            IMappingConfiguration mappingConfiguration)
        {
            var propertyMappings = mappingConfiguration.GetSeasonMappingDefinitions();

            return this.ToCollection(propertyMappings);
        }

        private PropertyMappingDefinitionCollection[] GetDefaultEpisodeMappings(
            IMappingConfiguration mappingConfiguration)
        {
            var propertyMappings = mappingConfiguration.GetEpisodeMappingDefinitions();

            return this.ToCollection(propertyMappings);
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

            return configured.Where(m => defaults.Any(dm => this.AreEquivalent(dm, m)))
                .Concat(defaults.Where(dm =>
                    !configured.Any(m => this.AreEquivalent(dm, m))))
                .Select(m => this.MergeMappings(defaults.Single(d => this.AreEquivalent(d, m)), m))
                .ToArray();
        }

        private PropertyMappingDefinitionCollection MergeMappings(PropertyMappingDefinitionCollection defaults,
            PropertyMappingDefinitionCollection configured)
        {
            var mergedMappings = configured.Mappings.Where(m => defaults.Mappings.Any(dm => this.AreEquivalent(dm, m)))
                .Concat(defaults.Mappings.Where(dm =>
                    !configured.Mappings.Any(m => this.AreEquivalent(dm, m))));

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