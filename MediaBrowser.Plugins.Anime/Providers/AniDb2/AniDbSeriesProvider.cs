using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Providers;
using MediaBrowser.Plugins.Anime.AniDb;
using MediaBrowser.Plugins.Anime.AniDb.Data;
using MediaBrowser.Plugins.Anime.AniDb.Mapping;

namespace MediaBrowser.Plugins.Anime.Providers.AniDb2
{
    public class AniDbSeriesProvider : IRemoteMetadataProvider<Series, SeriesInfo>, IHasOrder
    {
        private readonly AniDbClient _aniDbClient;
        private readonly ILogger _log;

        public AniDbSeriesProvider(IApplicationPaths applicationPaths, IHttpClient httpClient, ILogManager logManager)
        {
            _aniDbClient = new AniDbClient(new AniDbDataCache(new AniDbFileCache(applicationPaths),
                new AniDbFileParser(), httpClient), new AnimeMappingListFactory());
            _log = logManager.GetLogger(nameof(AniDbSeriesProvider));
        }

        public int Order => -1;

        public Task<IEnumerable<RemoteSearchResult>> GetSearchResults(SeriesInfo searchInfo,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<MetadataResult<Series>> GetMetadata(SeriesInfo info, CancellationToken cancellationToken)
        {
            AniDbSeries aniDbSeries = null;

            if (!info.ProviderIds.TryGetValue(ProviderNames.AniDb, out string aniDbSeriesIdString) ||
                !int.TryParse(aniDbSeriesIdString, out int aniDbSeriesId))
            {
                var result = await _aniDbClient.FindSeriesAsync(info.Name);

                result.Match(
                    s =>
                    {
                        aniDbSeriesId = s.Id;
                        aniDbSeries = s;
                    },
                    () => _log.Warn($"Failed to find an AniDb match for '{info.Name}'"));
            }
            else
            {
                aniDbSeries = await _aniDbClient.GetSeriesAsync(aniDbSeriesId);
            }

            if (aniDbSeries == null)
            {
                return null;
            }

            var mapper = await _aniDbClient.GetMapperAsync();

            throw new NotImplementedException();
        }

        public string Name => "AniDB";

        public Task<HttpResponseInfo> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }
}