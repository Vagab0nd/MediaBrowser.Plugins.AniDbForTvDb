using MediaBrowser.Common.Net;

namespace MediaBrowser.Plugins.Anime.TvDb
{
    internal class TvDbSession
    {
        private readonly ITvDbConnection _tvDbConnection;

        public TvDbSession(ITvDbConnection tvDbConnection)
        {
            _tvDbConnection = tvDbConnection;
        }


    }
}
