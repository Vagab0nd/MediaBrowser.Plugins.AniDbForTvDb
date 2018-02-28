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
using MediaBrowser.Plugins.AniMetadata.Mapping;

namespace MediaBrowser.Plugins.AniMetadata.Providers.AniDb
{
    internal class AniDbEpisodeProvider
    {
        private readonly IAniDbClient _aniDbClient;
        private readonly IEpisodeMatcher _episodeMatcher;
        private readonly IDataMapperFactory _dataMapperFactory;
        private readonly IEpisodeMetadataFactory _episodeMetadataFactory;
        private readonly ILogger _log;

        public AniDbEpisodeProvider(IAniDbClient aniDbClient, IEpisodeMetadataFactory episodeMetadataFactory,
            ILogManager logManager, IEpisodeMatcher episodeMatcher, IDataMapperFactory dataMapperFactory)
        {
            _aniDbClient = aniDbClient;
            _episodeMetadataFactory = episodeMetadataFactory;
            _episodeMatcher = episodeMatcher;
            _dataMapperFactory = dataMapperFactory;
            _log = logManager.GetLogger(nameof(AniDbEpisodeProvider));
        }

        public Task<IEnumerable<RemoteSearchResult>> GetSearchResults(EpisodeInfo searchInfo,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<MetadataResult<Episode>> GetMetadata(EpisodeInfo info, CancellationToken cancellationToken)
        {
            _log.Info(
                $"Finding data for episode, season '{info.ParentIndexNumber}' episode '{info.IndexNumber}', '{info.Name}'");

            var aniDbSeries =
                await _aniDbClient.GetSeriesAsync(info.SeriesProviderIds.GetOrDefault(ProviderNames.AniDb));

            var result = await aniDbSeries.MatchAsync(
                s => GetNewEpisodeMetadataAsync(aniDbSeries, info),
                () =>
                {
                    _log.Debug(
                        $"Failed to get AniDb series with Id '{info.SeriesProviderIds.GetOrDefault(ProviderNames.AniDb)}'");
                    return _episodeMetadataFactory.NullResult;
                });

            if (result.HasMetadata)
            {
                info.IndexNumber = null;
                info.ParentIndexNumber = null;
                info.Name = "";
                info.ProviderIds = new Dictionary<string, string>();

                _log.Info(
                    $"Found episode data: {{Absolute episode index = {result.Item?.AbsoluteEpisodeNumber}, Season index = {result.Item?.ParentIndexNumber}, Episode index = {result.Item?.IndexNumber}}}");
            }
            else
            {
                _log.Info("Found no matching episode data");
            }

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
            var resultTask = aniDbSeries.MatchAsync(
                s =>
                {
                    _log.Debug($"Using AniDb series '{s?.Id}'");

                    return GetEpisodeMetadataAsync(s, info);
                },
                () => _episodeMetadataFactory.NullResult);

            return resultTask;
        }

        private Task<MetadataResult<Episode>> GetEpisodeMetadataAsync(AniDbSeriesData aniDbSeriesData,
            EpisodeInfo episodeInfo)
        {
            var episode = _episodeMatcher.FindEpisode(aniDbSeriesData.Episodes,
                episodeInfo.ParentIndexNumber.ToOption(), episodeInfo.IndexNumber.ToOption(), episodeInfo.Name);

            var result = episode.MatchAsync(
                e => GetEpisodeMetadataAsync(aniDbSeriesData, e, episodeInfo.MetadataLanguage),
                () =>
                {
                    _log.Debug("No episode metadata found");
                    return _episodeMetadataFactory.NullResult;
                });

            return result;
        }

        private Task<MetadataResult<Episode>> GetEpisodeMetadataAsync(AniDbSeriesData aniDbSeriesData,
            AniDbEpisodeData aniDbEpisodeData, string metadataLanguage)
        {
            return _dataMapperFactory.GetDataMapperAsync()
                .Match(mapper =>
                        mapper.MapEpisodeDataAsync(aniDbSeriesData, aniDbEpisodeData)
                            .Map(d => _episodeMetadataFactory.CreateMetadata(d, metadataLanguage)),
                    () =>
                    {
                        _log.Debug("No mapper set up");
                        return _episodeMetadataFactory.NullResult;
                    });
        }
    }
}