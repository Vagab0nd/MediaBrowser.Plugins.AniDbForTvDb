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
    }
}