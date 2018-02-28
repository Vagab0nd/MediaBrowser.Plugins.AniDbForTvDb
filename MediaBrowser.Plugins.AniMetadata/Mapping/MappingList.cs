using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.Mapping.Data;

namespace MediaBrowser.Plugins.AniMetadata.Mapping
{
    public class MappingList : IMappingList
    {
        public MappingList(IEnumerable<SeriesMapping> seriesMappings)
        {
            SeriesMappings = seriesMappings ?? new List<SeriesMapping>();
        }

        private IEnumerable<SeriesMapping> SeriesMappings { get; }

        public Option<ISeriesMapping> GetSeriesMappingFromAniDb(int aniDbSeriesId)
        {
            var mappings = SeriesMappings.Where(m => m.Ids.AniDbSeriesId == aniDbSeriesId).ToList();

            if (mappings.Count > 1)
            {
                throw new Exception($"Multiple series mappings match AniDb series Id '{aniDbSeriesId}'");
            }

            return mappings.SingleOrDefault();
        }

        public Option<IEnumerable<ISeriesMapping>> GetSeriesMappingsFromTvDb(int tvDbSeriesId)
        {
            var mappings = SeriesMappings.Where(m => m.Ids.TvDbSeriesId == tvDbSeriesId).ToList();

            return !mappings.Any() ? Option<IEnumerable<ISeriesMapping>>.None : mappings;
        }

        private static bool IsValidData(AnimeMappingListData data)
        {
            return data?.AnimeSeriesMapping != null;
        }

        public static Option<MappingList> FromData(AnimeMappingListData data)
        {
            if (!IsValidData(data))
            {
                return Option<MappingList>.None;
            }

            return new MappingList(data.AnimeSeriesMapping.Select(SeriesMapping.FromData).Somes());
        }
    }
}