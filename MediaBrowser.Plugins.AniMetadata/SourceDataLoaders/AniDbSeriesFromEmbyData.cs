﻿using System.Threading.Tasks;
using Emby.AniDbMetaStructure.AniDb;
using Emby.AniDbMetaStructure.AniDb.SeriesData;
using Emby.AniDbMetaStructure.Process;
using Emby.AniDbMetaStructure.Process.Sources;
using LanguageExt;

namespace Emby.AniDbMetaStructure.SourceDataLoaders
{
    /// <summary>
    ///     Loads series data from AniDb based on the data provided by Emby
    /// </summary>
    internal class AniDbSeriesFromEmbyData : IEmbySourceDataLoader
    {
        private readonly IAniDbClient aniDbClient;
        private readonly ISources sources;

        public AniDbSeriesFromEmbyData(IAniDbClient aniDbClient, ISources sources)
        {
            this.aniDbClient = aniDbClient;
            this.sources = sources;
        }

        public SourceName SourceName => SourceNames.AniDb;

        public bool CanLoadFrom(IMediaItemType mediaItemType)
        {
            return mediaItemType == MediaItemTypes.Series;
        }

        public Task<Either<ProcessFailedResult, ISourceData>> LoadFrom(IEmbyItemData embyItemData)
        {
            var resultContext = new ProcessResultContext(nameof(AniDbSeriesFromEmbyData), embyItemData.Identifier.Name,
                embyItemData.ItemType);

            return this.aniDbClient.FindSeriesAsync(embyItemData.Identifier.Name)
                .ToEitherAsync(resultContext.Failed("Failed to find series in AniDb"))
                .BindAsync(s =>
                {
                    var title = this.sources.AniDb.SelectTitle(s.Titles, embyItemData.Language, resultContext);

                    return title.Map(t => this.CreateSourceData(s, embyItemData, t));
                });
        }

        private ISourceData CreateSourceData(AniDbSeriesData seriesData, IEmbyItemData embyItemData, string title)
        {
            return new SourceData<AniDbSeriesData>(this.sources.AniDb, seriesData.Id,
                new ItemIdentifier(embyItemData.Identifier.Index, Option<int>.None, title), seriesData);
        }
    }
}