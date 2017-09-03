using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Providers;
using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.AniDb.Series.Data;

namespace MediaBrowser.Plugins.AniMetadata.Providers.AniDb
{
    public class AniDbSeriesProvider : IRemoteMetadataProvider<Series, SeriesInfo>, IHasOrder
    {
        private readonly IAniDbClient _aniDbClient;
        private readonly IEmbyMetadataFactory _embyMetadataFactory;
        private readonly ILogger _log;

        public AniDbSeriesProvider(ILogManager logManager, IAniDbClient aniDbClient,
            IEmbyMetadataFactory embyMetadataFactory)
        {
            _aniDbClient = aniDbClient;
            _embyMetadataFactory = embyMetadataFactory;
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
            AniDbSeriesData aniDbSeriesData = null;

            var seriesResult = await _aniDbClient.FindSeriesAsync(info.Name);

            seriesResult.Match(
                s => { aniDbSeriesData = s; },
                () => _log.Info($"Failed to find an AniDb match for '{info.Name}'"));

            if (aniDbSeriesData == null)
            {
                return _embyMetadataFactory.NullSeriesResult;
            }

            var metadataResult =
                _embyMetadataFactory.CreateSeriesMetadataResult(aniDbSeriesData, info.MetadataLanguage);

            var mapper = await _aniDbClient.GetMapperAsync();

            mapper.Match(m =>
                {
                    var mappedSeriesIds = m.GetMappedSeriesIds(aniDbSeriesData.Id);

                    mappedSeriesIds.Match(
                        map =>
                        {
                            map.TvDbSeriesId.Iter(id =>
                            {
                                metadataResult.Item.SetProviderId(MetadataProviders.Tvdb, id.ToString());
                                _log.Debug($"Found TvDb Id: {id}");
                            });
                            map.ImdbSeriesId.Iter(id =>
                            {
                                metadataResult.Item.SetProviderId(MetadataProviders.Imdb, id.ToString());
                                _log.Debug($"Found Imdb Id: {id}");
                            });
                            map.TmDbSeriesId.Iter(id =>
                            {
                                metadataResult.Item.SetProviderId(MetadataProviders.Tmdb, id.ToString());
                                _log.Debug($"Found TmDb Id: {id}");
                            });
                        },
                        () => _log.Info($"Failed to find an Id mapping for AniDb Id {aniDbSeriesData.Id}"));
                },
                () => _log.Info("Failed to get mappings"));

            return metadataResult;
        }

        public string Name => ProviderNames.AniDb;

        public Task<HttpResponseInfo> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }
}