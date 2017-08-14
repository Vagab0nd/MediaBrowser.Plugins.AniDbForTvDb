﻿using System.IO;
using MediaBrowser.Plugins.Anime.AniDb.Data;

namespace MediaBrowser.Plugins.Anime.AniDb
{
    internal class TitlesFileSpec : AniDbFileSpec<AniDbTitleList>
    {
        private const string TitlesPath = "anidb\\titles";
        private readonly string _rootPath;

        public TitlesFileSpec(IXmlFileParser xmlFileParser, string rootPath) : base(xmlFileParser)
        {
            _rootPath = rootPath;
        }

        public override string Url => "http://anidb.net/api/animetitles.xml";

        public override string DestinationFilePath => Path.Combine(_rootPath, TitlesPath, "titles.xml");

        public override bool IsGZipped => true;
    }
}