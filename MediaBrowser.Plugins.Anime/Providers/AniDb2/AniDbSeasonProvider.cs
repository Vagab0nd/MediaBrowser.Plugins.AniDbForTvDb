using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Providers;
using MediaBrowser.Plugins.Anime.AniDb;
using MediaBrowser.Plugins.Anime.AniDb.Data;

namespace MediaBrowser.Plugins.Anime.Providers.AniDb2
{
    internal class AniDbSeasonProvider : IRemoteMetadataProvider<Season, SeasonInfo>
    {
        private readonly IAniDbClient _aniDbClient;
        private readonly IEmbyMetadataFactory _embyMetadataFactory;

        public AniDbSeasonProvider(IAniDbClient aniDbClient, IEmbyMetadataFactory embyMetadataFactory)
        {
            _aniDbClient = aniDbClient;
            _embyMetadataFactory = embyMetadataFactory;
        }

        public Task<IEnumerable<RemoteSearchResult>> GetSearchResults(SeasonInfo searchInfo,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<MetadataResult<Season>> GetMetadata(SeasonInfo info, CancellationToken cancellationToken)
        {
            var aniDbSeries = await GetParentSeriesAsync(info.SeriesProviderIds.GetOrDefault(ProviderNames.AniDb));

            if (aniDbSeries == null)
            {
                return null;
            }

            var result = _embyMetadataFactory.CreateSeasonMetadataResult(aniDbSeries,
                info.IndexNumber.GetValueOrDefault(1), info.MetadataLanguage);

            return result;
        }

        public string Name => "AniDB";

        public Task<HttpResponseInfo> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private Task<AniDbSeries> GetParentSeriesAsync(string aniDbSeriesIdString)
        {
            if (!int.TryParse(aniDbSeriesIdString, out int aniDbSeriesId))
            {
                return null;
            }

            return _aniDbClient.GetSeriesAsync(aniDbSeriesId);
        }
    }
}