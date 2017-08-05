namespace MediaBrowser.Plugins.Anime.AniDb
{
    internal abstract class AniDbFileSpec
    {
        public abstract string Url { get; }

        public abstract string DestinationFilePath { get; }
    }
}