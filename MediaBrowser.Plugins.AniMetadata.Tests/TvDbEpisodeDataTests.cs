using System.Collections.Generic;
using FluentAssertions;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;
using Newtonsoft.Json;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests
{
    [TestFixture]
    public class TvDbEpisodeDataTests
    {
        [SetUp]
        public void Setup()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new OptionJsonConverter() }
            };
        }

        [Test]
        public void Deserialise_NullEpisodeAirDate_SetsToNone()
        {
            var serialiser = new JsonSerialiser();

            var value = serialiser.Deserialise<TvDbEpisodeData>(@"{
                firstAired: null
            }")
                .FirstAired;

            value.IsNone.Should().BeTrue();
        }

        [Test]
        public void Deserialise_EmptyEpisodeAirDate_SetsToNone()
        {
            var serialiser = new JsonSerialiser();

            var value = serialiser.Deserialise<TvDbEpisodeData>(@"{
                firstAired: """"
            }")
                .FirstAired;

            value.IsNone.Should().BeTrue();
        }
    }
}