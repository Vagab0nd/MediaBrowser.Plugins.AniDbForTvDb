using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using MediaBrowser.Plugins.AniMetadata.Providers.AniDb;

namespace MediaBrowser.Plugins.AniMetadata.EntryPoints
{
    public class PersonImageProvider : IRemoteImageProvider
    {
        private readonly AniDbPersonImageProvider _personImageProvider;

        public PersonImageProvider(IApplicationHost applicationHost)
        {
            _personImageProvider =
                DependencyConfiguration.Resolve<AniDbPersonImageProvider>(applicationHost);
        }

        public bool Supports(BaseItem item)
        {
            return _personImageProvider.Supports(item);
        }

        public string Name => _personImageProvider.Name;

        public IEnumerable<ImageType> GetSupportedImages(BaseItem item)
        {
            return _personImageProvider.GetSupportedImages(item);
        }

        public Task<IEnumerable<RemoteImageInfo>> GetImages(BaseItem item, CancellationToken cancellationToken)
        {
            return _personImageProvider.GetImages(item, cancellationToken);
        }

        public Task<HttpResponseInfo> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            return _personImageProvider.GetImageResponse(url, cancellationToken);
        }
    }
}