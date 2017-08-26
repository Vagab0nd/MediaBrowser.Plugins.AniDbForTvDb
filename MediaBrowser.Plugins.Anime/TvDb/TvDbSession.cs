using MediaBrowser.Common.Net;

namespace MediaBrowser.Plugins.Anime.TvDb
{
    internal class TvDbSession
    {
        private readonly TvDbConnection _tvDbConnection;

        public TvDbSession(TvDbConnection tvDbConnection)
        {
            _tvDbConnection = tvDbConnection;
        }


    }
}
