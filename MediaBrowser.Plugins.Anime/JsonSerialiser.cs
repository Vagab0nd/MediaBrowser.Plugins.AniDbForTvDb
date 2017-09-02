using Newtonsoft.Json;

namespace MediaBrowser.Plugins.AniMetadata
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

        public string Serialise<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}