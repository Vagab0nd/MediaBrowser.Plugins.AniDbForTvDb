using System.Threading.Tasks;
using Functional.Maybe;
using MediaBrowser.Plugins.Anime.TvDb.Requests;

namespace MediaBrowser.Plugins.Anime.TvDb
{
    internal class TvDbToken
    {
        private readonly string _apiKey;
        private readonly ITvDbConnection _tvDbConnection;
        private bool _hasToken;
        private string _token;

        public TvDbToken(ITvDbConnection tvDbConnection, string apiKey)
        {
            _tvDbConnection = tvDbConnection;
            _apiKey = apiKey;
        }

        public async Task<Maybe<string>> GetTokenAsync()
        {
            if (_hasToken)
            {
                return _token.ToMaybe();
            }

            var request = new LoginRequest(_apiKey);

            var response = await _tvDbConnection.PostAsync(request, Maybe<string>.Nothing);

            return response.Match(
                r =>
                {
                    _hasToken = true;
                    _token = r.Data.Token;
                    return _token.ToMaybe();
                },
                fr => Maybe<string>.Nothing);
        }
    }
}