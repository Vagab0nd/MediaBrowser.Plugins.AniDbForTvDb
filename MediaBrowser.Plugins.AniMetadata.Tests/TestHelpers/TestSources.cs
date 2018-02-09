using System.Linq;
using MediaBrowser.Plugins.AniMetadata.Process;
using NSubstitute;

namespace MediaBrowser.Plugins.AniMetadata.Tests.TestHelpers
{
    internal class TestSources : ISources
    {
        public TestSources()
        {
            AniDb = Substitute.For<ISource>();
            TvDb = Substitute.For<ISource>();
        }

        public ISource AniDb { get; }

        public ISource TvDb { get; }

        public ISource Get(string sourceName)
        {
            return new[] { AniDb, TvDb }.Single(s => s.Name == sourceName);
        }
    }
}