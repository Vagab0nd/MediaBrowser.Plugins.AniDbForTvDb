using MediaBrowser.Plugins.Anime.AniDb.Data;

namespace MediaBrowser.Plugins.Anime.AniDb
{
    public interface IAniDbFileParser
    {
        AniDbSeries ParseSeriesXml(string seriesXml);

        AniDbTitleList ParseTitleListXml(string titleListXml);

        SeiyuuList ParseSeiyuuListXml(string seiyuuListXml);
    }
}