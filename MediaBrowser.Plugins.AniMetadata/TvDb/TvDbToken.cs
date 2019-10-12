using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.AniMetadata.JsonApi;
using MediaBrowser.Plugins.AniMetadata.TvDb.Requests;

namespace MediaBrowser.Plugins.AniMetadata.TvDb
{
    internal class TvDbToken
    {
        private readonly string apiKey;
        private readonly ILogger log;
        private readonly IJsonConnection jsonConnection;
        private bool hasToken;
        private string token;

        public TvDbToken(IJsonConnection jsonConnection, string apiKey, ILogManager logManager)
        {
            this.jsonConnection = jsonConnection;
            this.apiKey = apiKey;
            this.log = logManager.GetLogger(nameof(TvDbToken));
        }

        public async Task<Option<string>> GetTokenAsync()
        {
            if (this.hasToken)
            {
                this.log.Debug($"Using existing token '{this.token}'");
                return this.token;
            }

            var request = new LoginRequest(this.apiKey);

            var response = await this.jsonConnection.PostAsync(request, Option<string>.None);

            return response.Match(
                r =>
                {
                    this.hasToken = true;
                    this.token = r.Data.Token;

                    this.log.Debug($"Got new token '{this.token}'");
                    return this.token;
                },
                fr =>
                {
                    this.log.Debug("Failed to get a new token");
                    return Option<string>.None;
                });
        }
    }
}