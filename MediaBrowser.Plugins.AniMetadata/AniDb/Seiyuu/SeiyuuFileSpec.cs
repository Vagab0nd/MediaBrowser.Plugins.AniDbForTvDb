using System.IO;
using Emby.AniDbMetaStructure.Files;
using Emby.AniDbMetaStructure.Infrastructure;

namespace Emby.AniDbMetaStructure.AniDb.Seiyuu
{
    internal class SeiyuuFileSpec : ILocalFileSpec<SeiyuuListData>
    {
        private readonly string rootPath;
        private readonly IXmlSerialiser xmlSerialiser;

        public SeiyuuFileSpec(IXmlSerialiser xmlSerialiser, string rootPath)
        {
            this.xmlSerialiser = xmlSerialiser;
            this.rootPath = rootPath;
        }

        public string LocalPath => Path.Combine(this.rootPath, "anidb\\seiyuu.xml");

        public ISerialiser Serialiser => this.xmlSerialiser;
    }
}