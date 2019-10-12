using System;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using LanguageExt;
using MediaBrowser.Common.Net;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.AniMetadata.JsonApi;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;
using MediaBrowser.Plugins.AniMetadata.TvDb.Requests;
using NSubstitute;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests
{
    using Infrastructure;

    [TestFixture]
    public class JsonConnectionTests
    {
        [Test]
        public async Task GetRequest_FailedRequest_ReturnsStatusCodeAndResponseContent()
        {
            var httpClient = Substitute.For<IHttpClient>();
            httpClient.GetResponse(Arg.Is<HttpRequestOptions>(o => o.AcceptHeader == "application/json" &&
                    o.Url == "https://api.thetvdb.com/series/122/episodes?page=1" &&
                    o.RequestContent.ToString() == null &&
                    o.RequestContentType == null))
                .ReturnsForAnyArgs(Task.FromResult(new HttpResponseInfo
                {
                    Content = Streams.ToStream("{\"Error\": \"Not Authorized\"}"),
                    StatusCode = HttpStatusCode.Unauthorized
                }));

            var jsonSerialiser = Substitute.For<ICustomJsonSerialiser>();

            var request = new GetEpisodesRequest(122, 1);

            var connection = new JsonConnection(httpClient, jsonSerialiser, Substitute.For<ILogManager>());

            var response = await connection.GetAsync(request, Option<string>.None);

            response.IsLeft.Should().BeTrue();

            response.IfLeft(fr =>
                {
                    ((object)fr.StatusCode).Should().Be(HttpStatusCode.Unauthorized);
                    fr.ResponseContent.Should().Be("{\"Error\": \"Not Authorized\"}");
                });
        }

        [Test]
        public async Task GetRequest_SuccessfulRequest_ReturnsResponse()
        {
            var httpClient = Substitute.For<IHttpClient>();
            httpClient.GetResponse(Arg.Is<HttpRequestOptions>(o => o.AcceptHeader == "application/json" &&
                    o.Url == "https://api.thetvdb.com/series/122/episodes?page=1" &&
                    o.RequestContent.ToString() == null &&
                    o.RequestContentType == null))
                .Returns(Task.FromResult(new HttpResponseInfo
                {
                    Content = Streams.ToStream(
                        @"{
  string.Emptydatastring.Empty: [
    {
      string.EmptyabsoluteNumberstring.Empty: 1,
      string.EmptyairedEpisodeNumberstring.Empty: 2,
      string.EmptyairedSeasonstring.Empty: 3,
      string.EmptydvdEpisodeNumberstring.Empty: 4,
      string.EmptydvdSeasonstring.Empty: 5,
      string.EmptyepisodeNamestring.Empty: string.EmptyEpisodeName1string.Empty,
      string.EmptyfirstAiredstring.Empty: string.Empty01/01/2017string.Empty,
      string.Emptyidstring.Empty: 6,
      string.EmptylastUpdatedstring.Empty: 7,
      string.Emptyoverviewstring.Empty: string.EmptyEpisodeOverview1string.Empty
    },
    {
      string.EmptyabsoluteNumberstring.Empty: 8,
      string.EmptyairedEpisodeNumberstring.Empty: 9,
      string.EmptyairedSeasonstring.Empty: 10,
      string.EmptydvdEpisodeNumberstring.Empty: 11,
      string.EmptydvdSeasonstring.Empty: 12,
      string.EmptyepisodeNamestring.Empty: string.EmptyEpisodeName2string.Empty,
      string.EmptyfirstAiredstring.Empty: string.Empty01/01/2015string.Empty,
      string.Emptyidstring.Empty: 13,
      string.EmptylastUpdatedstring.Empty: 14,
      string.Emptyoverviewstring.Empty: string.EmptyEpisodeOverview2string.Empty
    }
  ],
  string.Emptyerrorsstring.Empty: {
    string.EmptyinvalidFiltersstring.Empty: [
      string.Emptystringstring.Empty
    ],
    string.EmptyinvalidLanguagestring.Empty: string.Emptystringstring.Empty,
    string.EmptyinvalidQueryParamsstring.Empty: [
      string.Emptystringstring.Empty
    ]
  },
  string.Emptylinksstring.Empty: {
    string.Emptyfirststring.Empty: 1,
    string.Emptylaststring.Empty: 2,
    string.Emptynextstring.Empty: 3,
    string.Emptypreviousstring.Empty: 4
  }
}"),
                    StatusCode = HttpStatusCode.OK
                }));

            var jsonSerialiser = Substitute.For<ICustomJsonSerialiser>();

            var request = new GetEpisodesRequest(122, 1);

            jsonSerialiser.Deserialise<GetEpisodesRequest.Response>(null)
                .ReturnsForAnyArgs(new GetEpisodesRequest.Response(new[]
                {
                    new TvDbEpisodeSummaryData(6, "EpisodeName1", 1L, 2, 3, 7, new DateTime(2017, 1, 2, 3, 4, 5), "Overview"),
                    new TvDbEpisodeSummaryData(13, "EpisodeName2", 8L, 9, 10, 17, new DateTime(2017, 1, 2, 3, 4, 5), "Overview")
                }, new GetEpisodesRequest.PageLinks(1, 2, 3, 4)));

            var connection = new JsonConnection(httpClient, jsonSerialiser, Substitute.For<ILogManager>());

            var response = await connection.GetAsync(request, Option<string>.None);

            response.IsRight.Should().BeTrue();

            response.IfRight(r => r.Data.Data.Should().HaveCount(2));
        }

        [Test]
        public async Task PostRequest_FailedRequest_ReturnsStatusCodeAndResponseContent()
        {
            var httpClient = Substitute.For<IHttpClient>();
            httpClient.Post(null)
                .ReturnsForAnyArgs(Task.FromResult(new HttpResponseInfo
                {
                    Content = Streams.ToStream("{\"Error\": \"Not Authorized\"}"),
                    StatusCode = HttpStatusCode.Unauthorized
                }));

            var jsonSerialiser = Substitute.For<ICustomJsonSerialiser>();

            var request = new LoginRequest("ApiKey");

            jsonSerialiser.Serialise(request.Data).Returns("{\"apikey\": \"E32490FAD276FF5E\"}");

            var connection = new JsonConnection(httpClient, jsonSerialiser, Substitute.For<ILogManager>());

            var response = await connection.PostAsync(request, Option<string>.None);

            response.IsLeft.Should().BeTrue();

            response.IfLeft(fr =>
                {
                    ((object)fr.StatusCode).Should().Be(HttpStatusCode.Unauthorized);
                    fr.ResponseContent.Should().Be("{\"Error\": \"Not Authorized\"}");
                });
        }

        [Test]
        public async Task PostRequest_SuccessfulRequest_ReturnsResponse()
        {
            var httpClient = Substitute.For<IHttpClient>();
            httpClient.Post(Arg.Is<HttpRequestOptions>(o => o.AcceptHeader == "application/json" &&
                    o.Url == "https://api.thetvdb.com/login" &&
                    o.RequestContent.ToString() == "{\"apikey\": \"E32490FAD276FF5E\"}" &&
                    o.RequestContentType == "application/json"))
                .Returns(Task.FromResult(new HttpResponseInfo
                {
                    Content = Streams.ToStream(
                        "{\"token\": \"eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJleHAiOjE1MDM4MjQwNTUsImlkIjoiTWVkaWFCcm93c2VyLlBsdWdpbnMuQW5pRGJGb3JUdkRiIiwib3JpZ19pYXQiOjE1MDM3Mzc2NTV9.jEVPlHoFFURb3lZU9Svis42YXwDN5GEI-LdZhhjFaRm26XV6DPahm68HTYmL9koMqlIwfGR5a-m4pULFok7B0OCiZPAQOOHlaNxqYEBleSG-saz_Bj3A3mq9ht8pj-xc7pMFb4mR2X6-zL6xoLO1A0h_r4oMAQCkCk8NApDdIdqyCi9nV0EeICfEU1AM84wVV0i-jxRDXaq3TLQynPeLhdefXx8sV0dye7cZo9bebfk18soE8lnc0QkBApv3RcqfoFKxyxAOTKOhHfMGZlB7NSG_duTWciiyFZXlIND6GP7zKScaes3fNu8tbpLAOiNQAyK-o-jq-5cI0y69zR2dBA\"}"),
                    StatusCode = HttpStatusCode.OK
                }));

            var jsonSerialiser = Substitute.For<ICustomJsonSerialiser>();

            var request = new LoginRequest("ApiKey");

            jsonSerialiser.Serialise(request.Data).Returns("{\"apikey\": \"E32490FAD276FF5E\"}");
            jsonSerialiser.Deserialise<LoginRequest.Response>(null)
                .ReturnsForAnyArgs(new LoginRequest.Response("Token"));

            var connection = new JsonConnection(httpClient, jsonSerialiser, Substitute.For<ILogManager>());

            var response = await connection.PostAsync(request, Option<string>.None);

            response.IsRight.Should().BeTrue();

            response.IfRight(r => r.Data.Token.Should().Be("Token"));
        }
    }
}