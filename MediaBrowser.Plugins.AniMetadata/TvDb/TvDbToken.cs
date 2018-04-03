using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.AniMetadata.JsonApi;
using MediaBrowser.Plugins.AniMetadata.TvDb.Requests;

namespace MediaBrowser.Plugins.AniMetadata.TvDb
{
    internal class TvDbToken
    {
        private readonly string _apiKey;
        private readonly ILogger _log;
        private readonly IJsonConnection _jsonConnection;
        private bool _hasToken;
        private string _token;

        public TvDbToken(IJsonConnection jsonConnection, string apiKey, ILogManager logManager)
        {
            _jsonConnection = jsonConnection;
            _apiKey = apiKey;
            _log = logManager.GetLogger(nameof(TvDbToken));
        }

        public async Task<Option<string>> GetTokenAsync()
        {
            if (_hasToken)
            {
                _log.Debug($"Using existing token '{_token}'");
                return _token;
            }

            var request = new LoginRequest(_apiKey);

            var response = await _jsonConnection.PostAsync(request, Option<string>.None);

            return response.Match(
                r =>
                {
                    _hasToken = true;
                    _token = r.Data.Token;

                    _log.Debug($"Got new token '{_token}'");
                    return _token;
                },
                fr =>
                {
                    _log.Debug("Failed to get a new token");
                    return Option<string>.None;
                });
        }
    }
}