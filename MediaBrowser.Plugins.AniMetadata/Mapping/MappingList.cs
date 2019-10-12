using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Plugins.AniMetadata.Files;
using MediaBrowser.Plugins.AniMetadata.Mapping.Data;
using MediaBrowser.Plugins.AniMetadata.Process;
using static LanguageExt.Prelude;

namespace MediaBrowser.Plugins.AniMetadata.Mapping
{
    internal class MappingList : IMappingList
    {
        private readonly IFileCache fileCache;
        private readonly Lazy<Task<IEnumerable<SeriesMapping>>> mappingListTaskLazy;
        private readonly MappingsFileSpec mappingsFileSpec;

        public MappingList(IApplicationPaths applicationPaths, IFileCache fileCache)
        {
            this.mappingsFileSpec = new MappingsFileSpec(applicationPaths.CachePath);
            this.fileCache = fileCache;

            this.mappingListTaskLazy =
                new Lazy<Task<IEnumerable<SeriesMapping>>>(() => CreateMappingListAsync(CancellationToken.None));
        }

        public Task<Either<ProcessFailedResult, ISeriesMapping>> GetSeriesMappingFromAniDb(int aniDbSeriesId,
            ProcessResultContext resultContext)
        {
            return this.mappingListTaskLazy.Value
                .Map(seriesMappings => seriesMappings.Where(m => m.Ids.AniDbSeriesId == aniDbSeriesId).ToList())
                .Map(matchingSeriesMappings =>
                {
                    switch (matchingSeriesMappings.Count)
                    {
                        case 0:
                            return Left<ProcessFailedResult, ISeriesMapping>(
                                resultContext.Failed($"No series mapping for AniDb series Id '{aniDbSeriesId}'"));

                        case 1:
                            return Right<ProcessFailedResult, ISeriesMapping>(matchingSeriesMappings.Single());

                        default:
                            return Left<ProcessFailedResult, ISeriesMapping>(
                                resultContext.Failed(
                                    $"Multiple series mappings match AniDb series Id '{aniDbSeriesId}'"));
                    }
                });
        }

        public Task<Either<ProcessFailedResult, IEnumerable<ISeriesMapping>>> GetSeriesMappingsFromTvDb(
            int tvDbSeriesId, ProcessResultContext resultContext)
        {
            return this.mappingListTaskLazy.Value.Map(seriesMappings =>
                    seriesMappings.Where(m => m.Ids.TvDbSeriesId == tvDbSeriesId).ToList())
                .Map(matchingSeriesMappings =>
                    matchingSeriesMappings.Any()
                        ? Right<ProcessFailedResult, IEnumerable<ISeriesMapping>>(matchingSeriesMappings)
                        : Left<ProcessFailedResult, IEnumerable<ISeriesMapping>>(
                            resultContext.Failed($"No series mapping for TvDb series Id '{tvDbSeriesId}'")));
        }

        private async Task<IEnumerable<SeriesMapping>> CreateMappingListAsync(CancellationToken cancellationToken)
        {
            var mappingList = await this.fileCache.GetFileContentAsync(this.mappingsFileSpec, cancellationToken);

            return mappingList.Match(l =>
                {
                    if (!IsValidData(l))
                    {
                        return new List<SeriesMapping>();
                    }

                    return l.AnimeSeriesMapping.Select(SeriesMapping.FromData).Somes();
                },
                () => new List<SeriesMapping>());
        }

        private static bool IsValidData(AnimeMappingListData data)
        {
            return data?.AnimeSeriesMapping != null;
        }
    }
}