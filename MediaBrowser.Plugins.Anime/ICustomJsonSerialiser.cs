namespace MediaBrowser.Plugins.Anime
{
    internal interface ICustomJsonSerialiser
    {
        T Deserialise<T>(string json);

        string Serialise(object obj);
    }
}