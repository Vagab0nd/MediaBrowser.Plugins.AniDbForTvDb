using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.Anime.AniDb.Mapping.Data;

namespace MediaBrowser.Plugins.Anime.Mapping
{
    public class Mapper
    {
        private readonly Dictionary<string, AniDbSeriesMappingData> _anidbMappings;
        private readonly ILogger _log;
        private readonly Dictionary<string, List<AniDbSeriesMappingData>> _tvdbMappings;

        public Mapper(ILogManager logManager, string animeListFile = "anime-list.xml")
            : this(logManager, new AnimeMappingListData())
        {
        }

        public Mapper(ILogManager logManager, AnimeMappingListData list)
        {
            _log = logManager.GetLogger(nameof(Mapper));
            _anidbMappings = new Dictionary<string, AniDbSeriesMappingData>();
            _tvdbMappings = new Dictionary<string, List<AniDbSeriesMappingData>>();

            int n;
            foreach (var anime in list.AnimeSeriesMapping.Where(x => int.TryParse(x.TvDbId, out n)))
            {
                _anidbMappings[anime.AnidbId] = anime;

                List<AniDbSeriesMappingData> l;
                if (!_tvdbMappings.TryGetValue(anime.TvDbId, out l))
                {
                    l = new List<AniDbSeriesMappingData>();
                    _tvdbMappings[anime.TvDbId] = l;
                }

                l.Add(anime);
            }
        }

        public string GetTvDbSeriesId(string anidbSeriesId)
        {
            var mapping = GetMapping(anidbSeriesId);

            return mapping?.TvDbId;
        }

        public int GetTvDbSeasonIndex(string anidbSeriesId, int anidbSeasonIndex, int anidbEpisodeIndex)
        {
            var mapping = GetMapping(anidbSeriesId);

            if (mapping == null)
            {
                return 1;
            }

            var episodeMappings = GetEpisodeMappings(mapping, anidbSeasonIndex);
            var episodeMapping = episodeMappings.FirstOrDefault(m => m.AnidbSeason == anidbSeasonIndex &&
                GetEpisodeMappings(m).Any(em => em.AniDb == anidbEpisodeIndex));

            if (episodeMapping != null)
            {
                return episodeMapping.TvDbSeason;
            }

            return GetDefaultTvDbSeasonIndex(mapping);
        }

        private int GetDefaultTvDbSeasonIndex(AniDbSeriesMappingData aniDbSeriesMappingDataMappingData)
        {
            var defaultTvDbSeasonIndexString = aniDbSeriesMappingDataMappingData.DefaultTvDbSeason;
            int defaultTvDbSeasonIndex;

            if (string.IsNullOrEmpty(defaultTvDbSeasonIndexString) || defaultTvDbSeasonIndexString == "a")
            {
                defaultTvDbSeasonIndex = 1;
            }
            else
            {
                defaultTvDbSeasonIndex = int.Parse(defaultTvDbSeasonIndexString);
            }

            return defaultTvDbSeasonIndex;
        }

        public int? GetTvDbEpisodeIndex(string anidbSeriesId, int anidbSeasonIndex, int anidbEpisodeIndex)
        {
            var mapping = GetMapping(anidbSeriesId);

            if (mapping == null)
            {
                return null;
            }

            if (mapping.DefaultTvDbSeason == "a")
            {
                return anidbEpisodeIndex;
            }

            var tvDbEpisodeIndex = GetEpisodeMappings(mapping, anidbSeasonIndex)
                .Select(m => GetTvDbEpisodeIndex(anidbEpisodeIndex, m)).FirstOrDefault(id => id.HasValue);

            if (tvDbEpisodeIndex.HasValue)
            {
                return tvDbEpisodeIndex;
            }

            var episodeIndexOffset = mapping.EpisodeOffsetSpecified ? mapping.EpisodeOffset : 0;

            return anidbEpisodeIndex + episodeIndexOffset;
        }

        private AniDbSeriesMappingData GetMapping(string anidbSeriesId)
        {
            AniDbSeriesMappingData seriesMappingData;

            _anidbMappings.TryGetValue(anidbSeriesId, out seriesMappingData);

            return seriesMappingData;
        }

        private IEnumerable<AnimeEpisodeGroupMappingData> GetEpisodeMappings(AniDbSeriesMappingData aniDbSeriesMappingDataMappingData,
            int anidbSeasonIndex)
        {
            return aniDbSeriesMappingDataMappingData.GroupMappingList?.Where(x => x.AnidbSeason == anidbSeasonIndex) ??
                new List<AnimeEpisodeGroupMappingData>();
        }

