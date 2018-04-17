using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.AniDb.Titles;
using MediaBrowser.Plugins.AniMetadata.AniList;
using MediaBrowser.Plugins.AniMetadata.AniList.Data;
using MediaBrowser.Plugins.AniMetadata.Process;
using MediaBrowser.Plugins.AniMetadata.Process.Sources;
using static LanguageExt.Prelude;

namespace MediaBrowser.Plugins.AniMetadata.SourceDataLoaders
{
    /// <summary>
    ///     Loads series data from AniList based on data provided by AniDb
    /// </summary>
    internal class AniListSeriesFromAniDb : ISourceDataLoader
    {
        private readonly IAniListClient _aniListClient;
        private readonly ISources _sources;
        private readonly ITitleNormaliser _titleNormaliser;

        public AniListSeriesFromAniDb(IAniListClient aniListClient, ISources sources, ITitleNormaliser titleNormaliser)
        {
            _aniListClient = aniListClient;
            _sources = sources;
            _titleNormaliser = titleNormaliser;
        }

        public SourceName SourceName => SourceNames.AniList;

        public bool CanLoadFrom(object sourceData)
        {
            return sourceData is ISourceData<AniDbSeriesData>;
        }

        public Task<Either<ProcessFailedResult, ISourceData>> LoadFrom(IMediaItem mediaItem, object sourceData)
        {
            var resultContext = new ProcessResultContext(nameof(AniListSeriesFromAniDb),
                mediaItem.EmbyData.Identifier.Name,
                mediaItem.ItemType);

            var aniDbSeriesData = (ISourceData<AniDbSeriesData>)sourceData;

            var distinctAniDbTitles = aniDbSeriesData.Data.Titles
                .Where(t => new[] { "en", "ja" }.Contains(t.Language, StringComparer.InvariantCultureIgnoreCase))
                .Select(t => t.Title)
                .Select(_titleNormaliser.GetNormalisedTitle)
                .Distinct()
                .OrderByDescending(t => t.Length);

            var sourceDataTask = distinctAniDbTitles
                .Fold(Left<ProcessFailedResult, AniListSeriesData>(resultContext.Failed("")).AsTask(),
                    (lastResult, currentTitle) =>
                    {
                        return lastResult.Bind(e =>
                            e.IsLeft ? FindSingleMatchingSeries(currentTitle, resultContext) : e.AsTask());
                    });

            return sourceDataTask.BindAsync(sd => CreateSourceDataWithTitle(mediaItem, sd, resultContext));
        }

        private Either<ProcessFailedResult, ISourceData> CreateSourceDataWithTitle(IMediaItem mediaItem,
            AniListSeriesData seriesData, ProcessResultContext resultContext)
        {
            return _sources.AniList.SelectTitle(seriesData.Title, mediaItem.EmbyData.Language, resultContext)
                .Map(t => CreateSourceData(seriesData, t));
        }

        private Task<Either<ProcessFailedResult, AniListSeriesData>> FindSingleMatchingSeries(string title,
            ProcessResultContext resultContext)
        {
            return _aniListClient.FindSeriesAsync(title, resultContext)
                .BindAsync(aniListSeriesData => FailUnlessOneResult(aniListSeriesData, resultContext));
        }

        private static Either<ProcessFailedResult, AniListSeriesData> FailUnlessOneResult(
            IEnumerable<AniListSeriesData> aniListSeriesData, ProcessResultContext resultContext)
        {
            return aniListSeriesData.Match(
                () => resultContext.Failed("No matching AniList series"),
                single => Right<ProcessFailedResult, AniListSeriesData>(single),
                (head, tail) =>
                    resultContext.Failed($"Found too many ({tail.Count() + 1}) matching AniList series"));
        }

        private ISourceData CreateSourceData(AniListSeriesData seriesData, string title)
        {
            return new SourceData<AniListSeriesData>(_sources.AniList, seriesData.Id,
                new ItemIdentifier(None, None, title), seriesData);
        }
    }
}