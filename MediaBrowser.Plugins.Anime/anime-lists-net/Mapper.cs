using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Model.Logging;

namespace AnimeLists
{
    public class Mapper
    {
        private readonly Dictionary<string, AnimelistAnime> _anidbMappings;
        private readonly ILogger _log;
        private readonly Dictionary<string, List<AnimelistAnime>> _tvdbMappings;

        public Mapper(ILogManager logManager, string animeListFile = "anime-list.xml")
            : this(logManager, new Downloader(animeListFile).Download().Result)
        {
        }

        public Mapper(ILogManager logManager, Animelist list)
        {
            _log = logManager.GetLogger(nameof(Mapper));
            _anidbMappings = new Dictionary<string, AnimelistAnime>();
            _tvdbMappings = new Dictionary<string, List<AnimelistAnime>>();

            int n;
            foreach (var anime in list.Anime.Where(x => int.TryParse(x.TvdbId, out n)))
            {
                _anidbMappings[anime.AnidbId] = anime;

                List<AnimelistAnime> l;
                if (!_tvdbMappings.TryGetValue(anime.TvdbId, out l))
                {
                    l = new List<AnimelistAnime>();
                    _tvdbMappings[anime.TvdbId] = l;
                }

                l.Add(anime);
            }
        }

        public string GetTvDbSeriesId(string anidbSeriesId)
        {
            var mapping = GetMapping(anidbSeriesId);

            return mapping?.TvdbId;
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

        private int GetDefaultTvDbSeasonIndex(AnimelistAnime animeMapping)
        {
            var defaultTvDbSeasonIndexString = animeMapping.DefaultTvdbSeason;
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

            if (mapping.DefaultTvdbSeason == "a")
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

        private AnimelistAnime GetMapping(string anidbSeriesId)
        {
            AnimelistAnime mapping;

            _anidbMappings.TryGetValue(anidbSeriesId, out mapping);

            return mapping;
        }

        private IEnumerable<AnimelistMapping> GetEpisodeMappings(AnimelistAnime animeMapping, int anidbSeasonIndex)
        {
            return animeMapping.Mappinglist?.Where(x => x.AnidbSeason == anidbSeasonIndex) ??
                new List<AnimelistMapping>();
        }

        public AnidbEpisode ToAnidb(TvdbEpisode tvdb)
        {
            List<AnimelistAnime> animeList;
            if (!_tvdbMappings.TryGetValue(tvdb.Series, out animeList))
            {
                return null;
            }

            // look for exact mapping in mapping list
            foreach (var anime in animeList.Where(x => x.Mappinglist != null))
            {
                var mappings = anime.Mappinglist.Where(x => x.TvdbSeason == tvdb.Season);
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
                .Select(x => new { Season = Parse(x.DefaultTvdbSeason), Match = x })
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
            var absolute = animeList.FirstOrDefault(x => x.DefaultTvdbSeason == "a");
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
            AnimelistAnime anime;
            if (!_anidbMappings.TryGetValue(anidb.Series, out anime))
            {
                _log.Debug("Anidb Id was null");
                return null;
            }


            // look for exact mapping in mapping list
            if (anime.Mappinglist != null)
            {
                var mappings = anime.Mappinglist.Where(x => x.AnidbSeason == anidb.Season);
                foreach (var mapping in mappings)
                {
                    var episode = GetTvDbEpisodeIndex(anidb.Index, mapping);

                    if (episode != null)
                    {
                        return new TvdbEpisode
                        {
                            Series = anime.TvdbId,
                            Season = mapping.TvdbSeason,
                            Index = episode.Value
                        };
                    }
                }
            }

            // absolute episode numbers match
            var season = anime.DefaultTvdbSeason;
            if (season == "a")
            {
                return new TvdbEpisode
                {
                    Series = anime.TvdbId,
                    Season = null,
                    Index = anidb.Index
                };
            }

            // fallback to offset
            var offset = anime.EpisodeOffsetSpecified ? anime.EpisodeOffset : 0;

            return new TvdbEpisode
            {
                Series = anime.TvdbId,
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