using System.Collections.Generic;
using FluentAssertions;
using Functional.Maybe;
using MediaBrowser.Plugins.Anime.TvDb.Data;
using MediaBrowser.Plugins.Anime.TvDb.Requests;
using Newtonsoft.Json;
using NUnit.Framework;

namespace MediaBrowser.Plugins.Anime.Tests
{
    [TestFixture]
    public class MaybeJsonConverterTests
    {
        [SetUp]
        public void Setup()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new MaybeJsonConverter() }
            };
            _dataNull = new TvDbEpisodeData(1, "Test", Maybe<long>.Nothing, 1, 2, 2);
            _dataNonNull = new TvDbEpisodeData(1, "Test", 5L.ToMaybe(), 1, 2, 2);
        }

        private TvDbEpisodeData _dataNull;
        private TvDbEpisodeData _dataNonNull;

        [Test]
        public void ComplexMaybeSome_CanDeserialise()
        {
            var serialised = @"
{
  ""links"": {
    ""first"": 1,
    ""last"": 1,
    ""next"": null,
    ""prev"": null
  },
  ""data"": [
    {
      ""absoluteNumber"": 1,
      ""airedEpisodeNumber"": 1,
      ""airedSeason"": 1,
      ""airedSeasonID"": 29182,
      ""dvdEpisodeNumber"": null,
      ""dvdSeason"": null,
      ""episodeName"": ""Celestial Being"",
      ""firstAired"": ""2007-10-06"",
      ""id"": 340368,
      ""language"": {
        ""episodeName"": ""en"",
        ""overview"": ""en""
      },
      ""lastUpdated"": 1496255818,
      ""overview"": ""Celestial Being, a private army dedicated to eradicating war, begins demonstrating the powers of their new \""MS-GUNDAM\"" suits by interrupting the public demonstration of AEU's latest Mobile Suit, the AEU Enact and by protecting the Human Reform League's Space Elevator, \""Tenchu\"" from being attacked by terrorists when their mobile suits had attempted to launch rockets on the \""Tenchu\"", earning a news appearance from various TV news channels where Celestial Being's goals were publicly stated by Aeoria Schenberg.""
    }
  ]
}";

            JsonConvert.DeserializeObject<GetEpisodesRequest.Response>(serialised)
                .ShouldBeEquivalentTo(new GetEpisodesRequest.Response(
                        new[] { new TvDbEpisodeData(340368, "Celestial Being", 1L.ToMaybe(), 1, 1, 1496255818) },
                        new GetEpisodesRequest.PageLinks(1, 1, Maybe<int>.Nothing, Maybe<int>.Nothing)),
                    o => o.Excluding(i =>
                        i.SelectedMemberInfo.Name == "Value" &&
                        i.SelectedMemberInfo.DeclaringType == typeof(Maybe<int>)));
        }

        [Test]
        public void MaybeNone_CanDeserialise()
        {
            var serialised = @"{
		""Id"": 1,
		""EpisodeName"": ""Test"",
		""AbsoluteNumber"": null,
		""AiredEpisodeNumber"": 1,
		""AiredSeason"": 2,
		""LastUpdated"": 2
	}";

            JsonConvert.DeserializeObject<TvDbEpisodeData>(serialised)
                .ShouldBeEquivalentTo(_dataNull, o => o.Excluding(i => i.AbsoluteNumber.Value));
        }

        [Test]
        public void MaybeNone_CanSerialise()
        {
            JsonConvert.SerializeObject(_dataNull)
                .Should()
                .Be(
                    "{\"Id\":1,\"EpisodeName\":\"Test\",\"AbsoluteNumber\":null,\"AiredEpisodeNumber\":1,\"AiredSeason\":2,\"LastUpdated\":2}");
        }

        [Test]
        public void MaybeSome_CanDeserialise()
        {
            var serialised = @"{
		""Id"": 1,
		""EpisodeName"": ""Test"",
		""AbsoluteNumber"": 5,
		""AiredEpisodeNumber"": 1,
		""AiredSeason"": 2,
		""LastUpdated"": 2
	}";

            JsonConvert.DeserializeObject<TvDbEpisodeData>(serialised)
                .ShouldBeEquivalentTo(_dataNonNull);
        }

        [Test]
        public void MaybeSome_CanSerialise()
        {
            JsonConvert.SerializeObject(_dataNonNull)
                .Should()
                .Be(
                    "{\"Id\":1,\"EpisodeName\":\"Test\",\"AbsoluteNumber\":5,\"AiredEpisodeNumber\":1,\"AiredSeason\":2,\"LastUpdated\":2}");
        }
    }
}