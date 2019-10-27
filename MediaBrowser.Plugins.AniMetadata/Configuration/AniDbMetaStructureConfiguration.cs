using System.Collections.Generic;
using System.Linq;
using Emby.AniDbMetaStructure.AniList;
using Emby.AniDbMetaStructure.Process;
using Emby.AniDbMetaStructure.Process.Sources;
using Emby.AniDbMetaStructure.PropertyMapping;
using LanguageExt;
using MediaBrowser.Controller.Entities.TV;

namespace Emby.AniDbMetaStructure.Configuration
{
    internal class AniDbMetaStructureConfiguration : IPluginConfiguration, ITitlePreferenceConfiguration, IAnilistConfiguration
    {
        private readonly IMappingConfiguration mappingConfiguration;
        private readonly PluginConfiguration pluginConfiguration;
        private readonly ISources sources;

        public AniDbMetaStructureConfiguration(PluginConfiguration pluginConfiguration,
            IMappingConfiguration mappingConfiguration, ISources sources)
        {
            this.pluginConfiguration = pluginConfiguration;
            this.mappingConfiguration = mappingConfiguration;
            this.sources = sources;
        }

        public bool IsLinked => !string.IsNullOrWhiteSpace(this.AniListAuthorizationCode) ||
            !string.IsNullOrWhiteSpace(this.pluginConfiguration.AniListAccessToken);

        public string AuthorizationCode => this.AniListAuthorizationCode;

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

        public string AniListAuthorizationCode
        {
            get => this.pluginConfiguration.AniListAuthorisationCode;
            set => this.pluginConfiguration.AniListAuthorisationCode = value;
        }

        public ISource FileStructureSource(IMediaItemType itemType)
        {
            var fileSource = this.sources.Get(SourceNames.TvDb);

            if (itemType == MediaItemTypes.Episode)
            {
                fileSource = this.sources.Get(SourceNames.AniDb);
            }

            return fileSource;
        }

        public ISource LibraryStructureSource(IMediaItemType itemType)
        {
            var librarySource = this.sources.Get(SourceNames.AniDb);

            if (itemType == MediaItemTypes.Series)
            {
                librarySource = this.sources.Get(SourceNames.TvDb);
            }

            return librarySource;
        }

        public IEnumerable<string> ExcludedSeriesNames =>
            this.pluginConfiguration.ExcludedSeriesNames?.Split('\n').Select(n => n.Trim()) ??
            new List<string>();

        public IPropertyMappingCollection GetSeriesMetadataMapping(string metadataLanguage)
        {
            return this.GetConfiguredPropertyMappings(this.pluginConfiguration.SeriesMappings,
                this.mappingConfiguration.GetSeriesMappings(this.MaxGenres, this.AddAnimeGenre, this.MoveExcessGenresToTags,
                    this.TitlePreference, metadataLanguage));
        }

        public IPropertyMappingCollection GetSeasonMetadataMapping(string metadataLanguage)
        {
            return this.GetConfiguredPropertyMappings(this.pluginConfiguration.SeasonMappings,
                this.mappingConfiguration.GetSeasonMappings(this.MaxGenres, this.AddAnimeGenre, this.TitlePreference, metadataLanguage));
        }

        public IPropertyMappingCollection GetEpisodeMetadataMapping(string metadataLanguage)
        {
            return this.GetConfiguredPropertyMappings(this.pluginConfiguration.EpisodeMappings,
                this.mappingConfiguration.GetEpisodeMappings(this.MaxGenres, this.AddAnimeGenre, this.MoveExcessGenresToTags,
                    this.TitlePreference, metadataLanguage));
        }

        private IPropertyMappingCollection GetConfiguredPropertyMappings(
            IEnumerable<PropertyMappingDefinitionCollection> configuredMappings,
            IEnumerable<IPropertyMapping> availableMappings)
        {
            return new PropertyMappingCollection(configuredMappings.SelectMany(cm =>
                cm.Mappings.Join(availableMappings, this.ToKey, this.ToKey, (configured, available) => available)));
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