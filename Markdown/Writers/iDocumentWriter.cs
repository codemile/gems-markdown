using System.Collections.Generic;
using Markdown.Converter;

namespace Markdown.Writers
{
    /// <summary>
    /// Handles writing to memory storage the document as it's converted.
    /// </summary>
    public interface iDocumentWriter
    {
        /// <summary>
        /// Finish the current document.
        /// </summary>
        void Close();

        /// <summary>
        /// A list of HTML tags this writer wants ignored.
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> ExcludeTags();

        /// <summary>
        /// Prepare to start a new document.
        /// </summary>
        void Open();

        /// <summary>
        /// Write the close tag.
        /// </summary>
        void WriteClose(SimpleXmlNode pNode);

        /// <summary>
        /// Write the open tag.
        /// </summary>
        void WriteOpen(SimpleXmlNode pNode);

        /// <summary>
        /// Write the text.
        /// </summary>
        void WriteText(SimpleXmlNode pNode);
    }
}