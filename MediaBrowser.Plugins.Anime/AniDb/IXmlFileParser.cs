namespace MediaBrowser.Plugins.Anime.AniDb
{
    public interface IXmlFileParser
    {
        T Parse<T>(string xml) where T : class;
    }
}