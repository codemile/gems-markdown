using System.Collections.Generic;
using Markdown.Converter;
using Markdown.Writers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MarkdownTest.Writers
{
    [TestClass]
    public class MarkdownWriterTests : MarkdownTest
    {
        [TestMethod]
        public void Test_Post_2248()
        {
            string html = getResourceAsString("Html.post-2248.txt");
            HtmlConverter converter = new HtmlConverter();

            MarkdownWriter writer = new MarkdownWriter();
            HtmlConverter.Convert(new List<iDocumentWriter> {writer}, html);

            string markdown = getResourceAsString("Markdown.post-2248.txt");
            string text = writer.Text;

            markdown = markdown.Replace("\r", "").Trim();
            text = text.Replace("\r", "").Trim();

            Assert.AreEqual(markdown.Length, text.Length);
            Assert.AreEqual(markdown, text);
        }
    }
}