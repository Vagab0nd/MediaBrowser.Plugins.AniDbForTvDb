namespace MediaBrowser.Plugins.Anime.Files
{
    public interface IXmlSerialiser : ISerialiser
    {
        void SerialiseToFile<T>(string filePath, T data) where T : class;
    }
}