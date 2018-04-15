using System;
using System.Linq;
using MediaBrowser.Plugins.AniMetadata.Process;
using MediaBrowser.Plugins.AniMetadata.Process.Sources;
using NSubstitute;

namespace MediaBrowser.Plugins.AniMetadata.Tests.TestHelpers
{
    internal class TestSources : ISources
    {
        private readonly Lazy<IAniDbSource> _aniDbSource = new Lazy<IAniDbSource>(() => AniDbSource);
        private readonly Lazy<IAniListSource> _aniListSource = new Lazy<IAniListSource>(() => AniListSource);
        private readonly Lazy<ITvDbSource> _tvDbSource = new Lazy<ITvDbSource>(() => TvDbSource);

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

        public static IAniListSource AniListSource
        {
            get
            {
                var aniListSource = Substitute.For<IAniListSource>();
                aniListSource.Name.Returns(SourceNames.AniList);

                return aniListSource;
            }
        }

        public IAniDbSource AniDb => _aniDbSource.Value;

        public ITvDbSource TvDb => _tvDbSource.Value;

        public IAniListSource AniList => _aniListSource.Value;

        public ISource Get(string sourceName)
        {
            return new ISource[] { AniDb, TvDb, AniList }.Single(s => s.Name == sourceName);
        }
    }
}