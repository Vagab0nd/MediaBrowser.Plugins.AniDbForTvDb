using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Functional.Maybe;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Providers;
using MediaBrowser.Plugins.Anime.AniDb;
using MediaBrowser.Plugins.Anime.AniDb.Seiyuu;

namespace MediaBrowser.Plugins.Anime.Providers.AniDb2
{
    public class AniDbPersonProvider : IRemoteMetadataProvider<Person, PersonLookupInfo>
    {
        private readonly IAniDbClient _aniDbClient;
        private readonly IHttpClient _httpClient;
        private readonly ILogger _log;
        private readonly IRateLimiter _rateLimiter;

        public AniDbPersonProvider(IAniDbClient aniDbClient, IRateLimiters rateLimiters, IHttpClient httpClient,
            ILogManager logManager)
        {
            _rateLimiter = rateLimiters.AniDb;
            _aniDbClient = aniDbClient;
            _httpClient = httpClient;
            _log = logManager.GetLogger(nameof(AniDbPersonProvider));
        }

        public Task<IEnumerable<RemoteSearchResult>> GetSearchResults(PersonLookupInfo searchInfo,
            CancellationToken cancellationToken)
        {
            _log.Debug(
                $"Searching for person name: '{searchInfo.Name}', id: '{searchInfo.ProviderIds.GetOrDefault(ProviderNames.AniDb)}'");

            var result = Enumerable.Empty<RemoteSearchResult>();

            if (!string.IsNullOrWhiteSpace(searchInfo.Name))
            {
                result = _aniDbClient.FindSeiyuu(searchInfo.Name).Select(ToSearchResult);
            }
            else if (searchInfo.ProviderIds.ContainsKey(ProviderNames.AniDb))
            {
                var aniDbPersonIdString = searchInfo.ProviderIds[ProviderNames.AniDb];
                var parser = MaybeFunctionalWrappers.Wrap<string, int>(int.TryParse);

                parser(aniDbPersonIdString).Do(aniDbPersonId =>
                {
                    _aniDbClient.GetSeiyuu(aniDbPersonId).Do(s =>
                        result = new[] { ToSearchResult(s) }
                    );
                });
            }

            _log.Debug($"Found {result.Count()} results");

            return Task.FromResult(result);
        }

        public Task<MetadataResult<Person>> GetMetadata(PersonLookupInfo info, CancellationToken cancellationToken)
        {
            _log.Debug(
                $"Getting metadata for person name: '{info.Name}', id: '{info.ProviderIds.GetOrDefault(ProviderNames.AniDb)}'");

            var result = new MetadataResult<Person>();

            if (info.ProviderIds.ContainsKey(ProviderNames.AniDb))
            {
                var aniDbPersonIdString = info.ProviderIds[ProviderNames.AniDb];
                var parser = MaybeFunctionalWrappers.Wrap<string, int>(int.TryParse);

                parser(aniDbPersonIdString).Do(aniDbPersonId =>
                {
                    _aniDbClient.GetSeiyuu(aniDbPersonId).Match(s =>
                        {
                            result.Item = new Person
                            {
                                Name = s.Name,
                                ImageInfos =
                                    new[]
                                    {
                                        new ItemImageInfo { Type = ImageType.Primary, Path = s.PictureUrl }
                                    },
                                ProviderIds =
                                    new Dictionary<string, string> { { ProviderNames.AniDb, s.Id.ToString() } }
                            };

                            _log.Debug("Found metadata");
                        },
                        () => _log.Debug("Failed to find metadata"));
                });
            }

            return Task.FromResult(result);
        }

        public string Name => "AniDB";

        public async Task<HttpResponseInfo> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            _log.Debug($"Getting image: '{url}'");

            await _rateLimiter.TickAsync().ConfigureAwait(false);

            return await _httpClient.GetResponse(new HttpRequestOptions
            {
                CancellationToken = cancellationToken,
                Url = url,
                ResourcePool = _rateLimiter.Semaphore
            }).ConfigureAwait(false);
        }

        private RemoteSearchResult ToSearchResult(SeiyuuData seiyuuData)
        {
            return new RemoteSearchResult
            {
                Name = seiyuuData.Name,
                SearchProviderName = Name,
                ImageUrl = seiyuuData.PictureUrl,
                ProviderIds = new Dictionary<string, string> { { ProviderNames.AniDb, seiyuuData.Id.ToString() } }
            };
        }
    }
}