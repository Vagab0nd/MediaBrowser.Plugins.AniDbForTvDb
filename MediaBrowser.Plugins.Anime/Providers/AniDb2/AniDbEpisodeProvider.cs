using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Functional.Maybe;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Providers;
using MediaBrowser.Plugins.Anime.AniDb;
using MediaBrowser.Plugins.Anime.AniDb.Series;
using MediaBrowser.Plugins.Anime.AniDb.Series.Data;

namespace MediaBrowser.Plugins.Anime.Providers.AniDb2
{
    internal class AniDbEpisodeProvider : IRemoteMetadataProvider<Episode, EpisodeInfo>
    {
        private readonly IAniDbClient _aniDbClient;
        private readonly IEmbyMetadataFactory _embyMetadataFactory;
        private readonly ILogger _log;

        public AniDbEpisodeProvider(IAniDbClient aniDbClient, IEmbyMetadataFactory embyMetadataFactory,
            ILogManager logManager)
        {
            _aniDbClient = aniDbClient;
            _embyMetadataFactory = embyMetadataFactory;
            _log = logManager.GetLogger(nameof(AniDbEpisodeProvider));
        }

        public Task<IEnumerable<RemoteSearchResult>> GetSearchResults(EpisodeInfo searchInfo,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<MetadataResult<Episode>> GetMetadata(EpisodeInfo info, CancellationToken cancellationToken)
        {
            _log.Debug(
                $"Finding AniDb episode for season '{info.ParentIndexNumber}' episode '{info.IndexNumber}', '{info.Name}'");

            if (!info.ParentIndexNumber.HasValue)
            {
                info.ParentIndexNumber = 1;
                _log.Debug("No season specified, defaulting to season 1");
            }

            var aniDbSeries =
                await _aniDbClient.GetSeriesAsync(info.SeriesProviderIds.GetOrDefault(ProviderNames.AniDb));

            var resultTask = Task.FromResult(_embyMetadataFactory.NullEpisodeResult);

            aniDbSeries.Match(
                s => resultTask = GetNewEpisodeMetadataAsync(aniDbSeries, info),
                () => _log.Debug(
                    $"Failed to get AniDb series with Id '{info.SeriesProviderIds.GetOrDefault(ProviderNames.AniDb)}'"));

            var result = await resultTask;

            _log.Debug($"Metadata found: {result.HasMetadata}");

            return result;
        }

        public string Name => "AniDB";

        public Task<HttpResponseInfo> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        private Task<MetadataResult<Episode>> GetNewEpisodeMetadataAsync(Maybe<AniDbSeriesData> aniDbSeries,
            EpisodeInfo info)
        {
            var resultTask = Task.FromResult(_embyMetadataFactory.NullEpisodeResult);

            aniDbSeries.Match(
                s =>
                {
                    _log.Debug($"Using AniDb series '{s?.Id}'");

                    resultTask = GetEpisodeMetadataAsync(s, info);
                },
                () => { });

            return resultTask;
        }

        private Task<MetadataResult<Episode>> GetExistingEpisodeMetadataAsync(AniDbSeriesData aniDbSeriesData,
            int aniDbEpisodeId, string metadataLanguage)
        {
            var episode = aniDbSeriesData.Episodes.Single(e => e.Id == aniDbEpisodeId);

            return GetEpisodeMetadataAsync(aniDbSeriesData.Id, episode, metadataLanguage);
        }

        private Task<MetadataResult<Episode>> GetEpisodeMetadataAsync(AniDbSeriesData aniDbSeriesData, EpisodeInfo episodeInfo)
        {
            var result = Task.FromResult(_embyMetadataFactory.NullEpisodeResult);
            var episode = GetEpisode(aniDbSeriesData.Episodes, episodeInfo.IndexNumber, episodeInfo.ParentIndexNumber);

            episode.Match(
                e => result = GetEpisodeMetadataAsync(aniDbSeriesData.Id, e, episodeInfo.MetadataLanguage),
                () => _log.Debug("No episode metadata found"));

            return result;
        }

        private async Task<MetadataResult<Episode>> GetEpisodeMetadataAsync(int aniDbSeriesId,
            EpisodeData episodeData, string metadataLanguage)
        {
            var mapper = await _aniDbClient.GetMapperAsync();

            var tvDbEpisodeNumber = mapper.GetMappedTvDbEpisodeId(aniDbSeriesId, episodeData.EpisodeNumber);

            return _embyMetadataFactory.CreateEpisodeMetadataResult(episodeData, tvDbEpisodeNumber,
                metadataLanguage);
        }

        private int? GetId(IDictionary<string, string> providerIds, string providerName)
        {
            var idString = providerIds?.GetOrDefault(providerName) ?? "";

            if (!int.TryParse(idString, out int id))
            {
                return null;
            }

            return id;
        }

        private Maybe<EpisodeData> GetEpisode(IEnumerable<EpisodeData> episodes, int? episodeIndex,
            int? seasonIndex)
        {
            var type = seasonIndex == 0 ? EpisodeType.Special : EpisodeType.Normal;

            var episode = episodes?.FirstOrDefault(e => e.EpisodeNumber.Type == type &&
                e.EpisodeNumber.Number == episodeIndex);

            return episode.ToMaybe();
        }
    }
}