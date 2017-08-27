using System.IO;

namespace MediaBrowser.Plugins.Anime.Tests.TestHelpers
{
    internal static class StreamUtil
    {
        public static Stream ToStream(string value)
        {
            var stream = new MemoryStream();

            WriteToStream(stream, value);

            stream.Position = 0;

            return stream;
        }

        public static void WriteToStream(Stream stream, string value)
        {
            var writer = new StreamWriter(stream);

            writer.Write(value);
            writer.Flush();
        }

        public static string ReadAll(Stream stream)
        {
            var reader = new StreamReader(stream);

            return reader.ReadToEnd();
        }
    }
}