using System;

namespace Markdown.Exceptions
{
    /// <summary>
    /// Exceptions related to markdown processing.
    /// </summary>
    public class MarkdownException : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public MarkdownException(string pMessage)
            : base(pMessage)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pMessage"></param>
        /// <param name="pInnerException"></param>
        public MarkdownException(string pMessage, Exception pInnerException)
            : base(pMessage, pInnerException)
        {
        }
    }
}