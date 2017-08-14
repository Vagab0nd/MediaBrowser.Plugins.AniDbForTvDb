namespace MediaBrowser.Plugins.Anime.AniDb
{
    internal abstract class AniDbFileSpec<TRoot> : IAniDbFileSpec<TRoot> where TRoot : class
    {
        private readonly IXmlFileParser _xmlFileParser;

        protected AniDbFileSpec(IXmlFileParser xmlFileParser)
        {
            _xmlFileParser = xmlFileParser;
        }

        public abstract string Url { get; }

        public abstract string DestinationFilePath { get; }

        public abstract bool IsGZipped { get; }

        public TRoot ParseFile(string fileContent)
        {
            return _xmlFileParser.Parse<TRoot>(fileContent);
        }
    }
}