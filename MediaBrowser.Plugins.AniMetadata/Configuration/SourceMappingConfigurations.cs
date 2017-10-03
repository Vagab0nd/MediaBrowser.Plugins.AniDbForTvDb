using System.Collections;
using System.Collections.Generic;
using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.TvDb;

namespace MediaBrowser.Plugins.AniMetadata.Configuration
{
    internal class SourceMappingConfigurations : IEnumerable<ISourceMappingConfiguration>
    {
        private readonly IEnumerable<ISourceMappingConfiguration> _sourceMappingConfigurations;

        public SourceMappingConfigurations(AniDbSourceMappingConfiguration aniDbSourceMappingConfiguration,
            TvDbSourceMappingConfiguration tvDbSourceMappingConfiguration)
        {
            _sourceMappingConfigurations =
                new ISourceMappingConfiguration[] { aniDbSourceMappingConfiguration, tvDbSourceMappingConfiguration };
        }

        public IEnumerator<ISourceMappingConfiguration> GetEnumerator()
        {
            return _sourceMappingConfigurations.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_sourceMappingConfigurations).GetEnumerator();
        }
    }
}