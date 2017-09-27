using System.Linq;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.TvDb;

namespace MediaBrowser.Plugins.AniMetadata.Configuration
{
    public class PluginConfiguration : BasePluginConfiguration
    {
        public PluginConfiguration()
        {
            TitlePreference = TitleType.Localized;
            MaxGenres = 5;
            MoveExcessGenresToTags = true;
            AddAnimeGenre = true;

            SeriesMappings = GetDefaultSeriesMappings();
        }

        public TitleType TitlePreference { get; set; }
        public int MaxGenres { get; set; }
        public bool MoveExcessGenresToTags { get; set; }
        public bool AddAnimeGenre { get; set; }

        public string TvDbApiKey { get; set; }

        public PropertyMappingKeyCollection[] SeriesMappings { get; set; }

        private PropertyMappingKeyCollection[] GetDefaultSeriesMappings()
        {
            var propertyMappings =
                new MappingConfiguration(new ISourceMappingConfiguration[]
                {
                    new AniDbSourceMappingConfiguration(new AniDbParser()),
                    new TvDbSourceMappingConfiguration()
                }).GetSeriesMappings(MaxGenres, MoveExcessGenresToTags,
                    AddAnimeGenre);

            return propertyMappings
                .Select(m => new PropertyMappingKey(m.SourceName, m.TargetPropertyName))
                .GroupBy(m => m.TargetPropertyName)
                .Select(g =>
                    new PropertyMappingKeyCollection(g.Key, g.Concat(new[] { new PropertyMappingKey("None", g.Key) })))
                .ToArray();
        }
    }
}