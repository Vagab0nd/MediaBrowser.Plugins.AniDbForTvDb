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
        private readonly AniDbPersonProvider personProvider;

        public PersonProvider(IApplicationHost applicationHost)
        {
            this.personProvider =
                DependencyConfiguration.Resolve<AniDbPersonProvider>(applicationHost);
        }

        public Task<IEnumerable<RemoteSearchResult>> GetSearchResults(PersonLookupInfo searchInfo,
            CancellationToken cancellationToken)
        {
            return this.personProvider.GetSearchResults(searchInfo, cancellationToken);
        }

        public Task<MetadataResult<Person>> GetMetadata(PersonLookupInfo info, CancellationToken cancellationToken)
        {
            return this.personProvider.GetMetadata(info, cancellationToken);
        }

        public string Name => this.personProvider.Name;

        public Task<HttpResponseInfo> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            return this.personProvider.GetImageResponse(url, cancellationToken);
        }
    }
}