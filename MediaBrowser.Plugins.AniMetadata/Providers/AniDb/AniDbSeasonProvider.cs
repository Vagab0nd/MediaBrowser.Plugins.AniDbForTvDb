using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Providers;
using MediaBrowser.Plugins.AniMetadata.AniDb;

namespace MediaBrowser.Plugins.AniMetadata.Providers.AniDb
{
    internal class AniDbSeasonProvider : IRemoteMetadataProvider<Season, SeasonInfo>
    {
        private readonly IAniDbClient _aniDbClient;
        private readonly ISeasonMetadataFactory _seasonMetadataFactory;

        public AniDbSeasonProvider(IAniDbClient aniDbClient, ISeasonMetadataFactory seasonMetadataFactory)
        {
            _aniDbClient = aniDbClient;
            _seasonMetadataFactory = seasonMetadataFactory;
        }

        public Task<IEnumerable<RemoteSearchResult>> GetSearchResults(SeasonInfo searchInfo,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<MetadataResult<Season>> GetMetadata(SeasonInfo info, CancellationToken cancellationToken)
        {
            var aniDbSeries =
                await _aniDbClient.GetSeriesAsync(info.SeriesProviderIds.GetOrDefault(ProviderNames.AniDb));

            var result = _seasonMetadataFactory.NullSeasonResult;

            aniDbSeries.Match(
                s =>
                {
                    result = _seasonMetadataFactory.CreateMetadata(s, info.IndexNumber.GetValueOrDefault(1),
                        info.MetadataLanguage);
                },
                () => { });

            return result;
        }

        public string Name => ProviderNames.AniDb;

        public Task<HttpResponseInfo> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}