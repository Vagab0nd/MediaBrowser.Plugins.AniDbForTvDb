namespace MediaBrowser.Plugins.AniMetadata.AniList.Data
{
    internal class AniListTitleData
    {
        public AniListTitleData(string english, string romaji, string native)
        {
            English = english;
            Romaji = romaji;
            Native = native;
        }

        public string English { get; }

        public string Romaji { get; }

        public string Native { get; }
    }
}