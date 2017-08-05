using System.IO;
using System.Xml.Serialization;
using MediaBrowser.Plugins.Anime.AniDb.Data;

namespace MediaBrowser.Plugins.Anime.AniDb
{
    public class AniDbFileParser
    {
        public AniDbSeries ParseSeriesXml(string seriesXml)
        {
            return Deserialise<AniDbSeries>(seriesXml);
        }

        public AniDbTitleList ParseTitleListXml(string titleListXml)
        {
            return Deserialise<AniDbTitleList>(titleListXml);
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