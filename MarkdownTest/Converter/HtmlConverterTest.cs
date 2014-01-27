using System;
using System.Collections.Generic;
using System.Xml;
using Markdown.Converter;
using Markdown.Writers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MarkdownTest.Converter
{
    /// <summary>
    /// This is a test class for HtmlConverterTest and is intended
    /// to contain all HtmlConverterTest Unit Tests
    /// </summary>
    [TestClass]
    public class HtmlConverterTest : MarkdownTest
    {
        [TestMethod]
        public void Clean()
        {
            string html = getResourceAsString("Html.FeedItem.html");
            string clean = HtmlConverter.CleanHTML(html);
            Assert.AreEqual("<div>" + html + "</div>", clean);
            Assert.AreEqual("", HtmlConverter.CleanHTML(""));
            Assert.AreEqual("<p>Hello World</p>", HtmlConverter.CleanHTML("\r\nHello   \t   World\t\t\t\r\n"));
        }

        [TestMethod]
        public void HtmlToXmlReader()
        {
            string html = getResourceAsString("Html.FeedItem.html");
            using (XmlReader xml = HtmlConverter.HTMLToXmlReader(html))
            {
                //xml.Read();
                //xml.Read();
                //Assert.AreEqual("img", xml.Name);
            }
        }

        [TestMethod]
        public void Is_Html()
        {
            Assert.IsFalse(HtmlConverter.isHTML(""));
            Assert.IsFalse(HtmlConverter.isHTML("Hello World"));
            Assert.IsTrue(HtmlConverter.isHTML("<p>Hello World</p>"));
        }

        [TestMethod]
        public void Is_Html_Resource()
        {
            string html = getResourceAsString("Html.FeedItem.html");
            Assert.IsTrue(HtmlConverter.isHTML(html));
        }

        /// <summary>
        /// The converter should not throw an error when converting text that isn't
        /// HTML.
        /// </summary>
        [TestMethod]
        public void MarkdownWriter_Test()
        {
            string html = getResourceAsString("Html.FeedItem.html");
            //string markdown = getResourceAsString("Markdown.FeedItem.txt");

            HtmlConverter converter = new HtmlConverter();

            MarkdownWriter writer = new MarkdownWriter();
            HtmlConverter.Convert(new List<iDocumentWriter> {writer}, html);

            Assert.AreEqual(12, writer.References.Count);
            //Assert.AreEqual(markdown, writer.Text);
        }

        /// <summary>
        /// A test for HtmlConverter Constructor
        /// </summary>
        [TestMethod]
        public void TextWriter_Test()
        {
            string html = getResourceAsString("Html.FeedItem.html");
            HtmlConverter converter = new HtmlConverter();

            TextWriter writer = new TextWriter(false);
            HtmlConverter.Convert(new List<iDocumentWriter> {writer}, html);

            Console.WriteLine(writer.Text);

            Assert.AreEqual(writer.Text,
                "Thanks to a recent stock surge, Microsoft co-founder Bill Gates is once again the world's richest man, a title he hasn't held since 2007, according to Bloomberg. Gates' fortune is now worth $72.1 billion, according to Bloomberg, putting him about $550 million ahead of the former No. 1, Mexico's Carlos Slim. Gates' wealth has jumped 10% this year as Microsoft's shares have risen On Thursday, the company's stock price hit a five-year high. Slim's wealth has fallen $2 billion this year as Mexico's Congress passed a monopoly-busting telecom bill that threatens his company America Movil SAB's commanding position in the market. Read more... More about Bill Gates, Wealth, Bloomberg, Business, and Apps Software");
        }
    }
}