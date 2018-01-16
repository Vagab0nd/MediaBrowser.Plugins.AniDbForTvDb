using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Providers;
using MediaBrowser.Plugins.AniMetadata.Providers.AniDb;

namespace MediaBrowser.Plugins.AniMetadata.EntryPoints
{
    public class PersonProvider : IRemoteMetadataProvider<Person, PersonLookupInfo>
    {
        private readonly AniDbPersonProvider _personProvider;

        public PersonProvider(IApplicationHost applicationHost)
        {
            _personProvider =
                DependencyConfiguration.Resolve<AniDbPersonProvider>(applicationHost);
        }

        public Task<IEnumerable<RemoteSearchResult>> GetSearchResults(PersonLookupInfo searchInfo,
            CancellationToken cancellationToken)
        {
            return _personProvider.GetSearchResults(searchInfo, cancellationToken);
        }

        public Task<MetadataResult<Person>> GetMetadata(PersonLookupInfo info, CancellationToken cancellationToken)
        {
            return _personProvider.GetMetadata(info, cancellationToken);
        }

        public string Name => _personProvider.Name;

        public Task<HttpResponseInfo> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            return _personProvider.GetImageResponse(url, cancellationToken);
        }
    }
}