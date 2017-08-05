using System.IO;
using System.Xml.Serialization;
using MediaBrowser.Plugins.Anime.AniDb.SeriesData;

namespace MediaBrowser.Plugins.Anime.AniDb
{
    public class AniDbFileParser
    {
        public AniDbSeries ParseSeriesXml(string seriesXml)
        {
            var serializer = new XmlSerializer(typeof(AniDbSeries));

            using (var reader = new StringReader(seriesXml))
            {
                return serializer.Deserialize(reader) as AniDbSeries;
            }
        }
    }
}