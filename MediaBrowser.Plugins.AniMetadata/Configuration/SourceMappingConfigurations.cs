using System.Collections;
using System.Collections.Generic;
using Emby.AniDbMetaStructure.AniDb;
using Emby.AniDbMetaStructure.AniList;
using Emby.AniDbMetaStructure.TvDb;

namespace Emby.AniDbMetaStructure.Configuration
{
    internal class SourceMappingConfigurations : IEnumerable<ISourceMappingConfiguration>
    {
        private readonly IEnumerable<ISourceMappingConfiguration> sourceMappingConfigurations;

        public SourceMappingConfigurations(AniDbSourceMappingConfiguration aniDbSourceMappingConfiguration,
            TvDbSourceMappingConfiguration tvDbSourceMappingConfiguration,
            AniListSourceMappingConfiguration aniListSourceMappingConfiguration)
        {
            this.sourceMappingConfigurations =
                new ISourceMappingConfiguration[]
                {
                    aniDbSourceMappingConfiguration,
                    tvDbSourceMappingConfiguration,
                    aniListSourceMappingConfiguration
                };
        }

        public IEnumerator<ISourceMappingConfiguration> GetEnumerator()
        {
            return this.sourceMappingConfigurations.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this.sourceMappingConfigurations).GetEnumerator();
        }
    }
}