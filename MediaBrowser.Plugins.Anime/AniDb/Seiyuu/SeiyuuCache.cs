using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using MediaBrowser.Common.Configuration;

namespace MediaBrowser.Plugins.Anime.AniDb.Seiyuu
{
    public class SeiyuuCache : ISeiyuuCache
    {
        private readonly IXmlFileParser _fileParser;
        private readonly string _seiyuuFileLocation;

        public SeiyuuCache(IXmlFileParser fileParser, IApplicationPaths applicationPaths)
        {
            _fileParser = fileParser;
            _seiyuuFileLocation = Path.Combine(applicationPaths.CachePath, "anidb\\seiyuu.xml");
        }

        public void Add(IEnumerable<SeiyuuData> seiyuu)
        {
            var seiyuuList = GetAll();
            var newSeiyuu = seiyuu.Except(seiyuuList, new SeiyuuComparer()).ToList();

            if (!newSeiyuu.Any())
            {
                return;
            }

            SaveSeiyuuList(seiyuuList.Concat(newSeiyuu));
        }
        
        public IEnumerable<SeiyuuData> GetAll()
        {
            if (!File.Exists(_seiyuuFileLocation))
            {
                return new List<SeiyuuData>();
            }

            var seiyuuListXml = File.ReadAllText(_seiyuuFileLocation);

            return _fileParser.Parse<SeiyuuListData>(seiyuuListXml).Seiyuu;
        }

        private void SaveSeiyuuList(IEnumerable<SeiyuuData> seiyuuList)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_seiyuuFileLocation));

            using (var writer = new XmlTextWriter(_seiyuuFileLocation, Encoding.UTF8))
            {
                var serialiser = new XmlSerializer(typeof(SeiyuuListData));

                serialiser.Serialize(writer, new SeiyuuListData { Seiyuu = seiyuuList.ToArray() });
            }
        }

        private class SeiyuuComparer : IEqualityComparer<SeiyuuData>
        {
            public bool Equals(SeiyuuData x, SeiyuuData y)
            {
                return x != null && y != null && x.Id == y.Id;
            }

            public int GetHashCode(SeiyuuData obj)
            {
                return obj.Id.GetHashCode();
            }
        }
    }
}