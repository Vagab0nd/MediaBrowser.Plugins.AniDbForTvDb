namespace MediaBrowser.Plugins.Anime.AniDb
{
    internal interface IAniDbFileSpec
    {
        string DestinationFilePath { get; }

        bool IsGZipped { get; }

        string Url { get; }
    }

    internal interface IAniDbFileSpec<out TRoot> : IAniDbFileSpec where TRoot : class
    {
        TRoot ParseFile(string fileContent);
    }
}