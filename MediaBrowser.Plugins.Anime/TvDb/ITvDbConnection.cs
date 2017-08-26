using System.Threading.Tasks;
using MediaBrowser.Plugins.Anime.TvDb.Requests;

namespace MediaBrowser.Plugins.Anime.TvDb
{
    internal interface ITvDbConnection
    {
        Task<RequestResult<TResponseData>> PostAsync<TResponseData>(PostRequest<TResponseData> request);

        Task<RequestResult<TResponseData>> GetAsync<TResponseData>(GetRequest<TResponseData> request);
    }
}