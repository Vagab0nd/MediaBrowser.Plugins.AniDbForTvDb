using FluentAssertions;
using LanguageExt;
using LanguageExt.UnsafeValueAccess;
using MediaBrowser.Plugins.AniMetadata.Process;
using NSubstitute;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests.Process
{
    [TestFixture]
    public class SourceDataTests
    {
        [TestFixture]
        public class GetData : SourceDataTests
        {
            [SetUp]
            public void Setup()
            {
                _data = new ChildData();
                _sourceData =
                    new SourceData<ChildData>(Substitute.For<ISource>(), Option<int>.None,
                        Substitute.For<IItemIdentifier>(), _data);
            }

            private ISourceData _sourceData;
            private ChildData _data;

            private class ParentData
            {
            }

            private class ChildData : ParentData
            {
            }

            [Test]
            public void DataCastableToType_ReturnsDataAsType()
            {
                _sourceData.GetData<ParentData>().ValueUnsafe().Should().Be(_data);
            }

            [Test]
            public void DataDoesNotMatchType_ReturnsNone()
            {
                _sourceData.GetData<ISourceData>().IsNone.Should().BeTrue();
            }

            [Test]
            public void DataMatchesType_ReturnsData()
            {
                _sourceData.GetData<ChildData>().ValueUnsafe().Should().Be(_data);
            }
        }

        [TestFixture]
        public class Constructor : SourceDataTests
        {
            [Test]
            public void SetsIdentifier()
            {
                var identifier = Substitute.For<IItemIdentifier>();

                var sourceData = new SourceData<object>(Substitute.For<ISource>(), Option<int>.None, identifier,
                    new object());

                sourceData.Identifier.Should().Be(identifier);
            }

            [Test]
            public void SetsSource()
            {
                var source = Substitute.For<ISource>();

                var sourceData = new SourceData<object>(source, Option<int>.None, Substitute.For<IItemIdentifier>(),
                    new object());

                sourceData.Source.Should().Be(source);
            }
        }
    }
}