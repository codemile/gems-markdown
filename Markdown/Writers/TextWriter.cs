using System.Collections.Generic;
using System.Text;
using Markdown.Converter;
using Markdown.Documents;

namespace Markdown.Writers
{
    /// <summary>
    /// Stores just the text without any formatting details.
    /// </summary>
    public class TextWriter : iDocumentWriter
    {
        /// <summary>
        /// The memory buffer.
        /// </summary>
        private StringBuilder _buffer;

        /// <summary>
        /// The converted text
        /// </summary>
        public string Text
        {
            get { return _buffer.ToString(); }
        }

        /// <summary>
        /// Add linefeed characters for paragraphs.
        /// </summary>
        private bool _lineFeeds { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public TextWriter(bool pLineFeeds = true)
        {
            _lineFeeds = pLineFeeds;
        }

        /// <summary>
        /// Keep just the text.
        /// </summary>
        public void WriteText(SimpleXmlNode pNode)
        {
            _buffer.Append(pNode.Value);
        }

        /// <summary>
        /// Do nothing
        /// </summary>
        public void WriteOpen(SimpleXmlNode pNode)
        {
        }

        /// <summary>
        /// Add line feeds.
        /// </summary>
        public void WriteClose(SimpleXmlNode pNode)
        {
            switch (pNode.Name)
            {
                case "p":
                case "br":
                case "h1":
                case "h2":
                case "h3":
                case "h4":
                case "li":
                    _buffer.Append(_lineFeeds ? "\n" : " ");
                    break;
                case "hr":
                    _buffer.Append(_lineFeeds ? "\n\n" : " ");
                    break;
            }
        }

        /// <summary>
        /// Uses the same blacklist as Markdown.
        /// </summary>
        public IEnumerable<string> ExcludeTags()
        {
            return MarkdownWriter.TagBlacklist;
        }

        /// <summary>
        /// Prepare for a new document.
        /// </summary>
        public void Open()
        {
            _buffer = new StringBuilder();
        }

        /// <summary>
        /// Nothing
        /// </summary>
        public void Close()
        {
        }
    }
}