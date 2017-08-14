using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.Anime.Mapping;

namespace MediaBrowser.Plugins.Anime.Providers.AniDB.Converter
{
    public class AnidbTvdbEpisodeConverter
    {
        private readonly ILogger _log;
        private readonly Mapper _mapper;

        public AnidbTvdbEpisodeConverter(Mapper mapper, ILogManager logManager)
        {
            _mapper = mapper;
            _log = logManager.GetLogger(nameof(AnidbTvdbEpisodeConverter));
        }

        public bool Convert(EpisodeInfo info)
        {
            var anidb = info.ProviderIds.GetOrDefault(ProviderNames.AniDb);
            var tvdb = info.ProviderIds.GetOrDefault("Tvdb");

            if (string.IsNullOrEmpty(anidb) && !string.IsNullOrEmpty(tvdb))
            {
                _log.Debug("Converting tvdb to anidb");

                var converted = TvdbToAnidb(tvdb);
                if (converted != null)
                {
                    _log.Debug($"Converted '{tvdb}' to '{converted}'");

                    info.ProviderIds.Add(ProviderNames.AniDb, converted);
                    return true;
                }
                _log.Debug("Failed to convert");
            }

            var overrideTvdb = string.IsNullOrEmpty(tvdb)
                || info.ParentIndexNumber == null
                || info.ParentIndexNumber < 2 && Plugin.Instance.Configuration.UseAnidbOrderingWithSeasons;

            if (!string.IsNullOrEmpty(anidb) && overrideTvdb)
            {
                _log.Debug($"Converting anidb ('{anidb}') to tvdb");

                var converted = AnidbToTvdb(anidb);

                if (converted == tvdb)
                {
                    _log.Debug($"Tvdb already set to '{tvdb}'");
                    return true;
                }

                if (converted != null)
                {
                    _log.Debug($"Converted '{anidb}' to '{converted}'");

                    info.ProviderIds["Tvdb"] = converted;
                    return tvdb != converted;
                }
                _log.Debug("Failed to convert");
            }

            return false;
        }

        private string TvdbToAnidb(string tvdb)
        {
            var tvdbId = TvdbEpisodeIdentity.Parse(tvdb);
            if (tvdbId == null)
            {
                return null;
            }

            var converted = _mapper.ToAnidb(new TvdbEpisode
            {
                Series = tvdbId.Value.SeriesId,
                Season = tvdbId.Value.SeasonIndex,
                Index = tvdbId.Value.EpisodeNumber
            });

            if (converted == null)
            {
                return null;
            }

            int? end = null;
            if (tvdbId.Value.EpisodeNumberEnd != null)
            {
                var convertedEnd = _mapper.ToAnidb(new TvdbEpisode
                {
                    Series = tvdbId.Value.SeriesId,
                    Season = tvdbId.Value.SeasonIndex,
                    Index = tvdbId.Value.EpisodeNumberEnd.Value
                });

                if (convertedEnd != null && convertedEnd.Season == converted.Season)
                {
                    end = convertedEnd.Index;
                }
            }

            var id = new AnidbEpisodeIdentity(converted.Series, converted.Index, end, null);
            return id.ToString();
        }

        private string AnidbToTvdb(string anidb)
        {
            var anidbId = AnidbEpisodeIdentity.Parse(anidb);
            if (anidbId == null)
            {
                _log.Debug("Anidb Id was null");
                return null;
            }

            var converted = _mapper.ToTvdb(new AniDbEpisode
            {
                Series = anidbId.SeriesId,
                Season = string.IsNullOrEmpty(anidbId.EpisodeType) ? 1 : 0,
                Index = anidbId.EpisodeNumber
            });

            _log.Debug(
                $"Converted to series '{converted.Series}' season '{converted.Season}' episode index {converted.Index}");

            int? end = null;
            if (anidbId.EpisodeNumberEnd != null)
            {
                var convertedEnd = _mapper.ToAnidb(new TvdbEpisode
                {
                    Series = anidbId.SeriesId,
                    Season = string.IsNullOrEmpty(anidbId.EpisodeType) ? 1 : 0,
                    Index = anidbId.EpisodeNumberEnd.Value
                });

                if (convertedEnd.Season == converted.Season)
                {
                    end = convertedEnd.Index;
                }
            }

            var id = new TvdbEpisodeIdentity(converted.Series, converted.Season, converted.Index, end);
            return id.ToString();
        }
    }
}