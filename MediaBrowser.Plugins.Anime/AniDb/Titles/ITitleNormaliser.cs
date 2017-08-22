namespace MediaBrowser.Plugins.Anime.AniDb.Titles
{
    internal interface ITitleNormaliser
    {
        string GetNormalisedTitle(string title);
    }
}