using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace MediaBrowser.Plugins.Anime.AniDb
{
    public class XmlSerialiser : IXmlSerialiser
    {
        public T Deserialise<T>(string xml) where T : class
        {
            var serializer = new XmlSerializer(typeof(T));

            using (var reader = new StringReader(xml))
            {
                return serializer.Deserialize(reader) as T;
            }
        }

        public void SerialiseToFile<T>(string filePath, T data) where T : class
        {
            using (var writer = new XmlTextWriter(filePath, Encoding.UTF8))
            {
                var serialiser = new XmlSerializer(typeof(T));

                serialiser.Serialize(writer, data);
            }
        }
    }
}