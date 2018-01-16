using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Providers;
using MediaBrowser.Plugins.AniMetadata.Providers.AniDb;

namespace MediaBrowser.Plugins.AniMetadata.EntryPoints
{
    public class EpisodeProvider : IRemoteMetadataProvider<Episode, EpisodeInfo>
    {
        private readonly AniDbEpisodeProvider _episodeProvider;

        public EpisodeProvider(IApplicationHost applicationHost)
        {
            _episodeProvider = DependencyConfiguration.Resolve<AniDbEpisodeProvider>(applicationHost);
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