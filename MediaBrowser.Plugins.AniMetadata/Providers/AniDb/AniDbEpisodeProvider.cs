using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Providers;
using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;

namespace MediaBrowser.Plugins.AniMetadata.Providers.AniDb
{
    internal class AniDbEpisodeProvider : IRemoteMetadataProvider<Episode, EpisodeInfo>
    {
        private readonly IAniDbClient _aniDbClient;
        private readonly IEmbyMetadataFactory _embyMetadataFactory;
        private readonly IEpisodeMatcher _episodeMatcher;
        private readonly ILogger _log;

        public AniDbEpisodeProvider(IAniDbClient aniDbClient, IEmbyMetadataFactory embyMetadataFactory,
            ILogManager logManager, IEpisodeMatcher episodeMatcher)
        {
            _aniDbClient = aniDbClient;
            _embyMetadataFactory = embyMetadataFactory;
            _episodeMatcher = episodeMatcher;
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

            var aniDbSeries =
                await _aniDbClient.GetSeriesAsync(info.SeriesProviderIds.GetOrDefault(ProviderNames.AniDb));

            var resultTask = Task.FromResult(_embyMetadataFactory.NullEpisodeResult);

            aniDbSeries.Match(
                s => resultTask = GetNewEpisodeMetadataAsync(aniDbSeries, info),
                () => _log.Debug(
                    $"Failed to get AniDb series with Id '{info.SeriesProviderIds.GetOrDefault(ProviderNames.AniDb)}'"));

            var result = await resultTask;

            info.IndexNumber = result.Item?.IndexNumber;
            info.ParentIndexNumber = result.Item?.ParentIndexNumber;
            info.Name = result.Item?.Name;
            info.ProviderIds = result.Item?.ProviderIds;

            _log.Debug(
                $"Returning metadata: {{Absolute episode index = {result.Item?.AbsoluteEpisodeNumber}, Season index = {result.Item?.ParentIndexNumber}, Episode index = {result.Item?.IndexNumber}}}");

            return result;
        }

        public string Name => ProviderNames.AniDb;

        public Task<HttpResponseInfo> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        private Task<MetadataResult<Episode>> GetNewEpisodeMetadataAsync(Option<AniDbSeriesData> aniDbSeries,
            EpisodeInfo info)
        {
            var resultTask = aniDbSeries.Match(
                s =>
                {
                    _log.Debug($"Using AniDb series '{s?.Id}'");

                    return GetEpisodeMetadataAsync(s, info);
                },
                () => Task.FromResult(_embyMetadataFactory.NullEpisodeResult));

            return resultTask;
        }

        private Task<MetadataResult<Episode>> GetEpisodeMetadataAsync(AniDbSeriesData aniDbSeriesData,
            EpisodeInfo episodeInfo)
        {
            var result = Task.FromResult(_embyMetadataFactory.NullEpisodeResult);
            var episode = _episodeMatcher.FindEpisode(aniDbSeriesData.Episodes, episodeInfo.ParentIndexNumber.ToOption(),
                episodeInfo.IndexNumber.ToOption(), episodeInfo.Name);

            episode.Match(
                e => result = GetEpisodeMetadataAsync(aniDbSeriesData.Id, e, episodeInfo.MetadataLanguage),
                () => _log.Debug("No episode metadata found"));

            return result;
        }

        private async Task<MetadataResult<Episode>> GetEpisodeMetadataAsync(int aniDbSeriesId,
            EpisodeData episodeData, string metadataLanguage)
        {
            var mapper = await _aniDbClient.GetMapperAsync();

            var result = await mapper.Match(async m =>
                {
                    var tvDbEpisodeNumber =
                        await m.GetMappedTvDbEpisodeIdAsync(aniDbSeriesId, episodeData.EpisodeNumber);

                    return _embyMetadataFactory.CreateEpisodeMetadataResult(episodeData, tvDbEpisodeNumber,
                        metadataLanguage);
                },
                () => Task.FromResult(_embyMetadataFactory.NullEpisodeResult));

            return result;
        }
    }
}