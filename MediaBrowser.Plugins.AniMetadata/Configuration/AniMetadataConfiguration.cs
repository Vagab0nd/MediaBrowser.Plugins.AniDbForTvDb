using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.AniList;
using MediaBrowser.Plugins.AniMetadata.Process;
using MediaBrowser.Plugins.AniMetadata.Process.Sources;
using MediaBrowser.Plugins.AniMetadata.PropertyMapping;

namespace MediaBrowser.Plugins.AniMetadata.Configuration
{
    internal class AniMetadataConfiguration : IPluginConfiguration, ITitlePreferenceConfiguration, IAnilistConfiguration
    {
        private readonly IMappingConfiguration mappingConfiguration;
        private readonly PluginConfiguration pluginConfiguration;
        private readonly ISources sources;

        public AniMetadataConfiguration(PluginConfiguration pluginConfiguration,
            IMappingConfiguration mappingConfiguration, ISources sources)
        {
            this.pluginConfiguration = pluginConfiguration;
            this.mappingConfiguration = mappingConfiguration;
            this.sources = sources;
        }

        public bool IsLinked => !string.IsNullOrWhiteSpace(AniListAuthorisationCode) ||
            !string.IsNullOrWhiteSpace(this.pluginConfiguration.AniListAccessToken);

        public string AuthorisationCode => AniListAuthorisationCode;

        public Option<string> AccessToken
        {
            get => string.IsNullOrWhiteSpace(this.pluginConfiguration.AniListAccessToken)
                ? Option<string>.None
                : this.pluginConfiguration.AniListAccessToken;
            set => this.pluginConfiguration.AniListAccessToken = value.IfNone(string.Empty);
        }

        public bool AddAnimeGenre
        {
            get => this.pluginConfiguration.AddAnimeGenre;
            set => this.pluginConfiguration.AddAnimeGenre = value;
        }

        public int MaxGenres
        {
            get => this.pluginConfiguration.MaxGenres;
            set => this.pluginConfiguration.MaxGenres = value;
        }

        public bool MoveExcessGenresToTags
        {
            get => this.pluginConfiguration.MoveExcessGenresToTags;
            set => this.pluginConfiguration.MoveExcessGenresToTags = value;
        }

        public TitleType TitlePreference
        {
            get => this.pluginConfiguration.TitlePreference;
            set => this.pluginConfiguration.TitlePreference = value;
        }

        public LibraryStructure LibraryStructure => LibraryStructure.AniDb;

        public string TvDbApiKey
        {
            get => this.pluginConfiguration.TvDbApiKey;
            set => this.pluginConfiguration.TvDbApiKey = value;
        }

        public string AniListAuthorisationCode
        {
            get => this.pluginConfiguration.AniListAuthorisationCode;
            set => this.pluginConfiguration.AniListAuthorisationCode = value;
        }

        public ISource FileStructureSource => this.sources.Get(this.pluginConfiguration.FileStructureSourceName);

        public ISource LibraryStructureSource => this.sources.Get(this.pluginConfiguration.LibraryStructureSourceName);

        public IEnumerable<string> ExcludedSeriesNames =>
            this.pluginConfiguration.ExcludedSeriesNames?.Split('\n').Select(n => n.Trim()) ??
            new List<string>();

        public IPropertyMappingCollection GetSeriesMetadataMapping(string metadataLanguage)
        {
            return GetConfiguredPropertyMappings(this.pluginConfiguration.SeriesMappings,
                this.mappingConfiguration.GetSeriesMappings(MaxGenres, AddAnimeGenre, MoveExcessGenresToTags,
                    TitlePreference, metadataLanguage));
        }

        public IPropertyMappingCollection GetSeasonMetadataMapping(string metadataLanguage)
        {
            return GetConfiguredPropertyMappings(this.pluginConfiguration.SeasonMappings,
                this.mappingConfiguration.GetSeasonMappings(MaxGenres, AddAnimeGenre, TitlePreference, metadataLanguage));
        }

        public IPropertyMappingCollection GetEpisodeMetadataMapping(string metadataLanguage)
        {
            return GetConfiguredPropertyMappings(this.pluginConfiguration.EpisodeMappings,
                this.mappingConfiguration.GetEpisodeMappings(MaxGenres, AddAnimeGenre, MoveExcessGenresToTags,
                    TitlePreference, metadataLanguage));
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