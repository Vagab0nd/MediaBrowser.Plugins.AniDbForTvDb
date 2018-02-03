using System;
using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Providers.AniDb;
using static LanguageExt.Prelude;

namespace MediaBrowser.Plugins.AniMetadata.Process.Sources
{
    internal class AniDbSource : ISource
    {
        private readonly IAniDbClient _aniDbClient;
        private readonly IEpisodeMatcher _episodeMatcher;
        private readonly INewPluginConfiguration _pluginConfiguration;
        private readonly ITitleSelector _titleSelector;

        public AniDbSource(IAniDbClient aniDbClient, IEpisodeMatcher episodeMatcher,
            INewPluginConfiguration pluginConfiguration, ITitleSelector titleSelector)
        {
            _aniDbClient = aniDbClient;
            _episodeMatcher = episodeMatcher;
            _pluginConfiguration = pluginConfiguration;
            _titleSelector = titleSelector;
        }

        public string Name => "AniDb";

        public Task<Either<ProcessFailedResult, ISourceData>> LookupAsync(IMediaItem mediaItem)
        {
            throw new NotImplementedException();
        }

        public Task<Either<ProcessFailedResult, ISourceData>> LookupAsync(EmbyItemData embyItemData)
        {
            var resultContext = new ProcessResultContext(Name, embyItemData.Identifier.Name, embyItemData.ItemType);

            switch (embyItemData.ItemType)
            {
                case ItemType.Series:

                    return _aniDbClient.FindSeriesAsync(embyItemData.Identifier.Name)
                        .ToEitherAsync(resultContext.Failed("Failed to find series in AniDb"))
                        .BindAsync(s =>
                        {
                            var title = _titleSelector.SelectTitle(s.Titles, _pluginConfiguration.TitlePreference,
                                    embyItemData.Language)
                                .ToEither(resultContext.Failed("Failed to find a title"))
                                .AsTask();

                            return title.MapAsync(t => (ISourceData)new SourceData<AniDbSeriesData>(this, s.Id,
                                new ItemIdentifier(embyItemData.Identifier.Index, Option<int>.None, t.Title), s));
                        });

                case ItemType.Season:

                    var seasonIdentifier = new ItemIdentifier(embyItemData.Identifier.Index.IfNone(1),
                        embyItemData.Identifier.ParentIndex, embyItemData.Identifier.Name);

                    return Right<ProcessFailedResult, ISourceData>(new IdentifierOnlySourceData(this, Option<int>.None,
                            seasonIdentifier))
                        .AsTask();

                case ItemType.Episode:

                    var seriesId = embyItemData.GetParentId(ItemType.Series, this);

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
                                var title = _titleSelector.SelectTitle(e.Titles, _pluginConfiguration.TitlePreference,
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