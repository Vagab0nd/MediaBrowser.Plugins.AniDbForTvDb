using System.Threading.Tasks;
using LanguageExt;

namespace MediaBrowser.Plugins.AniMetadata.JsonApi
{
    internal interface IJsonConnection
    {
        Task<Either<FailedRequest, Response<TResponseData>>> PostAsync<TResponseData>(
            IPostRequest<TResponseData> request, Option<string> token);

        Task<Either<FailedRequest, Response<TResponseData>>> GetAsync<TResponseData>(IGetRequest<TResponseData> request,
            Option<string> token);
    }
}