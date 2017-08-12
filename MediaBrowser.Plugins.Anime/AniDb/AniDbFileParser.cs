using System.IO;
using System.Xml;
using System.Xml.Serialization;
using MediaBrowser.Plugins.Anime.AniDb.Data;

namespace MediaBrowser.Plugins.Anime.AniDb
{
    public class AniDbFileParser : IAniDbFileParser
    {
        public AniDbSeries ParseSeriesXml(string seriesXml)
        {
            return Deserialise<AniDbSeries>(seriesXml);
        }

        public AniDbTitleList ParseTitleListXml(string titleListXml)
        {
            return Deserialise<AniDbTitleList>(titleListXml);
        }

        public SeiyuuList ParseSeiyuuListXml(string seiyuuListXml)
        {
            return Deserialise<SeiyuuList>(seiyuuListXml);
        }
        
        private T Deserialise<T>(string xml) where T : class
        {
            var serializer = new XmlSerializer(typeof(T));

            using (var reader = new StringReader(xml))
            {
                return serializer.Deserialize(reader) as T;
            }
        }
    }
}