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
using MediaBrowser.Plugins.Anime.AniDb;
using MediaBrowser.Plugins.Anime.AniDb.Data;

namespace MediaBrowser.Plugins.Anime.Providers.AniDb2
{
    internal class AniDbEpisodeProvider : IRemoteMetadataProvider<Episode, EpisodeInfo>
    {
        private readonly IAniDbClient _aniDbClient;
        private readonly IEmbyMetadataFactory _embyMetadataFactory;
        private readonly ILogger _log;

        public AniDbEpisodeProvider(IAniDbClient aniDbClient, IEmbyMetadataFactory embyMetadataFactory, ILogManager logManager)
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
            _log.Debug($"Finding AniDb episode for season '{info.ParentIndexNumber}' episode '{info.IndexNumber}', '{info.Name}'");

            var anidbEpisodeId = GetId(info.ProviderIds, ProviderNames.AniDb);

            if (anidbEpisodeId.HasValue)
            {
                _log.Debug($"Found existing AniDb episode Id '{anidbEpisodeId.Value}'");
                return new MetadataResult<Episode>();
            }

            if (!info.ParentIndexNumber.HasValue)
            {
                info.ParentIndexNumber = 1;
                _log.Debug("No season specified, defaulting to season 1");
            }
            
            var aniDbSeries =
                await _aniDbClient.GetSeriesAsync(info.SeriesProviderIds.GetOrDefault(ProviderNames.AniDb));

            var resultTask = Task.FromResult(new MetadataResult<Episode>());

            aniDbSeries.Match(
                s =>
                {
                    _log.Debug($"Using AniDb series '{s?.Id}'");

                    resultTask = GetEpisodeMetadataAsync(s, info);
                },
                () => _log.Debug($"Failed to get AniDb series with Id '{info.SeriesProviderIds.GetOrDefault(ProviderNames.AniDb)}'"));

            var result = await resultTask;

            _log.Debug($"Metadata found: {result.HasMetadata}");

            return result;
        }

        public string Name => "AniDB";

        public Task<HttpResponseInfo> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        private Task<MetadataResult<Episode>> GetEpisodeMetadataAsync(AniDbSeries aniDbSeries, EpisodeInfo episodeInfo)
        {
            Task<MetadataResult<Episode>> result = null;
            var episode = GetEpisode(aniDbSeries.Episodes, episodeInfo.IndexNumber, episodeInfo.ParentIndexNumber);

            episode.Match(
                e => result = GetEpisodeMetadataAsync(aniDbSeries.Id, e, episodeInfo.MetadataLanguage),
                () => { });

            return result;
        }

        private async Task<MetadataResult<Episode>> GetEpisodeMetadataAsync(int aniDbSeriesId,
            AniDbEpisode aniDbEpisode, string metadataLanguage)
        {
            var mapper = await _aniDbClient.GetMapperAsync();

            var tvDbEpisodeNumber = mapper.GetMappedTvDbEpisodeId(aniDbSeriesId, aniDbEpisode.EpisodeNumber);

            return _embyMetadataFactory.CreateEpisodeMetadataResult(aniDbEpisode, tvDbEpisodeNumber,
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

        private IOption<AniDbEpisode> GetEpisode(IEnumerable<AniDbEpisode> episodes, int? episodeIndex,
            int? seasonIndex)
        {
            var type = seasonIndex == 0 ? EpisodeType.Special : EpisodeType.Normal;

            var episode = episodes?.FirstOrDefault(e => e.EpisodeNumber.Type == type &&
                e.EpisodeNumber.Number == episodeIndex);

            return Option.Optionify(episode);
        }
    }
}