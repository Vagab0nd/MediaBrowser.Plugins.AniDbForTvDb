using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.AniDb.Titles;
using MediaBrowser.Plugins.AniMetadata.Mapping;
using MediaBrowser.Plugins.AniMetadata.TvDb;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;
using static LanguageExt.Prelude;

namespace MediaBrowser.Plugins.AniMetadata.Process.Sources
{
    internal class TvDbSource : ISource
    {
        private readonly IAniDbClient _aniDbClient;
        private readonly IAnimeMappingListFactory _animeMappingListFactory;
        private readonly IDataMapperFactory _dataMapperFactory;
        private readonly ISources _sources;
        private readonly ITitleNormaliser _titleNormaliser;
        private readonly ITvDbClient _tvDbClient;

        public TvDbSource(ITvDbClient tvDbClient, IAnimeMappingListFactory animeMappingListFactory, ISources sources,
            IDataMapperFactory dataMapperFactory, IAniDbClient aniDbClient, ITitleNormaliser titleNormaliser)
        {
            _tvDbClient = tvDbClient;
            _animeMappingListFactory = animeMappingListFactory;
            _sources = sources;
            _dataMapperFactory = dataMapperFactory;
            _aniDbClient = aniDbClient;
            _titleNormaliser = titleNormaliser;
        }

        public string Name => SourceNames.TvDb;

        public Task<Either<ProcessFailedResult, ISourceData>> LookupFromOtherSourcesAsync(IMediaItem mediaItem)
        {
            var resultContext = new ProcessResultContext(Name, mediaItem.EmbyData.Identifier.Name, mediaItem.ItemType);

            var aniDbSourceData = mediaItem.GetDataFromSource(_sources.AniDb)
                .ToEither(resultContext.Failed("No AniDb data set on this media item"))
                .AsTask();

            switch (mediaItem.ItemType.Type)
            {
                case MediaItemTypeValue.Series:

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

                case MediaItemTypeValue.Season:

                    return Left<ProcessFailedResult, ISourceData>(
                            resultContext.Failed("TvDb source cannot load season data by mapping from other sources"))
                        .AsTask();

                case MediaItemTypeValue.Episode:

                    var aniDbSeriesData = mediaItem.EmbyData.GetParentId(MediaItemTypes.Series, _sources.AniDb)
                        .ToEitherAsync(
                            resultContext.Failed("No AniDb Id found on parent series"))
                        .BindAsync(aniDbSeriesId => _aniDbClient.GetSeriesAsync(aniDbSeriesId)
                            .ToEitherAsync(
                                resultContext.Failed($"Failed to load parent series with AniDb Id '{aniDbSeriesId}'")));

                    var aniDbEpisodeData = aniDbSourceData.MapAsync(sd => sd.GetData<AniDbEpisodeData>())
                        .BindAsync(sd =>
                            sd.ToEither(resultContext.Failed("No AniDb episode data associated with this media item")));

                    var tvDbEpisodeData = aniDbSeriesData.BindAsync(seriesData => aniDbEpisodeData.BindAsync(
                        episodeData =>
                            _dataMapperFactory.GetDataMapperAsync()
                                .ToEither(resultContext.Failed("Data mapper could not be created"))
                                .BindAsync(m => m.MapEpisodeDataAsync(seriesData, episodeData)
                                    .Bind(ed => ed.Match(
                                            aniDbOnly =>
                                                Left<ProcessFailedResult, TvDbEpisodeData>(
                                                    resultContext.Failed(
                                                        "Failed to find a corresponding TvDb episode")),
                                            combined =>
                                                Right<ProcessFailedResult, TvDbEpisodeData>(
                                                    combined.TvDbEpisodeData),
                                            none => Left<ProcessFailedResult, TvDbEpisodeData>(
                                                resultContext.Failed("Failed to find a corresponding TvDb episode"))
                                        )
                                        .AsTask()))));

                    return tvDbEpisodeData.MapAsync(episodeData => (ISourceData)new SourceData<TvDbEpisodeData>(this,
                        episodeData.Id,
                        new ItemIdentifier(episodeData.AiredEpisodeNumber, episodeData.AiredSeason,
                            episodeData.EpisodeName), episodeData));


                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public Task<Either<ProcessFailedResult, ISourceData>> LookupFromEmbyData(EmbyItemData embyItemData)
        {
            var resultContext = new ProcessResultContext(Name, embyItemData.Identifier.Name, embyItemData.ItemType);

            switch (embyItemData.ItemType.Type)
            {
                case MediaItemTypeValue.Series:

                    return _tvDbClient.FindSeriesAsync(embyItemData.Identifier.Name)
                        .ToEitherAsync(resultContext.Failed("Failed to find series in TvDb"))
                        .MapAsync(s => (ISourceData)new SourceData<TvDbSeriesData>(this, s.Id,
                            new ItemIdentifier(embyItemData.Identifier.Index, Option<int>.None, s.SeriesName), s));

                case MediaItemTypeValue.Season:

                    var seasonIdentifier = embyItemData.Identifier;

                    return Right<ProcessFailedResult, ISourceData>(new IdentifierOnlySourceData(this, Option<int>.None,
                            seasonIdentifier))
                        .AsTask();

                case MediaItemTypeValue.Episode:

                    var seriesId = embyItemData.GetParentId(MediaItemTypes.Series, this);

                    return seriesId
                        .ToEitherAsync(resultContext.Failed("No TvDb Id found on parent series"))
                        .BindAsync(id =>
                        {
                            var tvDbEpisodeData = _tvDbClient.GetEpisodesAsync(id)
                                .ToEitherAsync(
                                    resultContext.Failed($"Failed to load parent series with TvDb Id '{id}'"))
                                .BindAsync(episodes => FindEpisode(episodes,
                                    embyItemData.Identifier.Name,
                                    embyItemData.Identifier.Index,
                                    embyItemData.Identifier.ParentIndex,
                                    resultContext));

                            return tvDbEpisodeData.MapAsync(e => (ISourceData)new SourceData<TvDbEpisodeData>(this,
                                e.Id,
                                new ItemIdentifier(e.AiredEpisodeNumber, e.AiredSeason, e.EpisodeName), e));
                        });

                default:
                    return Left<ProcessFailedResult, ISourceData>(resultContext.Failed("Unsupported item type"))
                        .AsTask();
            }
        }

        private Task<Either<ProcessFailedResult, TvDbEpisodeData>> FindEpisode(IEnumerable<TvDbEpisodeData> episodes,
            string title, Option<int> episodeIndex, Option<int> seasonIndex, ProcessResultContext resultContext)
        {
            var normalisedTitle = _titleNormaliser.GetNormalisedTitle(title);

            var episodeByIndexes = episodeIndex.Bind(i =>
                seasonIndex.Bind(si => episodes.Find(e => e.AiredEpisodeNumber == i && e.AiredSeason == si))
                    .Match(e => e, () => episodes.Find(e => e.AiredEpisodeNumber == i && e.AiredSeason == 1)));

            return episodeByIndexes.Match(Right<ProcessFailedResult, TvDbEpisodeData>,
                    () => Option<TvDbEpisodeData>
                        .Some(episodes.FirstOrDefault(e =>
                            _titleNormaliser.GetNormalisedTitle(e.EpisodeName) == normalisedTitle))
                        .ToEither(resultContext.Failed(
                            $"Failed to find TvDb episode for index {episodeIndex}, season {seasonIndex}, title '{title}'")))
                .AsTask();
        }
    }
}