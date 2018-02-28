using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Mapping;
using MediaBrowser.Plugins.AniMetadata.Providers.AniDb;
using MediaBrowser.Plugins.AniMetadata.TvDb;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;
using static LanguageExt.Prelude;

namespace MediaBrowser.Plugins.AniMetadata.Process.Sources
{
    internal class AniDbSource : ISource
    {
        private readonly IAniDbClient _aniDbClient;
        private readonly IAnimeMappingListFactory _animeMappingListFactory;
        private readonly IDataMapperFactory _dataMapperFactory;
        private readonly IEpisodeMatcher _episodeMatcher;
        private readonly ISources _sources;
        private readonly ITitlePreferenceConfiguration _titlePreferenceConfiguration;
        private readonly ITitleSelector _titleSelector;
        private readonly ITvDbClient _tvDbClient;

        public AniDbSource(IAniDbClient aniDbClient, ITvDbClient tvDbClient, IEpisodeMatcher episodeMatcher,
            ITitlePreferenceConfiguration titlePreferenceConfiguration, ITitleSelector titleSelector,
            ISources sources, IAnimeMappingListFactory animeMappingListFactory, IDataMapperFactory dataMapperFactory)
        {
            _aniDbClient = aniDbClient;
            _tvDbClient = tvDbClient;
            _episodeMatcher = episodeMatcher;
            _titlePreferenceConfiguration = titlePreferenceConfiguration;
            _titleSelector = titleSelector;
            _sources = sources;
            _animeMappingListFactory = animeMappingListFactory;
            _dataMapperFactory = dataMapperFactory;
        }

        public string Name => SourceNames.AniDb;

