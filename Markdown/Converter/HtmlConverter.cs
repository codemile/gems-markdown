using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using HtmlAgilityPack;
using Markdown.Documents;

namespace Markdown.Converter
{
    /// <summary>
    /// Class uses to convert HTML to Markdown.
    /// </summary>
    public class HtmlConverter
    {
        /// <summary>
        /// Attempts to remove unwanted characters and
        /// normalize whitespace.
        /// </summary>
        private static string Clean(string pStr)
        {
            pStr = pStr.Replace("\n", " "); // new lines
            pStr = pStr.Replace("\r", " "); // returns
            pStr = pStr.Replace("\t", " "); // tabs
            pStr = Regex.Replace(pStr, @"[\s]{2,}", " "); // remove 2 or more spaces
            return pStr.Trim();
        }

        /// <summary>
        /// Closes all writers.
        /// </summary>
        private static void CloseWriters(IEnumerable<iDocumentWriter> pWriters)
        {
            foreach (iDocumentWriter writer in pWriters)
            {
                writer.Close();
            }
        }

        /// <summary>
        /// Creates a map between the index of the writer and it's exclusion list.
        /// </summary>
        private static Dictionary<int, List<string>> ExclusionMap(IList<iDocumentWriter> pWriters)
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            for (int i = 0, c = pWriters.Count; i < c; i++)
            {
                map.Add(i, new List<string>(pWriters[i].ExcludeTags()));
            }
            return map;
        }

        /// <summary>
        /// Opens all writers.
        /// </summary>
        private static void OpenWriters(IEnumerable<iDocumentWriter> pWriters)
        {
            foreach (iDocumentWriter writer in pWriters)
            {
                writer.Open();
            }
        }

        /// <summary>
        /// Sends the data from the XML node to the writer for storage.
        /// </summary>
        private static void WriteNode(XmlReader pReader, iDocumentWriter pWriter)
        {
            switch (pReader.NodeType)
            {
                case XmlNodeType.Text:
                    pWriter.WriteText(new SimpleXmlNode(pReader));
                    break;
                case XmlNodeType.Element:
                    pWriter.WriteOpen(new SimpleXmlNode(pReader));
                    break;
                case XmlNodeType.EndElement:
                    pWriter.WriteClose(new SimpleXmlNode(pReader));
                    break;
            }
        }

        /// <summary>
        /// Reads all the XML nodes in the stream, and dispatches to all the
        /// writers.
        /// </summary>
        private static void ReadNodes(XmlReader pReader, IList<iDocumentWriter> pWriters)
        {
            Dictionary<int, List<string>> exclude = ExclusionMap(pWriters);

            while (pReader.Read())
            {
                string name = pReader.Name.ToLower().Trim();

                for (int i = 0, c = pWriters.Count; i < c; i++)
                {
                    if (!exclude[i].Contains(name))
                    {
                        WriteNode(pReader, pWriters[i]);
                    }
                }
            }
        }

        /// <summary>
        /// Ensures that the text is formatted as HTML and
        /// no extra whitespaces.
        /// </summary>
        public static string CleanHTML(string pHTML)
        {
            pHTML = pHTML.Trim();
            if (isHTML(pHTML))
            {
                pHTML = "<div>" + pHTML.Trim() + "</div>";
            }
            else
            {
                StringBuilder str = new StringBuilder();
                foreach (string line in pHTML.Split(new[] {'\n'}))
                {
                    string tmp = Clean(line.Trim());
                    if (tmp.Length <= 0)
                    {
                        continue;
                    }
                    str.Append("<p>");
                    str.Append(tmp);
                    str.Append("</p>");
                }
                pHTML = str.ToString();
            }
            return pHTML;
        }

        /// <summary>
        /// Creates an XmlTextReader from a HTML source.
        /// </summary>
        public static XmlReader HTMLToXmlReader(string pHTML)
        {
            // convert to valid XML
            HtmlDocument doc = new HtmlDocument {OptionOutputAsXml = true};
            doc.LoadHtml(pHTML);
            using (StringWriter xml = new StringWriter())
            {
                doc.Save(xml);
                return XmlReader.Create(new StringReader(xml.ToString()));
            }
        }

        /// <summary>
        /// Doesn't check if it's valid HTML. Just if it starts with
        /// a &lt; character.
        /// </summary>
        public static bool isHTML(string pHTML)
        {
            return (pHTML.Length > 1 && pHTML[0] == '<');
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public static void Convert(IList<iDocumentWriter> pWriters, string pHTML)
        {
            OpenWriters(pWriters);

            pHTML = CleanHTML(pHTML);
            using (XmlReader reader = HTMLToXmlReader(pHTML))
            {
                ReadNodes(reader, pWriters);
            }

            CloseWriters(pWriters);
        }
    }
}