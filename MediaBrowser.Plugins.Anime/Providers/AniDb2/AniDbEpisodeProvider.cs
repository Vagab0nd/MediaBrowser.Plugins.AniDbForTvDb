using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Functional.Maybe;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Providers;
using MediaBrowser.Plugins.Anime.AniDb;
using MediaBrowser.Plugins.Anime.AniDb.Series.Data;

namespace MediaBrowser.Plugins.Anime.Providers.AniDb2
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
            var resultTask = aniDbSeries.SelectOrElse(
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
            var episode = _episodeMatcher.FindEpisode(aniDbSeriesData.Episodes, episodeInfo.ParentIndexNumber.ToMaybe(),
                episodeInfo.IndexNumber.ToMaybe(), episodeInfo.Name.ToMaybe());

            episode.Match(
                e => result = GetEpisodeMetadataAsync(aniDbSeriesData.Id, e, episodeInfo.MetadataLanguage),
                () => _log.Debug("No episode metadata found"));

            return result;
        }

        private async Task<MetadataResult<Episode>> GetEpisodeMetadataAsync(int aniDbSeriesId,
            EpisodeData episodeData, string metadataLanguage)
        {
            var result = _embyMetadataFactory.NullEpisodeResult;
            var mapper = await _aniDbClient.GetMapperAsync();

            mapper.Do(m =>
            {
                var tvDbEpisodeNumber = m.GetMappedTvDbEpisodeId(aniDbSeriesId, episodeData.EpisodeNumber);

                result = _embyMetadataFactory.CreateEpisodeMetadataResult(episodeData, tvDbEpisodeNumber,
                    metadataLanguage);
            });

            return result;
        }
    }
}