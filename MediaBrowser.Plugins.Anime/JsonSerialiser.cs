using Newtonsoft.Json;

namespace MediaBrowser.Plugins.Anime
{
    /// <summary>
    ///     Uses NewtonSoft.Json which supports deserialisation via constructors
    /// </summary>
    internal class JsonSerialiser : ICustomJsonSerialiser
    {
        public T Deserialise<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public string Serialise(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}