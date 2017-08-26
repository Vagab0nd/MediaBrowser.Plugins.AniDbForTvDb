using System.IO;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using MediaBrowser.Common.Net;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Plugins.Anime.TvDb;
using MediaBrowser.Plugins.Anime.TvDb.Requests;
using NSubstitute;
using NUnit.Framework;

namespace MediaBrowser.Plugins.Anime.Tests
{
    [TestFixture]
    public class TvDbConnectionTests
    {
        private Stream AsStream(string value)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(value);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        [Test]
        public async Task PostRequest_SuccessfulRequest_ReturnsResponse()
        {
            var httpClient = Substitute.For<IHttpClient>();
            httpClient.Post(Arg.Is<HttpRequestOptions>(o => o.AcceptHeader == "application/json" &&
                    o.Url == "api.thetvdb.com/login" &&
                    o.RequestContent == "{\"apikey\": \"E32490FAD276FF5E\"}" &&
                    o.RequestContentType == "application/json"))
                .Returns(Task.FromResult(new HttpResponseInfo
                {
                    Content = AsStream(
                        "{\"token\": \"eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJleHAiOjE1MDM4MjQwNTUsImlkIjoiTWVkaWFCcm93c2VyLlBsdWdpbnMuQW5pRGJGb3JUdkRiIiwib3JpZ19pYXQiOjE1MDM3Mzc2NTV9.jEVPlHoFFURb3lZU9Svis42YXwDN5GEI-LdZhhjFaRm26XV6DPahm68HTYmL9koMqlIwfGR5a-m4pULFok7B0OCiZPAQOOHlaNxqYEBleSG-saz_Bj3A3mq9ht8pj-xc7pMFb4mR2X6-zL6xoLO1A0h_r4oMAQCkCk8NApDdIdqyCi9nV0EeICfEU1AM84wVV0i-jxRDXaq3TLQynPeLhdefXx8sV0dye7cZo9bebfk18soE8lnc0QkBApv3RcqfoFKxyxAOTKOhHfMGZlB7NSG_duTWciiyFZXlIND6GP7zKScaes3fNu8tbpLAOiNQAyK-o-jq-5cI0y69zR2dBA\"}"),
                    StatusCode = HttpStatusCode.OK
                }));

            var jsonSerialiser = Substitute.For<IJsonSerializer>();

            var request = new LoginRequest("ApiKey");

            jsonSerialiser.SerializeToString(request.Data).Returns("{\"apikey\": \"E32490FAD276FF5E\"}");
            jsonSerialiser.DeserializeFromStream<LoginRequest.Response>(null)
                .ReturnsForAnyArgs(new LoginRequest.Response("Token"));

            var connection = new TvDbConnection(httpClient, jsonSerialiser);

            var response = await connection.PostAsync(request);

            response.ResultType().Should().Be(typeof(Response<LoginRequest.Response>));

            response.Match(
                r => r.Data.Token.Should().Be("Token"), 
                x => { });
        }

        [Test]
        public async Task PostRequest_FailedRequest_ReturnsStatusCodeAndResponseContent()
        {
            var httpClient = Substitute.For<IHttpClient>();
            httpClient.Post(null)
                .ReturnsForAnyArgs(Task.FromResult(new HttpResponseInfo
                {
                    Content = AsStream("{\"Error\": \"Not Authorized\"}"),
                    StatusCode = HttpStatusCode.Unauthorized
                }));

            var jsonSerialiser = Substitute.For<IJsonSerializer>();

            var request = new LoginRequest("ApiKey");

            jsonSerialiser.SerializeToString(request.Data).Returns("{\"apikey\": \"E32490FAD276FF5E\"}");

            var connection = new TvDbConnection(httpClient, jsonSerialiser);

            var response = await connection.PostAsync(request);

            response.ResultType().Should().Be(typeof(FailedRequest));

            response.Match(
                x => { }, 
                fr =>
                {
                    fr.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
                    fr.ResponseContent.Should().Be("{\"Error\": \"Not Authorized\"}");
                });
        }
    }
}