using System;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Mapping;
using MediaBrowser.Plugins.AniMetadata.TvDb;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;

namespace MediaBrowser.Plugins.AniMetadata.Process.Sources
{
    internal class TvDbSource : ISource
    {
        private readonly IAniDbClient _aniDbClient;
        private readonly IAnimeMappingListFactory _animeMappingListFactory;
        private readonly IDataMapper _dataMapper;
        private readonly ISources _sources;
        private readonly ITvDbClient _tvDbClient;

        public TvDbSource(ITvDbClient tvDbClient, IAnimeMappingListFactory animeMappingListFactory, ISources sources,
            IDataMapper dataMapper, IAniDbClient aniDbClient)
        {
            _tvDbClient = tvDbClient;
            _animeMappingListFactory = animeMappingListFactory;
            _sources = sources;
            _dataMapper = dataMapper;
            _aniDbClient = aniDbClient;
        }

        public string Name => "TvDb";

        public Task<Either<ProcessFailedResult, ISourceData>> LookupAsync(IMediaItem mediaItem)
        {
            var resultContext = new ProcessResultContext(Name, mediaItem.EmbyData.Identifier.Name, mediaItem.ItemType);
            var aniDbSource = _sources.GetSource<AniDbSource>()
                .ToEither(resultContext.Failed("AniDb source is not configured"));
            var aniDbSourceData = aniDbSource
                .BindAsync(s => mediaItem.GetDataFromSource(s)
                    .ToEither(resultContext.Failed("No AniDb data set on this media item"))
                    .AsTask());

            switch (mediaItem.ItemType)
            {
                case ItemType.Series:

                    return aniDbSourceData
                        .BindAsync(sd => sd.Id.ToEither(
                            resultContext.Failed(
                                "No AniDb Id found on the AniDb data associated with this media item")))
                        .BindAsync(aniDbSeriesId =>
                            _animeMappingListFactory.CreateMappingListAsync(CancellationToken.None)
                                .BindAsync(l => l.GetSeriesMappingFromAniDb(aniDbSeriesId))
                                .ToEither(resultContext.Failed(
                                    $"No mapping found for AniDb series Id '{aniDbSeriesId}'")))
                        .BindAsync(sm => sm.Ids.TvDbSeriesId.ToEither(
                            resultContext.Failed("No TvDb Id found on matching mapping")))
                        .BindAsync(tvDbSeriesId => _tvDbClient.GetSeriesAsync(tvDbSeriesId)
                            .ToEitherAsync(resultContext.Failed("Failed to load TvDb series data")))
                        .MapAsync(tvDbSeriesData => (ISourceData)new SourceData<TvDbSeriesData>(this, tvDbSeriesData.Id,
                            new ItemIdentifier(Option<int>.None, Option<int>.None, tvDbSeriesData.SeriesName),
                            tvDbSeriesData));

                case ItemType.Season:

                    return Prelude.Left<ProcessFailedResult, ISourceData>(
                            resultContext.Failed("TvDb source cannot load season data by mapping from other sources"))
                        .AsTask();

                case ItemType.Episode:

                    var aniDbSeriesData = aniDbSource.BindAsync(s => mediaItem.EmbyData.GetParentId(ItemType.Series, s)
                            .ToEitherAsync(
                                resultContext.Failed("No AniDb Id found on parent series")))
                        .BindAsync(aniDbSeriesId => _aniDbClient.GetSeriesAsync(aniDbSeriesId)
                            .ToEitherAsync(
                                resultContext.Failed($"Failed to load parent series with AniDb Id '{aniDbSeriesId}'")));

                    var aniDbEpisodeData = aniDbSourceData.MapAsync(sd => sd.GetData<SourceData<AniDbEpisodeData>>())
                        .BindAsync(sd => sd.ToEither(resultContext.Failed("No AniDb episode data associated with this media item")));

                    var tvDbEpisodeData = aniDbSeriesData.BindAsync(seriesData => aniDbEpisodeData.BindAsync(
                        episodeData =>
                            _dataMapper.MapEpisodeDataAsync(seriesData, episodeData.Data)
                                .Bind(ed => ed.Match(
                                        aniDbOnly =>
                                            Prelude.Left<ProcessFailedResult, TvDbEpisodeData>(
                                                resultContext.Failed("Failed to find a corresponding TvDb episode")),
                                        combined =>
                                            Prelude.Right<ProcessFailedResult, TvDbEpisodeData>(combined.TvDbEpisodeData),
                                        none => Prelude.Left<ProcessFailedResult, TvDbEpisodeData>(
                                            resultContext.Failed("Failed to find a corresponding TvDb episode"))
                                    )
                                    .AsTask())));

                    return tvDbEpisodeData.MapAsync(episodeData => (ISourceData)new SourceData<TvDbEpisodeData>(this,
                        episodeData.Id,
                        new ItemIdentifier(episodeData.AiredEpisodeNumber, episodeData.AiredSeason,
                            episodeData.EpisodeName), episodeData));


                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public Task<Either<ProcessFailedResult, ISourceData>> LookupAsync(EmbyItemData embyItemData)
        {
            throw new NotImplementedException();
        }
    }
}