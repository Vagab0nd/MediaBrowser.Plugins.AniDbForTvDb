using System.IO;
using MediaBrowser.Plugins.AniMetadata.Files;

namespace MediaBrowser.Plugins.AniMetadata.AniDb.Seiyuu
{
    using Infrastructure;

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