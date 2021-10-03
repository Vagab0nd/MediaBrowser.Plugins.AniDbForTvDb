using System.IO;
using Emby.AniDbMetaStructure.Files;
using Emby.AniDbMetaStructure.Infrastructure;

namespace Emby.AniDbMetaStructure.AniDb.Titles
{
    internal class TitlesFileSpec : IRemoteFileSpec<TitleListData>
    {
        private const string TitlesPath = "anidb\\titles";
        private readonly string rootPath;
        private readonly IXmlSerialiser serializer;

        public TitlesFileSpec(string rootPath, IXmlSerialiser serializer)
        {
            this.rootPath = rootPath;
            this.serializer = serializer;
        }

        public string Url => "https://neyesha.myqnapcloud.com:8081/animetitles.xml";

        public string LocalPath => Path.Combine(this.rootPath, TitlesPath, "titles.xml");

        public bool IsGZipped => false;

        public ISerialiser Serialiser => this.serializer;
    }
}