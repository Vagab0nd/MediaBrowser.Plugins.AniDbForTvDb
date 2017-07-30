using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.Anime.Providers.AniDB.Converter;

namespace MediaBrowser.Plugins.Anime.Providers.AniDB.Identity
{
    public class AnidbEpisodeIdentityProvider
    {
        private readonly ILogger _log;

        public AnidbEpisodeIdentityProvider(ILogManager logManager)
        {
            _log = logManager.GetLogger(nameof(AnidbEpisodeIdentityProvider));
        }

        public void Identify(EpisodeInfo info)
        {
            if (info.ProviderIds.ContainsKey(ProviderNames.AniDb))
            {
                _log.Debug($"{nameof(Identify)}: AniDb provider Id already set");
                return;
            }

            var inspectSeason = info.ParentIndexNumber == null ||
                info.ParentIndexNumber < 2 && Plugin.Instance.Configuration.UseAnidbOrderingWithSeasons;

            _log.Debug($"{nameof(Identify)}: inspectSeason {inspectSeason}");

            var series = info.SeriesProviderIds.GetOrDefault(ProviderNames.AniDb);
            if (!string.IsNullOrEmpty(series) && inspectSeason && info.IndexNumber != null)
            {
                string type = null;
                if (info.ParentIndexNumber != null)
                {
                    type = info.ParentIndexNumber == 0 ? "S" : null;
                }

                var id = new AnidbEpisodeIdentity(series, info.IndexNumber.Value, info.IndexNumberEnd, type);
                info.ProviderIds.Remove(ProviderNames.AniDb);
                info.ProviderIds.Add(ProviderNames.AniDb, id.ToString());

                _log.Debug($"{nameof(Identify)}: Added Id {id}");
            }
            else
            {
                _log.Debug(
                    $"{nameof(Identify)}: series '{series}', inspectSeason '{inspectSeason}', indexNumber '{info.IndexNumber}'");
            }
        }
    }
}