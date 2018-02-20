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
using static LanguageExt.Prelude;

namespace MediaBrowser.Plugins.AniMetadata.Process.Providers
{
    internal class EpisodeProvider
    {
        private readonly ILogger _log;
        private readonly IMediaItemProcessor _mediaItemProcessor;

        public EpisodeProvider(ILogManager logManager, IMediaItemProcessor mediaItemProcessor)
        {
            _mediaItemProcessor = mediaItemProcessor;
            _log = logManager.GetLogger(nameof(EpisodeProvider));
        }

        public int Order => -1;

        public string Name => "AniMetadata";

        private MetadataResult<Episode> EmptyMetadataResult => new MetadataResult<Episode>
        {
            Item = new Episode(),
            HasMetadata = false
        };

        public Task<IEnumerable<RemoteSearchResult>> GetSearchResults(EpisodeInfo searchInfo,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<MetadataResult<Episode>> GetMetadata(EpisodeInfo info, CancellationToken cancellationToken)
        {
            var metadataResult = Try(() =>
                {
                    var result = _mediaItemProcessor.GetResultAsync(info, MediaItemTypes.Episode, GetParentIds(info));

                    return result.Map(either =>
                        either.Match(r =>
                            {
                                _log.Info($"Found data for episode '{info.Name}': '{r.EmbyMetadataResult.Item.Name}'");

                                info.IndexNumber = null;
                                info.ParentIndexNumber = null;
                                info.Name = "";
                                info.ProviderIds = new Dictionary<string, string>();

                                return r.EmbyMetadataResult;
                            },
                            failure =>
                            {
                                _log.Error($"Failed to get data for episode '{info.Name}': {failure.Reason}");

                                return EmptyMetadataResult;
                            })
                    );
                })
                .IfFail(e =>
                {
                    _log.ErrorException($"Failed to get data for episode '{info.Name}'", e);

                    return EmptyMetadataResult.AsTask();
                });

            return metadataResult;
        }

        public Task<HttpResponseInfo> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        private IEnumerable<EmbyItemId> GetParentIds(EpisodeInfo info)
        {
            return info.SeriesProviderIds.Where(kv => int.TryParse(kv.Value, out _))
                .Select(kv => new EmbyItemId(MediaItemTypes.Series, kv.Key, int.Parse(kv.Value)));
        }
    }
}