        public Task<Either<ProcessFailedResult, ISourceData>> LookupAsync(IMediaItem mediaItem)
        {
            var resultContext = new ProcessResultContext(Name, mediaItem.EmbyData.Identifier.Name, mediaItem.ItemType);

            var tvDbSourceData = mediaItem.GetDataFromSource(_sources.TvDb)
                .ToEither(resultContext.Failed("No TvDb data set on this media item"))
                .AsTask();

            switch (mediaItem.ItemType.Type)
            {
                case MediaItemTypeValue.Series:

                    return tvDbSourceData
                        .BindAsync(sd => sd.Id.ToEither(
                            resultContext.Failed(
                                "No TvDB Id found on the TvDB data associated with this media item")))
                        .BindAsync(tvDbSeriesId =>
                            _animeMappingListFactory.CreateMappingListAsync(CancellationToken.None)
                                .BindAsync(l => l.GetSeriesMappingsFromTvDb(tvDbSeriesId))
                                .ToEither(resultContext.Failed(
                                    $"No mapping found for TvDb series Id '{tvDbSeriesId}'")))
                        .BindAsync(sm => _aniDbClient.GetSeriesAsync(sm.First().Ids.AniDbSeriesId)
                            .ToEitherAsync(resultContext.Failed("Failed to load AniDb series data")))
                        .BindAsync(aniDbSeriesData =>
                            _titleSelector.SelectTitle(aniDbSeriesData.Titles,
                                    _titlePreferenceConfiguration.TitlePreference, mediaItem.EmbyData.Language)
                                .Map(t => (ISourceData)new SourceData<AniDbSeriesData>(this, aniDbSeriesData.Id,
                                    new ItemIdentifier(Option<int>.None, Option<int>.None, t.Title), aniDbSeriesData))
                                .ToEither(resultContext.Failed("Failed to find a title")));

                case MediaItemTypeValue.Season:

                    return Left<ProcessFailedResult, ISourceData>(
                            resultContext.Failed("AniDb source cannot load season data by mapping from other sources"))
                        .AsTask();

                case MediaItemTypeValue.Episode:

                    var tvDbSeriesData = mediaItem.EmbyData.GetParentId(MediaItemTypes.Series, _sources.TvDb)
                        .ToEitherAsync(
                            resultContext.Failed("No TvDb Id found on parent series"))
                        .BindAsync(tvDbSeriesId => _tvDbClient.GetSeriesAsync(tvDbSeriesId)
                            .ToEitherAsync(
                                resultContext.Failed($"Failed to load parent series with TvDb Id '{tvDbSeriesId}'")));

                    var tvDbEpisodeData = tvDbSourceData.MapAsync(sd => sd.GetData<TvDbEpisodeData>())
                        .BindAsync(sd =>
                            sd.ToEither(resultContext.Failed("No TvDb episode data associated with this media item")));

                    var aniDbSeriesId = mediaItem.EmbyData.GetParentId(MediaItemTypes.Series, _sources.AniDb);

                    var aniDbEpisodeData = tvDbSeriesData.BindAsync(seriesData => tvDbEpisodeData.BindAsync(
                        episodeData =>
                            _dataMapperFactory.GetDataMapperAsync()
                                .ToEither(resultContext.Failed("Data mapper could not be created"))
                                .BindAsync(m =>
                                    aniDbSeriesId.ToEither(resultContext.Failed("Failed to find AniDb series Id"))
                                        .BindAsync(id => m.MapEpisodeDataAsync(id, seriesData, episodeData)
                                            .Map(ed =>
                                            {
                                                return ed.Match(
                                                    aniDbOnly =>
                                                        Right<ProcessFailedResult, AniDbEpisodeData>(
                                                            aniDbOnly.EpisodeData),
                                                    combined =>
                                                        Right<ProcessFailedResult, AniDbEpisodeData>(
                                                            combined.AniDbEpisodeData),
                                                    none => Left<ProcessFailedResult, AniDbEpisodeData>(
                                                        resultContext.Failed(
                                                            "Failed to find a corresponding AniDb episode")));
                                            })))));

                    return aniDbEpisodeData.BindAsync(episodeData => _titleSelector
                        .SelectTitle(episodeData.Titles, _titlePreferenceConfiguration.TitlePreference,
                            mediaItem.EmbyData.Language)
                        .Map(t => (ISourceData)new SourceData<AniDbEpisodeData>(this,
                            episodeData.Id,
                            new ItemIdentifier(episodeData.EpisodeNumber.Number, episodeData.EpisodeNumber.SeasonNumber,
                                t.Title), episodeData))
                        .ToEither(resultContext.Failed("Failed to find a title")));


                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public Task<Either<ProcessFailedResult, ISourceData>> LookupAsync(EmbyItemData embyItemData)
        {
            var resultContext = new ProcessResultContext(Name, embyItemData.Identifier.Name, embyItemData.ItemType);

            switch (embyItemData.ItemType.Type)
            {
                case MediaItemTypeValue.Series:

                    return _aniDbClient.FindSeriesAsync(embyItemData.Identifier.Name)
                        .ToEitherAsync(resultContext.Failed("Failed to find series in AniDb"))
                        .BindAsync(s =>
                        {
                            var title = _titleSelector.SelectTitle(s.Titles,
                                    _titlePreferenceConfiguration.TitlePreference,
                                    embyItemData.Language)
                                .ToEither(resultContext.Failed("Failed to find a title"))
                                .AsTask();

                            return title.MapAsync(t => (ISourceData)new SourceData<AniDbSeriesData>(this, s.Id,
                                new ItemIdentifier(embyItemData.Identifier.Index, Option<int>.None, t.Title), s));
                        });

                case MediaItemTypeValue.Season:

                    var seasonIdentifier = new ItemIdentifier(embyItemData.Identifier.Index.IfNone(1),
                        embyItemData.Identifier.ParentIndex, embyItemData.Identifier.Name);

                    return Right<ProcessFailedResult, ISourceData>(new IdentifierOnlySourceData(this, Option<int>.None,
                            seasonIdentifier))
                        .AsTask();

                case MediaItemTypeValue.Episode:

                    var seriesId = embyItemData.GetParentId(MediaItemTypes.Series, this);

                    return seriesId
                        .ToEitherAsync(resultContext.Failed("No AniDb Id found on parent series"))
                        .BindAsync(id =>
                        {
                            var aniDbEpisodeData = _aniDbClient.GetSeriesAsync(id)
                                .ToEitherAsync(
                                    resultContext.Failed($"Failed to load parent series with AniDb Id '{id}'"))
                                .BindAsync(series => _episodeMatcher.FindEpisode(series.Episodes,
                                        embyItemData.Identifier.ParentIndex,
                                        embyItemData.Identifier.Index, embyItemData.Identifier.Name)
                                    .ToEither(resultContext.Failed("Failed to find episode in AniDb"))
                                    .AsTask());

                            return aniDbEpisodeData.BindAsync(e =>
                            {
                                var title = _titleSelector.SelectTitle(e.Titles,
                                        _titlePreferenceConfiguration.TitlePreference,
                                        embyItemData.Language)
                                    .ToEither(resultContext.Failed("Failed to find a title"))
                                    .AsTask();

                                return title.MapAsync(t => (ISourceData)new SourceData<AniDbEpisodeData>(this, e.Id,
                                    new ItemIdentifier(e.EpisodeNumber.Number, e.EpisodeNumber.SeasonNumber, t.Title),
                                    e));
                            });
                        });

                default:
                    return Left<ProcessFailedResult, ISourceData>(resultContext.Failed("Unsupported item type"))
                        .AsTask();
            }
        }
    }
}