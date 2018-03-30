using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.AniDb.Titles;
using MediaBrowser.Plugins.AniMetadata.Process;
using MediaBrowser.Plugins.AniMetadata.Process.Sources;
using MediaBrowser.Plugins.AniMetadata.TvDb;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;
using static LanguageExt.Prelude;

namespace MediaBrowser.Plugins.AniMetadata.SourceDataLoaders
{
    /// <summary>
    ///     Loads episode data from TvDb based on the data provided by Emby
    /// </summary>
    internal class TvDbEpisodeFromEmbyData : IEmbySourceDataLoader
    {
        private readonly ISources _sources;
        private readonly ITitleNormaliser _titleNormaliser;
        private readonly ITvDbClient _tvDbClient;

        public TvDbEpisodeFromEmbyData(ISources sources, ITvDbClient tvDbClient, ITitleNormaliser titleNormaliser)
        {
            _sources = sources;
            _tvDbClient = tvDbClient;
            _titleNormaliser = titleNormaliser;
        }

        public SourceName SourceName => SourceNames.TvDb;

        public bool CanLoadFrom(IMediaItemType mediaItemType)
        {
            return mediaItemType == MediaItemTypes.Episode;
        }

        public Task<Either<ProcessFailedResult, ISourceData>> LoadFrom(IEmbyItemData embyItemData)
        {
            var resultContext = new ProcessResultContext(nameof(TvDbEpisodeFromEmbyData), embyItemData.Identifier.Name,
                embyItemData.ItemType);

            var seriesId = embyItemData.GetParentId(MediaItemTypes.Series, _sources.TvDb);

            return seriesId.ToEitherAsync(resultContext.Failed("No TvDb Id found on parent series"))
                .BindAsync(id => _tvDbClient.GetEpisodesAsync(id)
                    .ToEitherAsync(resultContext.Failed($"Failed to load parent series with TvDb Id '{id}'")))
                .BindAsync(episodes => FindEpisode(episodes, embyItemData.Identifier.Name,
                    embyItemData.Identifier.Index, embyItemData.Identifier.ParentIndex, resultContext))
                .MapAsync(CreateSourceData);
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
                            $"Failed to find TvDb episode")))
                .AsTask();
        }

        private ISourceData CreateSourceData(TvDbEpisodeData e)
        {
            return new SourceData<TvDbEpisodeData>(_sources.TvDb, e.Id,
                new ItemIdentifier(e.AiredEpisodeNumber, e.AiredSeason, e.EpisodeName), e);
        }
    }
}