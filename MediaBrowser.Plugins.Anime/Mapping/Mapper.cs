using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.Anime.Mapping.Lists;

namespace MediaBrowser.Plugins.Anime.Mapping
{
    public class Mapper
    {
        private readonly Dictionary<string, AnimeSeriesMapping> _anidbMappings;
        private readonly ILogger _log;
        private readonly Dictionary<string, List<AnimeSeriesMapping>> _tvdbMappings;

        public Mapper(ILogManager logManager, string animeListFile = "anime-list.xml")
            : this(logManager, new AnimeMappingListFactory(animeListFile).CreateMappingListAsync().Result)
        {
        }

        public Mapper(ILogManager logManager, AnimeMappingList list)
        {
            _log = logManager.GetLogger(nameof(Mapper));
            _anidbMappings = new Dictionary<string, AnimeSeriesMapping>();
            _tvdbMappings = new Dictionary<string, List<AnimeSeriesMapping>>();

            int n;
            foreach (var anime in list.AnimeSeriesMapping.Where(x => int.TryParse(x.TvDbId, out n)))
            {
                _anidbMappings[anime.AnidbId] = anime;

                List<AnimeSeriesMapping> l;
                if (!_tvdbMappings.TryGetValue(anime.TvDbId, out l))
                {
                    l = new List<AnimeSeriesMapping>();
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

        private int GetDefaultTvDbSeasonIndex(AnimeSeriesMapping animeSeriesMappingMapping)
        {
            var defaultTvDbSeasonIndexString = animeSeriesMappingMapping.DefaultTvDbSeason;
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

        private AnimeSeriesMapping GetMapping(string anidbSeriesId)
        {
            AnimeSeriesMapping seriesMapping;

            _anidbMappings.TryGetValue(anidbSeriesId, out seriesMapping);

            return seriesMapping;
        }

        private IEnumerable<AnimeEpisodeGroupMapping> GetEpisodeMappings(AnimeSeriesMapping animeSeriesMappingMapping, int anidbSeasonIndex)
        {
            return animeSeriesMappingMapping.GroupMappingList?.Where(x => x.AnidbSeason == anidbSeasonIndex) ??
                new List<AnimeEpisodeGroupMapping>();
        }

        public AnidbEpisode ToAnidb(TvdbEpisode tvdb)
        {
            List<AnimeSeriesMapping> animeList;
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
                        return new AnidbEpisode
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
                return new AnidbEpisode
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
                return new AnidbEpisode
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

        public TvdbEpisode ToTvdb(AnidbEpisode anidb)
        {
            AnimeSeriesMapping animeSeriesMapping;
            if (!_anidbMappings.TryGetValue(anidb.Series, out animeSeriesMapping))
            {
                _log.Debug("Anidb Id was null");
                return null;
            }


            // look for exact mapping in mapping list
            if (animeSeriesMapping.GroupMappingList != null)
            {
                var mappings = animeSeriesMapping.GroupMappingList.Where(x => x.AnidbSeason == anidb.Season);
                foreach (var mapping in mappings)
                {
                    var episode = GetTvDbEpisodeIndex(anidb.Index, mapping);

                    if (episode != null)
                    {
                        return new TvdbEpisode
                        {
                            Series = animeSeriesMapping.TvDbId,
                            Season = mapping.TvDbSeason,
                            Index = episode.Value
                        };
                    }
                }
            }

            // absolute episode numbers match
            var season = animeSeriesMapping.DefaultTvDbSeason;
            if (season == "a")
            {
                return new TvdbEpisode
                {
                    Series = animeSeriesMapping.TvDbId,
                    Season = null,
                    Index = anidb.Index
                };
            }

            // fallback to offset
            var offset = animeSeriesMapping.EpisodeOffsetSpecified ? animeSeriesMapping.EpisodeOffset : 0;

            return new TvdbEpisode
            {
                Series = animeSeriesMapping.TvDbId,
                Season = int.Parse(season),
                Index = anidb.Index + offset
            };
        }

        private int? FindTvdbEpisodeMapping(TvdbEpisode tvdb, AnimeEpisodeGroupMapping groupMapping)
        {
            var maps = GetEpisodeMappings(groupMapping);
            var exact = maps.FirstOrDefault(x => x.TvDb == tvdb.Index);

            if (exact != null)
            {
                return exact.AniDb;
            }

            if (groupMapping.OffsetSpecified)
            {
                var startInRange = !groupMapping.StartSpecified || groupMapping.Start + groupMapping.Offset <= tvdb.Index;
                var endInRange = !groupMapping.EndSpecified || groupMapping.End + groupMapping.Offset >= tvdb.Index;

                if (startInRange && endInRange)
                {
                    return tvdb.Index - groupMapping.Offset;
                }
            }

            return null;
        }

        private int? GetTvDbEpisodeIndex(int anidbEpisodeIndex, AnimeEpisodeGroupMapping groupMapping)
        {
            var maps = GetEpisodeMappings(groupMapping);
            var exact = maps.FirstOrDefault(x => x.AniDb == anidbEpisodeIndex);

            if (exact != null)
            {
                return exact.TvDb;
            }

            if (groupMapping.OffsetSpecified)
            {
                var startInRange = !groupMapping.StartSpecified || groupMapping.Start <= anidbEpisodeIndex;
                var endInRange = !groupMapping.EndSpecified || groupMapping.End >= anidbEpisodeIndex;

                if (startInRange && endInRange)
                {
                    return anidbEpisodeIndex + groupMapping.Offset;
                }
            }

            return null;
        }

        private List<AnimeEpisodeMapping> GetEpisodeMappings(AnimeEpisodeGroupMapping groupMapping)
        {
            if (groupMapping.ParsedMappings == null)
            {
                var pairs = groupMapping.Value.Split(';');
                groupMapping.ParsedMappings = pairs
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Select(x =>
                    {
                        var parts = x.Split('-');
                        return new AnimeEpisodeMapping
                        {
                            AniDb = int.Parse(parts[0]),
                            TvDb = int.Parse(parts[1])
                        };
                    }).ToList();
            }

            return groupMapping.ParsedMappings;
        }
    }
}