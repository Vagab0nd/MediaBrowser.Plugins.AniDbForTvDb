using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Plugins.Anime.AniDb.Data;

namespace MediaBrowser.Plugins.Anime.AniDb
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

        public void Add(IEnumerable<Seiyuu> seiyuu)
        {
            var seiyuuList = GetAll();
            var newSeiyuu = seiyuu.Except(seiyuuList, new SeiyuuComparer()).ToList();

            if (!newSeiyuu.Any())
            {
                return;
            }

            SaveSeiyuuList(seiyuuList.Concat(newSeiyuu));
        }
        
        public IEnumerable<Seiyuu> GetAll()
        {
            if (!File.Exists(_seiyuuFileLocation))
            {
                return new List<Seiyuu>();
            }

            var seiyuuListXml = File.ReadAllText(_seiyuuFileLocation);

            return _fileParser.Parse<SeiyuuList>(seiyuuListXml).Seiyuu;
        }

        private void SaveSeiyuuList(IEnumerable<Seiyuu> seiyuuList)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_seiyuuFileLocation));

            using (var writer = new XmlTextWriter(_seiyuuFileLocation, Encoding.UTF8))
            {
                var serialiser = new XmlSerializer(typeof(SeiyuuList));

                serialiser.Serialize(writer, new SeiyuuList { Seiyuu = seiyuuList.ToArray() });
            }
        }

        private class SeiyuuComparer : IEqualityComparer<Seiyuu>
        {
            public bool Equals(Seiyuu x, Seiyuu y)
            {
                return x != null && y != null && x.Id == y.Id;
            }

            public int GetHashCode(Seiyuu obj)
            {
                return obj.Id.GetHashCode();
            }
        }
    }
}