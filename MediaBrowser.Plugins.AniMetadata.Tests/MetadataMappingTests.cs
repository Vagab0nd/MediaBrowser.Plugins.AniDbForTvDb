using FluentAssertions;
using MediaBrowser.Plugins.AniMetadata.MetadataMapping;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests
{
    [TestFixture]
    public class MetadataMappingTests
    {
        private class AniDbSource
        {
            public string AniDbValue => "AniDb";
        }

        private class TvDbSource
        {
            public string TvDbValue => "TvDb";
        }

        private class Metadata
        {
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
            public string TargetValueA { get; set; } = "TargetValueA";

            public string TargetValueB { get; set; } = "TargetValueB";
        }

        [Test]
        public void Apply_CopiesSourceDataToTarget()
        {
            var aniDbMapping =
                PropertyMapping<AniDbSource, Metadata>.Create(t => t.TargetValueA,
                    (s, t) => t.TargetValueA = s.AniDbValue);
            var tvDbMapping =
                PropertyMapping<TvDbSource, Metadata>.Create(t => t.TargetValueB,
                    (s, t) => t.TargetValueB = s.TvDbValue);

            var aniDbSource = new AniDbSource();
            var tvDbSource = new TvDbSource();
            var metadata = new Metadata();

            var metadataMapping =
                MetadataMapping<Metadata>.Create<AniDbSource, TvDbSource>(new[] { aniDbMapping },
                    new[] { tvDbMapping });

            metadataMapping.Apply(aniDbSource, tvDbSource, metadata);

            metadata.TargetValueA.Should().Be("AniDb");
            metadata.TargetValueB.Should().Be("TvDb");
        }
    }
}