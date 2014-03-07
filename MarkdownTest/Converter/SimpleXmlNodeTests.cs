using System.IO;
using System.Xml;
using Markdown.Converter;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MarkdownTest.Converter
{
    [TestClass]
    public class SimpleXmlNodeTests : MarkdownTest
    {
        [TestMethod]
        public void Transfers_Attributes()
        {
            const string html = @"<a href=""http://www.example.com"" class=""magic"" id=""house"">Hello World</a>";

            using (StringReader strReader = new StringReader(html))
            {
                using (XmlReader reader = XmlReader.Create(strReader))
                {
                    reader.Read();
                    SimpleXmlNode node = new SimpleXmlNode(reader);

                    Assert.AreEqual("a", node.Name);
                    Assert.AreEqual(3, node.Count);
                    Assert.IsTrue(node.ContainsKey("href"));
                    Assert.IsTrue(node.ContainsKey("class"));
                    Assert.IsTrue(node.ContainsKey("id"));
                    Assert.AreEqual("http://www.example.com", node["href"]);
                    Assert.AreEqual("magic", node["class"]);
                    Assert.AreEqual("house", node["id"]);

                    reader.Read();
                    node = new SimpleXmlNode(reader);
                    Assert.AreEqual("Hello World", node.Value);
                }
            }
        }
    }
}