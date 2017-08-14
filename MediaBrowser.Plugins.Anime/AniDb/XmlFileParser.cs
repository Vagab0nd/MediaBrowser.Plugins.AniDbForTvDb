using System.IO;
using System.Xml;
using System.Xml.Serialization;
using MediaBrowser.Plugins.Anime.AniDb.Data;

namespace MediaBrowser.Plugins.Anime.AniDb
{
    public class XmlFileParser : IXmlFileParser
    {
        public T Parse<T>(string xml) where T : class
        {
            var serializer = new XmlSerializer(typeof(T));

            using (var reader = new StringReader(xml))
            {
                return serializer.Deserialize(reader) as T;
            }
        }
    }
}