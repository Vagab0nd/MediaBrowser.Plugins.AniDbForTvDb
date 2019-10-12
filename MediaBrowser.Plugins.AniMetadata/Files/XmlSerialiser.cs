using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using MediaBrowser.Model.Logging;

namespace MediaBrowser.Plugins.AniMetadata.Files
{
    public class XmlSerialiser : IXmlSerialiser
    {
        private readonly ILogger log;

        public XmlSerialiser(ILogManager logManager)
        {
            this.log = logManager.GetLogger(nameof(XmlSerialiser));
        }

        public T Deserialise<T>(string xml)
        {
            var serializer = new XmlSerializer(typeof(T));

            using (var reader = new StringReader(xml))
            {
                try
                {
                    return (T)serializer.Deserialize(reader);
                }
                catch (Exception ex)
                {
                    this.log.ErrorException($"Failed to deserialise content: '{xml}'", ex);
                    throw;
                }
            }
        }

        public string Serialise<T>(T obj)
        {
            using (var writer = new StringWriter())
            {
                var serialiser = new XmlSerializer(typeof(T));

                serialiser.Serialize(writer, obj);

                return writer.ToString();
            }
        }

        public void SerialiseToFile<T>(string filePath, T data) where T : class
        {
            using (var writer = new XmlTextWriter(filePath, Encoding.UTF8))
            {
                var serialiser = new XmlSerializer(typeof(T));

                serialiser.Serialize(writer, data);
            }
        }
    }
}