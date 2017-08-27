using System;
using System.IO;
using MediaBrowser.Model.Serialization;
using Newtonsoft.Json;

namespace MediaBrowser.Plugins.Anime.Tests.TestHelpers
{
    internal class TestJsonSerialiser : IJsonSerializer
    {
        public void SerializeToStream(object obj, Stream stream)
        {
            var stringValue = JsonConvert.SerializeObject(obj);

            StreamUtil.WriteToStream(stream, stringValue);
        }

        public void SerializeToFile(object obj, string file)
        {
            throw new NotImplementedException();
        }

        public object DeserializeFromFile(Type type, string file)
        {
            throw new NotImplementedException();
        }

        public T DeserializeFromFile<T>(string file) where T : class
        {
            throw new NotImplementedException();
        }

        public T DeserializeFromStream<T>(Stream stream)
        {
            return JsonConvert.DeserializeObject<T>(StreamUtil.ReadAll(stream));
        }

        public T DeserializeFromString<T>(string text)
        {
            return JsonConvert.DeserializeObject<T>(text);
        }

        public object DeserializeFromStream(Stream stream, Type type)
        {
            return JsonConvert.DeserializeObject(StreamUtil.ReadAll(stream));
        }

        public object DeserializeFromString(string json, Type type)
        {
            return JsonConvert.DeserializeObject(json, type);
        }

        public string SerializeToString(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}