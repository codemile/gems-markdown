using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Markdown.Converter;
using Markdown.Writers;

namespace Markdown.Documents
{
    /// <summary>
    /// Converts a document into fragments.
    /// </summary>
    public class DocumentFragmentWriter : iDocumentWriter
    {
        /// <summary>
        /// The body of the document.
        /// </summary>
        private StringBuilder _body;

        /// <summary>
        /// The current paragraph.
        /// </summary>
        private StringBuilder _paragraph;

        /// <summary>
        /// The default title for new documents.
        /// </summary>
        private string _defaultTitle { get; set; }

        /// <summary>
        /// The min number of characters for a sentence fragment to be created.
        /// </summary>
        private int _minSentenceLength { get; set; }

        /// <summary>
        /// The document fragments.
        /// </summary>
        public DocumentFactory Document { get; private set; }

        /// <summary>
        /// Adds the body fragment to the document.
        /// </summary>
        private void CloseDocument()
        {
            CloseParagraph();
            Document.Add(new Fragment(Fragment.BODY, _body.ToString()));
            _body.Clear();

            if (Document.Count(Fragment.TITLE) == 0)
            {
                Document.Add(new Fragment(Fragment.TITLE, _defaultTitle));
            }
        }

        /// <summary>
        /// Closes the current paragraph and saves the fragments.
        /// </summary>
        private void CloseParagraph()
        {
            string str = _paragraph.ToString().Trim();
            if (str.Length > 0)
            {
                Document.Add(new Fragment(Fragment.PARAGRAPH, str));
                foreach (string sentence in SplitSentences(str, _minSentenceLength))
                {
                    Document.Add(new Fragment(Fragment.SENTENCE, sentence));
                }
            }
            _paragraph.Clear();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        private DocumentFragmentWriter(int pMinSentenceLength = 3)
        {
            _minSentenceLength = pMinSentenceLength;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public DocumentFragmentWriter(string pDefaultTitle, int pMinSentenceLength = 3)
            : this(pMinSentenceLength)
        {
            _defaultTitle = pDefaultTitle;
        }

        /// <summary>
        /// Use the blacklist for Markdown, but exclude a few tags we need.
        /// </summary>
        public IEnumerable<string> ExcludeTags()
        {
            List<string> exclude = new List<string>(MarkdownWriter.TagBlacklist);
            exclude.Remove("head");
            exclude.Remove("meta");
            return exclude.ToArray();
        }

        /// <summary>
        /// Prepare for a new document.
        /// </summary>
        public void Open()
        {
            Document = new DocumentFactory();
            _body = new StringBuilder();
            _paragraph = new StringBuilder();
        }

        /// <summary>
        /// </summary>
        public void WriteOpen(SimpleXmlNode pNode)
        {
        }

        /// <summary>
        /// </summary>
        public void WriteText(SimpleXmlNode pNode)
        {
            if (pNode.Name != "meta" && pNode.Name != "head")
            {
                _body.Append(pNode.Value);
                _paragraph.Append(pNode.Value);
            }
        }

        /// <summary>
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
                case "hr":
                    CloseParagraph();
                    break;
            }
        }

        /// <summary>
        /// </summary>
        public void Close()
        {
            CloseDocument();
        }

        /// <summary>
        /// Splits block of text into sentences.
        /// </summary>
        public static string[] SplitSentences(string pText, int pMinLength)
        {
            const string pattern = @"(?<sentence>[^\.\?!:;]*[\.\?!:;]?)";
            return Regex.Matches(pText, pattern)
                .Cast<Match>()
                .Select(pMatch=>pMatch.Groups[Fragment.SENTENCE].Value)
                .Where(pSentence=>pSentence.Length > pMinLength)
                .ToArray();
        }
    }
}