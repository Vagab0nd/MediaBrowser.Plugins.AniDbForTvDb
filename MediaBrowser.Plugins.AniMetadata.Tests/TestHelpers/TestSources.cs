﻿using System;
using System.Linq;
using Emby.AniDbMetaStructure.Process;
using Emby.AniDbMetaStructure.Process.Sources;
using NSubstitute;

namespace Emby.AniDbMetaStructure.Tests.TestHelpers
{
    internal class TestSources : ISources
    {
        private readonly Lazy<IAniDbSource> aniDbSource = new Lazy<IAniDbSource>(() => AniDbSource);
        private readonly Lazy<IAniListSource> aniListSource = new Lazy<IAniListSource>(() => AniListSource);
        private readonly Lazy<ITvDbSource> tvDbSource = new Lazy<ITvDbSource>(() => TvDbSource);

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

        public IAniDbSource AniDb => this.aniDbSource.Value;

        public ITvDbSource TvDb => this.tvDbSource.Value;

        public IAniListSource AniList => this.aniListSource.Value;

        public ISource Get(string sourceName)
        {
            return new ISource[] { this.AniDb, this.TvDb, this.AniList }.Single(s => s.Name == sourceName);
        }
    }
}