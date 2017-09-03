using MediaBrowser.Model.Plugins;

namespace MediaBrowser.Plugins.AniMetadata.Configuration
{
    public class PluginConfiguration
        : BasePluginConfiguration
    {
        public PluginConfiguration()
        {
            TitlePreference = TitleType.Localized;
            MaxGenres = 5;
            MoveExcessGenresToTags = true;
            AddAnimeGenre = true;
        }

        public TitleType TitlePreference { get; set; }
        public int MaxGenres { get; set; }
        public bool MoveExcessGenresToTags { get; set; }
        public bool AddAnimeGenre { get; set; }

        public string TvDbApiKey { get; set; }
    }
}