        public AniDbEpisode ToAnidb(TvdbEpisode tvdb)
        {
            List<AniDbSeriesMappingData> animeList;
            if (!_tvdbMappings.TryGetValue(tvdb.Series, out animeList))
            {
                return null;
            }

            // look for exact mapping in mapping list
            foreach (var anime in animeList.Where(x => x.GroupMappingList != null))
            {
                var mappings = anime.GroupMappingList.Where(x => x.TvDbSeason == tvdb.Season);
                foreach (var mapping in mappings)
                {
                    var episode = FindTvdbEpisodeMapping(tvdb, mapping);

                    if (episode != null)
                    {
                        return new AniDbEpisode
                        {
                            Series = anime.AnidbId,
                            Season = mapping.AnidbSeason,
                            Index = episode.Value
                        };
                    }
                }
            }

            var seasonMatch = animeList
                .Select(x => new { Season = Parse(x.DefaultTvDbSeason), Match = x })
                .Where(x => x.Season == tvdb.Season)
                .Select(x => new { Offset = x.Match.EpisodeOffsetSpecified ? x.Match.EpisodeOffset : 0, x.Match })
                .Where(x => x.Offset <= tvdb.Index)
                .OrderByDescending(x => x.Offset)
                .FirstOrDefault();

            if (seasonMatch != null)
            {
                return new AniDbEpisode
                {
                    Series = seasonMatch.Match.AnidbId,
                    Season = 1,
                    Index = tvdb.Index - seasonMatch.Offset
                };
            }

            // absolute episode numbers match
            var absolute = animeList.FirstOrDefault(x => x.DefaultTvDbSeason == "a");
            if (absolute != null)
            {
                return new AniDbEpisode
                {
                    Series = absolute.AnidbId,
                    Season = 1,
                    Index = tvdb.Index
                };
            }

            return null;
        }

        private int? Parse(string s)
        {
            int x;
            if (int.TryParse(s, out x))
            {
                return x;
            }

            return null;
        }

        public TvdbEpisode ToTvdb(AniDbEpisode aniDb)
        {
            AniDbSeriesMappingData aniDbSeriesMappingData;
            if (!_anidbMappings.TryGetValue(aniDb.Series, out aniDbSeriesMappingData))
            {
                _log.Debug("Anidb Id was null");
                return null;
            }


            // look for exact mapping in mapping list
            if (aniDbSeriesMappingData.GroupMappingList != null)
            {
                var mappings = aniDbSeriesMappingData.GroupMappingList.Where(x => x.AnidbSeason == aniDb.Season);
                foreach (var mapping in mappings)
                {
                    var episode = GetTvDbEpisodeIndex(aniDb.Index, mapping);

                    if (episode != null)
                    {
                        return new TvdbEpisode
                        {
                            Series = aniDbSeriesMappingData.TvDbId,
                            Season = mapping.TvDbSeason,
                            Index = episode.Value
                        };
                    }
                }
            }

            // absolute episode numbers match
            var season = aniDbSeriesMappingData.DefaultTvDbSeason;
            if (season == "a")
            {
                return new TvdbEpisode
                {
                    Series = aniDbSeriesMappingData.TvDbId,
                    Season = null,
                    Index = aniDb.Index
                };
            }

            // fallback to offset
            var offset = aniDbSeriesMappingData.EpisodeOffsetSpecified ? aniDbSeriesMappingData.EpisodeOffset : 0;

            return new TvdbEpisode
            {
                Series = aniDbSeriesMappingData.TvDbId,
                Season = int.Parse(season),
                Index = aniDb.Index + offset
            };
        }

        private int? FindTvdbEpisodeMapping(TvdbEpisode tvdb, AnimeEpisodeGroupMappingData groupMappingData)
        {
            var maps = GetEpisodeMappings(groupMappingData);
            var exact = maps.FirstOrDefault(x => x.TvDb == tvdb.Index);

            if (exact != null)
            {
                return exact.AniDb;
            }

            if (groupMappingData.OffsetSpecified)
            {
                var startInRange = !groupMappingData.StartSpecified ||
                    groupMappingData.Start + groupMappingData.Offset <= tvdb.Index;
                var endInRange = !groupMappingData.EndSpecified || groupMappingData.End + groupMappingData.Offset >= tvdb.Index;

                if (startInRange && endInRange)
                {
                    return tvdb.Index - groupMappingData.Offset;
                }
            }

            return null;
        }

        private int? GetTvDbEpisodeIndex(int anidbEpisodeIndex, AnimeEpisodeGroupMappingData groupMappingData)
        {
            var maps = GetEpisodeMappings(groupMappingData);
            var exact = maps.FirstOrDefault(x => x.AniDb == anidbEpisodeIndex);

            if (exact != null)
            {
                return exact.TvDb;
            }

            if (groupMappingData.OffsetSpecified)
            {
                var startInRange = !groupMappingData.StartSpecified || groupMappingData.Start <= anidbEpisodeIndex;
                var endInRange = !groupMappingData.EndSpecified || groupMappingData.End >= anidbEpisodeIndex;

                if (startInRange && endInRange)
                {
                    return anidbEpisodeIndex + groupMappingData.Offset;
                }
            }

            return null;
        }

        private List<AnimeEpisodeMappingData> GetEpisodeMappings(AnimeEpisodeGroupMappingData groupMappingData)
        {
            if (groupMappingData.ParsedMappings == null)
            {
                var pairs = groupMappingData.EpisodeMappingString.Split(';');
                //groupMapping.ParsedMappings = pairs
                //    .Where(x => !string.IsNullOrEmpty(x))
                //    .Select(x =>
                //    {
                //        var parts = x.Split('-');
                //        return new AnimeEpisodeMapping
                //        {
                //            AniDb = int.Parse(parts[0]),
                //            TvDb = int.Parse(parts[1])
                //        };
                //    }).ToList();
            }

            return null;
        }
    }
}