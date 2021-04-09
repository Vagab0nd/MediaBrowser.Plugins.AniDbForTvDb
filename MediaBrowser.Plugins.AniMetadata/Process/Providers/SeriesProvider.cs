using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Emby.AniDbMetaStructure.Configuration;
using Emby.AniDbMetaStructure.Infrastructure;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Providers;
using static LanguageExt.Prelude;

namespace Emby.AniDbMetaStructure.Process.Providers
{
    internal class SeriesProvider
    {
        private readonly ILogger log;
        private readonly IMediaItemProcessor mediaItemProcessor;
        private readonly IPluginConfiguration pluginConfiguration;

        public SeriesProvider(ILogManager logManager, IMediaItemProcessor mediaItemProcessor,
            IPluginConfiguration pluginConfiguration)
        {
            this.mediaItemProcessor = mediaItemProcessor;
            this.pluginConfiguration = pluginConfiguration;
            this.log = logManager.GetLogger(nameof(SeriesProvider));
        }

        public int Order => -1;

        public string Name => "AniDbMetaStructure";

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
                    if (this.pluginConfiguration.ExcludedSeriesNames.Contains(info.Name,
                        StringComparer.InvariantCultureIgnoreCase))
                    {
                        this.log.Info($"Skipping series '{info.Name}' as it is excluded");

                        return this.EmptyMetadataResult.AsTask();
                    }

                    var result =
                        this.mediaItemProcessor.GetResultAsync(info, MediaItemTypes.Series, Enumerable.Empty<EmbyItemId>());

                    return result.Map(either =>
                        either.Match(r =>
                            {
                                this.log.Info($"Found data for series '{info.Name}': '{r.EmbyMetadataResult.Item.Name}'");

                                info.IndexNumber = null;
                                info.ParentIndexNumber = null;
                                info.Name = string.Empty;
                                info.ProviderIds = new Dictionary<string, string>().ToProviderIdDictionary();

                                return r.EmbyMetadataResult;
                            },
                            failure =>
                            {
                                this.log.Error($"Failed to get data for series '{info.Name}': {failure.Reason}");

                                return this.EmptyMetadataResult;
                            })
                    );
                })
                .IfFail(e =>
                {
                    this.log.ErrorException($"Failed to get data for series '{info.Name}'", e);

                    return this.EmptyMetadataResult.AsTask();
                });

            return metadataResult;
        }

        public Task<HttpResponseInfo> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }
}