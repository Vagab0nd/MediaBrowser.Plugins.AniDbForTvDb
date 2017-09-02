using System.Threading.Tasks;
using Functional.Maybe;
using MediaBrowser.Plugins.Anime.TvDb.Requests;

namespace MediaBrowser.Plugins.Anime.TvDb
{
    internal interface ITvDbConnection
    {
        Task<RequestResult<TResponseData>> PostAsync<TResponseData>(PostRequest<TResponseData> request,
            Maybe<string> token);

        Task<RequestResult<TResponseData>> GetAsync<TResponseData>(GetRequest<TResponseData> request,
            Maybe<string> token);
    }
}