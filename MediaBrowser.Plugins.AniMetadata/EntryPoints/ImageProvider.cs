using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Providers;
using MediaBrowser.Plugins.AniMetadata.Providers.AniDb;

namespace MediaBrowser.Plugins.AniMetadata.EntryPoints
{
    public class ImageProvider : IRemoteImageProvider
    {
        private readonly AniDbImageProvider imageProvider;

        public ImageProvider(IApplicationHost applicationHost, ILogManager logManager)
        {
            var logger = logManager.GetLogger(nameof(ImageProvider));

            logger.Info("Resolving...");

            this.imageProvider = DependencyConfiguration.Resolve<AniDbImageProvider>(applicationHost);
        }

        public bool Supports(BaseItem item)
        {
            return this.imageProvider.Supports(item);
        }

        public string Name => this.imageProvider.Name;

        public IEnumerable<ImageType> GetSupportedImages(BaseItem item)
        {
            return this.imageProvider.GetSupportedImages(item);
        }

        public Task<IEnumerable<RemoteImageInfo>> GetImages(BaseItem item, CancellationToken cancellationToken)
        {
            return this.imageProvider.GetImages(item, cancellationToken);
        }

        public Task<HttpResponseInfo> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            return this.imageProvider.GetImageResponse(url, cancellationToken);
        }

        public Task<IEnumerable<RemoteImageInfo>> GetImages(BaseItem item, LibraryOptions libraryOptions, CancellationToken cancellationToken)
        {
            return this.GetImages(item, cancellationToken);
        }
    }
}