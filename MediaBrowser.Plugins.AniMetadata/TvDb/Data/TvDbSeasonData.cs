namespace MediaBrowser.Plugins.AniMetadata.TvDb.Data
{
    internal class TvDbSeasonData
    {
        public TvDbSeasonData(int seasonNumber)
        {
            SeasonNumber = seasonNumber;
        }

        public int SeasonNumber { get; }
    }
}