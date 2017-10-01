using System.Linq;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.PropertyMapping;
using MediaBrowser.Plugins.AniMetadata.TvDb;

namespace MediaBrowser.Plugins.AniMetadata.Configuration
{
    public class PluginConfiguration : BasePluginConfiguration
    {
        private PropertyMappingKeyCollection[] _episodeMappings;
        private PropertyMappingKeyCollection[] _seasonMappings;
        private PropertyMappingKeyCollection[] _seriesMappings;

        public PluginConfiguration()
        {
            TitlePreference = TitleType.Localized;
            MaxGenres = 5;
            MoveExcessGenresToTags = true;
            AddAnimeGenre = true;

            var mappingConfiguration = new MappingConfiguration(new ISourceMappingConfiguration[]
            {
                new AniDbSourceMappingConfiguration(new AniDbParser()),
                new TvDbSourceMappingConfiguration()
            });

            _seriesMappings = GetDefaultSeriesMappings(mappingConfiguration);
            _seasonMappings = GetDefaultSeasonMappings(mappingConfiguration);
            _episodeMappings = GetDefaultEpisodeMappings(mappingConfiguration);
        }

        public TitleType TitlePreference { get; set; }
        public int MaxGenres { get; set; }
        public bool MoveExcessGenresToTags { get; set; }
        public bool AddAnimeGenre { get; set; }

        public string TvDbApiKey { get; set; }

        public PropertyMappingKeyCollection[] SeriesMappings
        {
            get => _seriesMappings;
            set => _seriesMappings = MergeMappings(_seriesMappings, value);
        }

        public PropertyMappingKeyCollection[] SeasonMappings
        {
            get => _seasonMappings;
            set => _seasonMappings = MergeMappings(_seasonMappings, value);
        }

        public PropertyMappingKeyCollection[] EpisodeMappings
        {
            get => _episodeMappings;
            set => _episodeMappings = MergeMappings(_episodeMappings, value);
        }

        private PropertyMappingKeyCollection[] GetDefaultSeriesMappings(IMappingConfiguration mappingConfiguration)
        {
            var propertyMappings =
                mappingConfiguration.GetSeriesMappings(MaxGenres, MoveExcessGenresToTags, AddAnimeGenre);

            return ToCollection(propertyMappings);
        }

        private PropertyMappingKeyCollection[] GetDefaultSeasonMappings(IMappingConfiguration mappingConfiguration)
        {
            var propertyMappings = mappingConfiguration.GetSeasonMappings(MaxGenres, AddAnimeGenre);

            return ToCollection(propertyMappings);
        }

        private PropertyMappingKeyCollection[] GetDefaultEpisodeMappings(IMappingConfiguration mappingConfiguration)
        {
            var propertyMappings = mappingConfiguration.GetEpisodeMappings();

            return ToCollection(propertyMappings);
        }

        private PropertyMappingKeyCollection[] ToCollection(IPropertyMappingCollection propertyMappings)
        {
            return propertyMappings
                .Select(m => new PropertyMappingKey(m.SourceName, m.TargetPropertyName))
                .GroupBy(m => m.TargetPropertyName)
                .Select(g =>
                    new PropertyMappingKeyCollection(g.Key, g.Concat(new[] { new PropertyMappingKey("None", g.Key) })))
                .ToArray();
        }

        private PropertyMappingKeyCollection[] MergeMappings(PropertyMappingKeyCollection[] defaults,
            PropertyMappingKeyCollection[] configured)
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

        private PropertyMappingKeyCollection MergeMappings(PropertyMappingKeyCollection defaults,
            PropertyMappingKeyCollection configured)
        {
            var mergedMappings = configured.Mappings.Where(m => defaults.Mappings.Any(dm => AreEquivalent(dm, m)))
                .Concat(defaults.Mappings.Where(dm =>
                    !configured.Mappings.Any(m => AreEquivalent(dm, m))));

            configured.Mappings = mergedMappings.ToArray();

            return configured;
        }

        private bool AreEquivalent(PropertyMappingKey keyA, PropertyMappingKey keyB)
        {
            return keyA.SourceName == keyB.SourceName && keyA.TargetPropertyName == keyB.TargetPropertyName;
        }

        private bool AreEquivalent(PropertyMappingKeyCollection keyCollectionA,
            PropertyMappingKeyCollection keyCollectionB)
        {
            return keyCollectionA.TargetPropertyName == keyCollectionB.TargetPropertyName;
        }
    }
}