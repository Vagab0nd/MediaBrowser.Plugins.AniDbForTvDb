using MediaBrowser.Model.Plugins;

namespace MediaBrowser.Plugins.Anime.Configuration
{
    public class PluginConfiguration
        : BasePluginConfiguration
    {
        public PluginConfiguration()
        {
            TitlePreference = TitleType.Localized;
            AllowAutomaticMetadataUpdates = true;
            TidyGenreList = true;
            MaxGenres = 5;
            MoveExcessGenresToTags = true;
            AddAnimeGenre = true;
            UseAnidbOrderingWithSeasons = false;
            UseAnidbDescriptions = false;
        }

        public TitleType TitlePreference { get; set; }
        public bool AllowAutomaticMetadataUpdates { get; set; }
        public bool TidyGenreList { get; set; }
        public int MaxGenres { get; set; }
        public bool MoveExcessGenresToTags { get; set; }
        public bool AddAnimeGenre { get; set; }
        public bool UseAnidbOrderingWithSeasons { get; set; }
        public bool UseAnidbDescriptions { get; set; }

        public string TvDbApiKey { get; set; }
    }
}