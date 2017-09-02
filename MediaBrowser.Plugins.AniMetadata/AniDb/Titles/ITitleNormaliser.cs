namespace MediaBrowser.Plugins.AniMetadata.AniDb.Titles
{
    internal interface ITitleNormaliser
    {
        string GetNormalisedTitle(string title);
    }
}