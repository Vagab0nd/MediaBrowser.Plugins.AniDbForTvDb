using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Model.Logging;

namespace MediaBrowser.Plugins.Anime.AnimeLists
{
    public class Mapper
    {
        private readonly Dictionary<string, AnimeSeriesMapping> _anidbMappings;
        private readonly ILogger _log;
        private readonly Dictionary<string, List<AnimeSeriesMapping>> _tvdbMappings;

        public Mapper(ILogManager logManager, string animeListFile = "anime-list.xml")
            : this(logManager, new AnimeListDownloader(animeListFile).DownloadAsync().Result)
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
                GetEpisodeMappings(m).Any(em => em.Anidb == anidbEpisodeIndex));

            if (episodeMapping != null)
            {
                return episodeMapping.TvdbSeason;
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

        private IEnumerable<AnimelistMapping> GetEpisodeMappings(AnimeSeriesMapping animeSeriesMappingMapping, int anidbSeasonIndex)
        {
            return animeSeriesMappingMapping.MappingList?.Where(x => x.AnidbSeason == anidbSeasonIndex) ??
                new List<AnimelistMapping>();
        }

        public AnidbEpisode ToAnidb(TvdbEpisode tvdb)
        {
            List<AnimeSeriesMapping> animeList;
            if (!_tvdbMappings.TryGetValue(tvdb.Series, out animeList))
            {
                return null;
            }

            // look for exact mapping in mapping list
            foreach (var anime in animeList.Where(x => x.MappingList != null))
            {
                var mappings = anime.MappingList.Where(x => x.TvdbSeason == tvdb.Season);
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
            if (animeSeriesMapping.MappingList != null)
            {
                var mappings = animeSeriesMapping.MappingList.Where(x => x.AnidbSeason == anidb.Season);
                foreach (var mapping in mappings)
                {
                    var episode = GetTvDbEpisodeIndex(anidb.Index, mapping);

                    if (episode != null)
                    {
                        return new TvdbEpisode
                        {
                            Series = animeSeriesMapping.TvDbId,
                            Season = mapping.TvdbSeason,
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

        private int? FindTvdbEpisodeMapping(TvdbEpisode tvdb, AnimelistMapping mapping)
        {
            var maps = GetEpisodeMappings(mapping);
            var exact = maps.FirstOrDefault(x => x.Tvdb == tvdb.Index);

            if (exact != null)
            {
                return exact.Anidb;
            }

            if (mapping.OffsetSpecified)
            {
                var startInRange = !mapping.StartSpecified || mapping.Start + mapping.Offset <= tvdb.Index;
                var endInRange = !mapping.EndSpecified || mapping.End + mapping.Offset >= tvdb.Index;

                if (startInRange && endInRange)
                {
                    return tvdb.Index - mapping.Offset;
                }
            }

            return null;
        }

        private int? GetTvDbEpisodeIndex(int anidbEpisodeIndex, AnimelistMapping mapping)
        {
            var maps = GetEpisodeMappings(mapping);
            var exact = maps.FirstOrDefault(x => x.Anidb == anidbEpisodeIndex);

            if (exact != null)
            {
                return exact.Tvdb;
            }

            if (mapping.OffsetSpecified)
            {
                var startInRange = !mapping.StartSpecified || mapping.Start <= anidbEpisodeIndex;
                var endInRange = !mapping.EndSpecified || mapping.End >= anidbEpisodeIndex;

                if (startInRange && endInRange)
                {
                    return anidbEpisodeIndex + mapping.Offset;
                }
            }

            return null;
        }

        private List<EpisodeMapping> GetEpisodeMappings(AnimelistMapping mapping)
        {
            if (mapping.ParsedMappings == null)
            {
                var pairs = mapping.Value.Split(';');
                mapping.ParsedMappings = pairs
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Select(x =>
                    {
                        var parts = x.Split('-');
                        return new EpisodeMapping
                        {
                            Anidb = int.Parse(parts[0]),
                            Tvdb = int.Parse(parts[1])
                        };
                    }).ToList();
            }

            return mapping.ParsedMappings;
        }
    }
}