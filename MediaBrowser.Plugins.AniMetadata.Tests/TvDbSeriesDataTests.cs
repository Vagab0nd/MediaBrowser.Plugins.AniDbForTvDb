using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;
using Newtonsoft.Json;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests
{
    [TestFixture]
    public class TvDbSeriesDataTests
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
        public void Deserialise_NullAirsDayOfWeek_SetsToNone()
        {
            var serialiser = new JsonSerialiser();

            var value = serialiser.Deserialise<TvDbSeriesData>(@"{
                airsDayOfWeek: null
            }")
                .AirsDayOfWeek;

            value.IsNone.Should().BeTrue();
        }

        [Test]
        public void Deserialise_EmptyAirsDayOfWeek_SetsToNone()
        {
            var serialiser = new JsonSerialiser();

            var value = serialiser.Deserialise<TvDbSeriesData>(@"{
                airsDayOfWeek: """"
            }")
                .AirsDayOfWeek;

            value.IsNone.Should().BeTrue();
        }
    }
}
