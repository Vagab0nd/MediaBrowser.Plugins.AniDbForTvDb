using System.Collections.Generic;
using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.SourceDataLoaders;

namespace MediaBrowser.Plugins.AniMetadata.Process.Sources
{
    internal class AniDbSource : IAniDbSource
    {
        private readonly IAniDbClient _aniDbClient;
        private readonly IEnumerable<IEmbySourceDataLoader> _embySourceDataLoaders;
        private readonly ITitlePreferenceConfiguration _titlePreferenceConfiguration;
        private readonly ITitleSelector _titleSelector;

        public AniDbSource(IAniDbClient aniDbClient, ITitlePreferenceConfiguration titlePreferenceConfiguration,
            ITitleSelector titleSelector, IEnumerable<IEmbySourceDataLoader> embySourceDataLoaders)
        {
            _aniDbClient = aniDbClient;
            _titlePreferenceConfiguration = titlePreferenceConfiguration;
            _titleSelector = titleSelector;
            _embySourceDataLoaders = embySourceDataLoaders;
        }

        public SourceName Name => SourceNames.AniDb;

        public Either<ProcessFailedResult, IEmbySourceDataLoader> GetEmbySourceDataLoader(IMediaItemType mediaItemType)
        {
            return _embySourceDataLoaders.Find(l => l.SourceName == Name && l.CanLoadFrom(mediaItemType))
                .ToEither(new ProcessFailedResult(Name, "", mediaItemType,
                    "No Emby source data loader for this source and media item type"));
        }

        public Task<Either<ProcessFailedResult, AniDbSeriesData>> GetSeriesData(IEmbyItemData embyItemData,
            ProcessResultContext resultContext)
        {
            return embyItemData.GetParentId(MediaItemTypes.Series, this)
                .ToEitherAsync(
                    resultContext.Failed("No AniDb Id found on parent series"))
                .BindAsync(aniDbSeriesId => _aniDbClient.GetSeriesAsync(aniDbSeriesId)
                    .ToEitherAsync(
                        resultContext.Failed($"Failed to load parent series with AniDb Id '{aniDbSeriesId}'")));
        }

        public Either<ProcessFailedResult, string> SelectTitle(IEnumerable<ItemTitleData> titles,
            string metadataLanguage, ProcessResultContext resultContext)
        {
            return _titleSelector.SelectTitle(titles, _titlePreferenceConfiguration.TitlePreference, metadataLanguage)
                .Map(t => t.Title)
                .ToEither(resultContext.Failed("Failed to find a title"));
        }
    }
}