using System.Threading.Tasks;
using Functional.Maybe;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.Anime.TvDb.Requests;

namespace MediaBrowser.Plugins.Anime.TvDb
{
    internal class TvDbToken
    {
        private readonly string _apiKey;
        private readonly ILogger _log;
        private readonly ITvDbConnection _tvDbConnection;
        private bool _hasToken;
        private string _token;

        public TvDbToken(ITvDbConnection tvDbConnection, string apiKey, ILogManager logManager)
        {
            _tvDbConnection = tvDbConnection;
            _apiKey = apiKey;
            _log = logManager.GetLogger(nameof(TvDbToken));
        }

        public async Task<Maybe<string>> GetTokenAsync()
        {
            if (_hasToken)
            {
                _log.Debug($"Using existing token '{_token}'");
                return _token.ToMaybe();
            }

            var request = new LoginRequest(_apiKey);

            var response = await _tvDbConnection.PostAsync(request, Maybe<string>.Nothing);

            return response.Match(
                r =>
                {
                    _hasToken = true;
                    _token = r.Data.Token;

                    _log.Debug($"Got new token '{_token}'");
                    return _token.ToMaybe();
                },
                fr =>
                {
                    _log.Debug("Failed to get a new token");
                    return Maybe<string>.Nothing;
                });
        }
    }
}