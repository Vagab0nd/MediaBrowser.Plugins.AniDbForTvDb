using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using MediaBrowser.Plugins.AniMetadata.Providers.AniDb;

namespace MediaBrowser.Plugins.AniMetadata.EntryPoints
{
    public class PersonImageProvider : IRemoteImageProvider
    {
        private readonly AniDbPersonImageProvider personImageProvider;

        public PersonImageProvider(IApplicationHost applicationHost)
        {
            this.personImageProvider =
                DependencyConfiguration.Resolve<AniDbPersonImageProvider>(applicationHost);
        }

        public bool Supports(BaseItem item)
        {
            return this.personImageProvider.Supports(item);
        }

        public string Name => this.personImageProvider.Name;

        public IEnumerable<ImageType> GetSupportedImages(BaseItem item)
        {
            return this.personImageProvider.GetSupportedImages(item);
        }

        public Task<IEnumerable<RemoteImageInfo>> GetImages(BaseItem item, CancellationToken cancellationToken)
        {
            return this.personImageProvider.GetImages(item, cancellationToken);
        }

        public Task<HttpResponseInfo> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            return this.personImageProvider.GetImageResponse(url, cancellationToken);
        }

        public Task<IEnumerable<RemoteImageInfo>> GetImages(BaseItem item, LibraryOptions libraryOptions, CancellationToken cancellationToken)
        {
            return this.GetImages(item, cancellationToken);
        }
    }
}