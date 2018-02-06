using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Providers;
using MediaBrowser.Plugins.AniMetadata.Configuration;
using static LanguageExt.Prelude;

namespace MediaBrowser.Plugins.AniMetadata.Process.Providers
{
    internal class SeriesProvider
    {
        private readonly ILogger _log;
        private readonly IMediaItemProcessor _mediaItemProcessor;
        private readonly IPluginConfiguration _pluginConfiguration;

        public SeriesProvider(ILogManager logManager, IMediaItemProcessor mediaItemProcessor,
            IPluginConfiguration pluginConfiguration)
        {
            _mediaItemProcessor = mediaItemProcessor;
            _pluginConfiguration = pluginConfiguration;
            _log = logManager.GetLogger(nameof(SeriesProvider));
        }

        public int Order => -1;

        public string Name => "AniMetadata";

        private MetadataResult<Series> EmptyMetadataResult => new MetadataResult<Series>
        {
            Item = new Series(),
            HasMetadata = false
        };

        public Task<IEnumerable<RemoteSearchResult>> GetSearchResults(SeriesInfo searchInfo,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<MetadataResult<Series>> GetMetadata(SeriesInfo info, CancellationToken cancellationToken)
        {
            var metadataResult = Try(() =>
                {
                    if (_pluginConfiguration.ExcludedSeriesNames.Contains(info.Name,
                        StringComparer.InvariantCultureIgnoreCase))
                    {
                        _log.Info($"Skipping series '{info.Name}' as it is excluded");

                        return EmptyMetadataResult.AsTask();
                    }

                    var result = _mediaItemProcessor.GetResultAsync(info, MediaItemTypes.Series);

                    return result.Map(either =>
                        either.Match(r =>
                            {
                                _log.Info($"Found data for series '{info.Name}': '{r.EmbyMetadataResult.Item.Name}'");

                                info.IndexNumber = null;
                                info.ParentIndexNumber = null;
                                info.Name = "";
                                info.ProviderIds = new Dictionary<string, string>();

                                return r.EmbyMetadataResult;
                            },
                            failure =>
                            {
                                _log.Error($"Failed to get data for series '{info.Name}': {failure.Reason}");

                                return EmptyMetadataResult;
                            })
                    );
                })
                .IfFail(e =>
                {
                    _log.ErrorException($"Failed to get data for series '{info.Name}'", e);

                    return EmptyMetadataResult.AsTask();
                });

            return metadataResult;
        }

        public Task<HttpResponseInfo> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }
}