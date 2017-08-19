using System;
using System.Collections.Generic;
using System.Linq;
using Functional.Maybe;
using MediaBrowser.Plugins.Anime.AniDb.Mapping.Data;

namespace MediaBrowser.Plugins.Anime.AniDb.Mapping
{
    public class MappingList : IMappingList
    {
        public MappingList(IEnumerable<SeriesMapping> seriesMappings)
        {
            SeriesMappings = seriesMappings ?? new List<SeriesMapping>();
        }

        private IEnumerable<SeriesMapping> SeriesMappings { get; }

        public Maybe<SeriesMapping> GetSeriesMapping(int aniDbSeriesId)
        {
            var mappings = SeriesMappings.Where(m => m.Ids.AniDbSeriesId == aniDbSeriesId).ToList();

            if (mappings.Count > 1)
            {
                throw new Exception($"Multiple series mappings match AniDb series Id '{aniDbSeriesId}'");
            }

            return mappings.SingleOrDefault().ToMaybe();
        }

        private static bool IsValidData(AnimeMappingListData data)
        {
            return data?.AnimeSeriesMapping != null;
        }

        public static Maybe<MappingList> FromData(AnimeMappingListData data)
        {
            if (!IsValidData(data))
            {
                return Maybe<MappingList>.Nothing;
            }

            return new MappingList(data.AnimeSeriesMapping.Select(SeriesMapping.FromData).SelectWhereValueExist(m => m))
                .ToMaybe();
        }
    }
}