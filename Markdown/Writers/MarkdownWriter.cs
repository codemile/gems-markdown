using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Markdown.Converter;
using Markdown.Documents;

namespace Markdown.Writers
{
    /// <summary>
    /// Converts a document to Markdown notation
    /// </summary>
    public class MarkdownWriter : iDocumentWriter
    {
        /// <summary>
        /// A mapping between a closing tag and it's replacement.
        /// </summary>
        private static readonly Dictionary<string, string> _closeMap;

        /// <summary>
        /// A mapping between an opening tag and it's replacement.
        /// </summary>
        private static readonly Dictionary<string, string> _openMap;

        /// <summary>
        /// A mapping of strings to be replace in any text body.
        /// </summary>
        private static readonly Dictionary<string, string> _replaceMap;

        /// <summary>
        /// Used by this writer and the TextWriter
        /// </summary>
        public static readonly string[] TagBlacklist =
        {
            "applet",
            "head",
            "base",
            "basefont",
            "button",
            "canvas",
            "command",
            "datalist",
            "embed",
            "iframe",
            "input",
            "select",
            "form",
            "label",
            "map",
            "link",
            "menu",
            "meta",
            "noscript",
            "object",
            "script",
            "style",
            "textarea",
            "video"
        };

        /// <summary>
        /// The converted MarkDown
        /// </summary>
        private StringBuilder _buffer;

        /// <summary>
        /// A reference stack.
        /// </summary>
        private Stack<string> _referenceStack;

        /// <summary>
        /// A list of links
        /// </summary>
        public List<string> References { get; private set; }

        /// <summary>
        /// The converted text
        /// </summary>
        public string text
        {
            get { return _buffer.ToString(); }
        }

        /// <summary>
        /// Escapes text that will be inside a markdown reference title.
        /// </summary>
        private static string EscapeText(string pText)
        {
            return _replaceMap.Aggregate(pText, (pCurrent, pAir)=>pCurrent.Replace(pAir.Key, pAir.Value));
        }

        /// <summary>
        /// A simple way to convert HTML to Markdown.
        /// </summary>
        private static string MapTag(string pTag, IDictionary<string, string> pMapping)
        {
            return pMapping.ContainsKey(pTag) ? pMapping[pTag] : "";
        }

        /// <summary>
        /// Returns the reference index for the current reference.
        /// </summary>
        /// <returns></returns>
        private int PopReference()
        {
            string url = _referenceStack.Pop();
            return References.IndexOf(url) + 1;
        }

        /// <summary>
        /// Adds a URL as a Markdown reference, and set's it as the current reference.
        /// </summary>
        /// <param name="pUrl"></param>
        private int PushReference(string pUrl)
        {
            if (!References.Contains(pUrl))
            {
                References.Add(pUrl);
            }

            _referenceStack.Push(pUrl);
            return References.IndexOf(pUrl) + 1;
        }

        /// <summary>
        /// Static constructor
        /// </summary>
        static MarkdownWriter()
        {
            _openMap = new Dictionary<string, string>
                       {
                           {"p", ""},
                           {"italic", "*"},
                           {"strong", "**"},
                           {"b", "**"},
                           {"h1", "\n\n#"},
                           {"h2", "\n\n##"},
                           {"h3", "\n\n###"},
                           {"h4", "\n\n###"},
                           {"h5", "\n\n###"},
                           {"h6", "\n\n###"},
                           {"h7", "\n\n###"},
                           {"h8", "\n\n###"},
                           {"h9", "\n\n###"},
                           {"blockquote", ">"},
                           {"li", "\n- "},
                           {"a", "["},
                           {"img", "!["}
                       };

            _closeMap = new Dictionary<string, string>
                        {
                            {"p", "\n\n"},
                            {"br", "\n\n"},
                            {"italic", "*"},
                            {"strong", "**"},
                            {"b", "**"},
                            {"h1", "#\n\n"},
                            {"h2", "##\n\n"},
                            {"h3", "###\n\n"},
                            {"h4", "###\n\n"},
                            {"h5", "###\n\n"},
                            {"h6", "###\n\n"},
                            {"h7", "###\n\n"},
                            {"h8", "###\n\n"},
                            {"h9", "###\n\n"},
                            {"hr", "\n---\n\n"},
                            {"li", "\n"},
                            {"a", "]"}
                        };

            _replaceMap = new Dictionary<string, string>
                          {
                              {"*", @"\*"},
                              {"[", @"\["},
                              {"]", @"\]"},
                              {"&#8216;", "'"},
                              {"&#8217;", "'"},
                              {"&#8212;", "--"},
                              {"&#8220;", "\""},
                              {"&#8221;", "\""},
                              {"#", @"\#"}
                          };
        }

        /// <summary>
        /// Prepare for new document.
        /// </summary>
        public void Open()
        {
            _buffer = new StringBuilder();
            References = new List<string>();
            _referenceStack = new Stack<string>();
        }

        /// <summary>
        /// Convert start tags.
        /// </summary>
        public void WriteOpen(SimpleXmlNode pNode)
        {
            _buffer.Append(MapTag(pNode.Name, _openMap));
            if (pNode.Name == "a" && pNode.Has("href"))
            {
                PushReference(pNode["href"]);
            }
            else if (pNode.Name == "img" && pNode.Has("src"))
            {
                int num = PushReference(pNode["src"]);
                string title = pNode["alt"];
                title = EscapeText(title);
                _buffer.Append(title);
                _buffer.Append(string.Format("][{0}]", num));
                PopReference();
            }
        }

        /// <summary>
        /// Write the contents of a tag.
        /// </summary>
        public void WriteText(SimpleXmlNode pNode)
        {
            string str = pNode.Value;
            str = EscapeText(str);
            _buffer.Append(str);
        }

        /// <summary>
        /// Convert end tags.
        /// </summary>
        public void WriteClose(SimpleXmlNode pNode)
        {
            _buffer.Append(MapTag(pNode.Name, _closeMap));
            if (pNode.Name == "a")
            {
                _buffer.Append(string.Format("[{0}]", PopReference()));
            }
        }

        /// <summary>
        /// Write out the references.
        /// </summary>
        public void Close()
        {
            _buffer.Append("\n");

            if (References.Count > 0)
            {
                _buffer.Append("\n");
                for (int i = 0; i < References.Count; i++)
                {
                    _buffer.Append(string.Format("[{0}]: {1}\n", i + 1, References[i]));
                }
            }

            string[] lines = _buffer.ToString().Split(new[] {'\n'});
            _buffer.Clear();
            string previous = "";
            foreach (string line in lines)
            {
                string str = line.Trim();
                if (str.Length > 0)
                {
                    // insert new lines after images that start paragraphs
                    str = Regex.Replace(str, @"^(?<image>!\[.*?\]\[\d\d?\])(?<paragraph>.+)$",
                        "${image}\n\n${paragraph}");

                    bool _ref = Regex.IsMatch(str, @"^\[\d\d?\]:");
                    if (_ref)
                    {
                        _buffer.Append("  ");
                    }
                    if (previous.StartsWith("- ") && !str.StartsWith("- "))
                    {
                        _buffer.Append("\n");
                    }
                    _buffer.Append(str);
                    _buffer.Append("\n");
                    if (!str.StartsWith("- ") && !_ref)
                    {
                        _buffer.Append("\n");
                    }
                    previous = str;
                }
            }
        }

        /// <summary>
        /// HTML tags in this list will be ignored.
        /// </summary>
        public IEnumerable<string> ExcludeTags()
        {
            return TagBlacklist;
        }
    }
}