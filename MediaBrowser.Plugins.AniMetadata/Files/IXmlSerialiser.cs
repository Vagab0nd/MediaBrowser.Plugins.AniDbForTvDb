using Emby.AniDbMetaStructure.Infrastructure;

namespace Emby.AniDbMetaStructure.Files
{
    public interface IXmlSerialiser : ISerialiser
    {
        void SerialiseToFile<T>(string filePath, T data) where T : class;
    }
}