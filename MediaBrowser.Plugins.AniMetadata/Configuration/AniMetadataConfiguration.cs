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
            set => _pluginConfiguration.LibraryStructure = value;
        }

        public string TvDbApiKey
        {
            get => _pluginConfiguration.TvDbApiKey;
            set => _pluginConfiguration.TvDbApiKey = value;
        }

        public IPropertyMappingCollection GetSeriesMetadataMapping()
        {
            return new PropertyMappingCollection(_pluginConfiguration.SeriesMappings.SelectMany(sm =>
                sm.Mappings.Select(m =>
                    _mappingConfiguration.GetSeriesMappings(MaxGenres, MoveExcessGenresToTags, AddAnimeGenre)
                        .Single(pm =>
                            pm.SourceName == m.SourceName && pm.TargetPropertyName == m.TargetPropertyName))));
        }

        public IPropertyMappingCollection GetSeasonMetadataMapping()
        {
            return new PropertyMappingCollection(_pluginConfiguration.SeasonMappings.SelectMany(sm =>
                sm.Mappings.Select(m =>
                    _mappingConfiguration.GetSeasonMappings(MaxGenres, AddAnimeGenre)
                        .Single(pm =>
                            pm.SourceName == m.SourceName && pm.TargetPropertyName == m.TargetPropertyName))));
        }

        public IPropertyMappingCollection GetEpisodeMetadataMapping()
        {
            return new PropertyMappingCollection(_pluginConfiguration.EpisodeMappings.SelectMany(sm =>
                sm.Mappings.Select(m =>
                    _mappingConfiguration.GetEpisodeMappings()
                        .Single(pm =>
                            pm.SourceName == m.SourceName && pm.TargetPropertyName == m.TargetPropertyName))));
        }
    }
}