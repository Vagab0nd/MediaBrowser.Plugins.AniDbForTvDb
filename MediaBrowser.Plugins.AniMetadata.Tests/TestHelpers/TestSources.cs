using System.Linq;
using MediaBrowser.Plugins.AniMetadata.Process;
using MediaBrowser.Plugins.AniMetadata.Process.Sources;
using NSubstitute;

namespace MediaBrowser.Plugins.AniMetadata.Tests.TestHelpers
{
    internal class TestSources : ISources
    {
        public static IAniDbSource AniDbSource
        {
            get
            {
                var aniDb = Substitute.For<IAniDbSource>();
                aniDb.Name.Returns(SourceNames.AniDb);

                return aniDb;
            }
        }

        public static ITvDbSource TvDbSource
        {
            get
            {
                var tvDbSource = Substitute.For<ITvDbSource>();
                tvDbSource.Name.Returns(SourceNames.TvDb);

                return tvDbSource;
            }
        }

        public IAniDbSource AniDb => AniDbSource;

        public ITvDbSource TvDb => TvDbSource;

        public ISource Get(string sourceName)
        {
            return new ISource[] { AniDb, TvDb }.Single(s => s.Name == sourceName);
        }
    }
}