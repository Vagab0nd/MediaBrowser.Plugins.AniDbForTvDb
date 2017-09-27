using System;
using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.AniDb.MetadataMapping;
using MediaBrowser.Plugins.AniMetadata.MetadataMapping;
using MediaBrowser.Plugins.AniMetadata.TvDb.MetadataMapping;

namespace MediaBrowser.Plugins.AniMetadata.Configuration
{
    public class PluginConfiguration : BasePluginConfiguration, IPluginConfiguration
    {
        private IEnumerable<IPropertyMapping> _propertyMappings;

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

        public TargetPropertyMappings[] SeriesMappings { get; set; }

        public IMetadataMapping GetSeriesMetadataMapping()
        {
            if (_propertyMappings == null)
            {
                throw new InvalidOperationException("Can't get series mappings before they are loaded");
            }

            return new MetadataMapping.MetadataMapping(SeriesMappings.SelectMany(sm => sm.Mappings.Select(m =>
                _propertyMappings.Single(pm =>
                    pm.SourceName == m.SourceName && pm.TargetPropertyName == m.TargetPropertyName))));
        }

        private TargetPropertyMappings[] GetDefaultSeriesMappings()
        {
            _propertyMappings =
                new SeriesMetadataMappingFactory(new AniDbSeriesMetadataMappings(new AniDbParser()),
                    new TvDbSeriesMetadataMappings()).GetSeriesMappings(MaxGenres, MoveExcessGenresToTags,
                    AddAnimeGenre);

            return _propertyMappings
                .Select(m => new MappingKey(m.SourceName, m.TargetPropertyName))
                .GroupBy(m => m.TargetPropertyName)
                .Select(g => new TargetPropertyMappings(g.Key, g.Concat(new[] { new MappingKey("None", g.Key) })))
                .ToArray();
        }

        public class TargetPropertyMappings
        {
            public TargetPropertyMappings()
            {
            }

            public TargetPropertyMappings(string targetPropertyName, IEnumerable<MappingKey> mappings)
            {
                TargetPropertyName = targetPropertyName;
                Mappings = mappings.ToArray();
            }

            public string TargetPropertyName { get; set; }

            public MappingKey[] Mappings { get; set; }
        }

        public class MappingKey
        {
            public MappingKey()
            {
            }

            public MappingKey(string sourceName, string targetPropertyName)
            {
                SourceName = sourceName;
                TargetPropertyName = targetPropertyName;
            }

            public string SourceName { get; set; }

            public string TargetPropertyName { get; set; }
        }
    }
}