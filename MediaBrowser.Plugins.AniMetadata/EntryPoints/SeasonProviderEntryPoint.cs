using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Providers;
using MediaBrowser.Plugins.AniMetadata.Process.Providers;

namespace MediaBrowser.Plugins.AniMetadata.EntryPoints
{
    public class SeasonProviderEntryPoint : IRemoteMetadataProvider<Season, SeasonInfo>
    {
        private readonly SeasonProvider seasonProvider;

        public SeasonProviderEntryPoint(IApplicationHost applicationHost)
        {
            this.seasonProvider = DependencyConfiguration.Resolve<SeasonProvider>(applicationHost);
        }

        public Task<IEnumerable<RemoteSearchResult>> GetSearchResults(SeasonInfo searchInfo,
            CancellationToken cancellationToken)
        {
            return this.seasonProvider.GetSearchResults(searchInfo, cancellationToken);
        }

        public Task<MetadataResult<Season>> GetMetadata(SeasonInfo info, CancellationToken cancellationToken)
        {
            return this.seasonProvider.GetMetadata(info, cancellationToken);
        }

        public string Name => this.seasonProvider.Name;

        public Task<HttpResponseInfo> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            return this.seasonProvider.GetImageResponse(url, cancellationToken);
        }
    }
}