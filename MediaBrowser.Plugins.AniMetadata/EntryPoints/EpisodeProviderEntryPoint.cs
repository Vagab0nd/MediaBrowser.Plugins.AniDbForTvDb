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
    public class EpisodeProviderEntryPoint : IRemoteMetadataProvider<Episode, EpisodeInfo>
    {
        private readonly EpisodeProvider _episodeProvider;

        public EpisodeProviderEntryPoint(IApplicationHost applicationHost)
        {
            _episodeProvider = DependencyConfiguration.Resolve<EpisodeProvider>(applicationHost);
        }

        public Task<IEnumerable<RemoteSearchResult>> GetSearchResults(EpisodeInfo searchInfo,
            CancellationToken cancellationToken)
        {
            return _episodeProvider.GetSearchResults(searchInfo, cancellationToken);
        }

        public Task<MetadataResult<Episode>> GetMetadata(EpisodeInfo info, CancellationToken cancellationToken)
        {
            return _episodeProvider.GetMetadata(info, cancellationToken);
        }

        public string Name => _episodeProvider.Name;

        public Task<HttpResponseInfo> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            return _episodeProvider.GetImageResponse(url, cancellationToken);
        }
    }
}