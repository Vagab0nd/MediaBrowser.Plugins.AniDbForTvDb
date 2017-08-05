using System.Xml.Serialization;

namespace MediaBrowser.Plugins.Anime.AniDb.Data
{
    [XmlType(AnonymousType = true)]
    public class ItemTitle
    {
        [XmlAttribute("xml:lang")]
        public string Language { get; set; }

        [XmlAttribute("type")]
        public virtual string Type { get; set; }

        [XmlText]
        public string Title { get; set; }

        public int? Priority
        {
            get
            {
                switch (Type)
                {
                    case "main":
                        return 1;

                    case "official":
                        return 2;

                    case "synonym":
                        return 3;
                }

                return null;
            }
        }
    }
}