namespace MediaBrowser.Plugins.AniMetadata.Files
{
    using Infrastructure;

    public interface IXmlSerialiser : ISerialiser
    {
        void SerialiseToFile<T>(string filePath, T data) where T : class;
    }
}