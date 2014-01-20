using System.Linq;
using Markdown.Converter;
using Markdown.Documents;
using Markdown.Readers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MarkdownTest.Documents
{
    [TestClass]
    public class DocumentFragmentWriterTest : MarkdownTest
    {
        [TestMethod]
        public void Creating_Document()
        {
            string[] lines =
            {
                "Suspected militants armed with rocket-propelled grenades struck two buses carrying security forces and killed the soldiers in the city of Rafah, on the border between Egypt and Gaza, state-run Nile TV reported." +
                "The Sinai Peninsula is a lawless area that was the site of frequent attacks even before Egypt's latest round of turmoil.",
                "In May, for example, seven Egyptian solders were kidnapped and held for six days there, a spokesman for Egypt's armed forces said." +
                "But the attack adds to the persistent tension across the country since the military ousted democratically elected President Mohamed Morsy in a coup."
            };

            DocumentFragmentWriter writer = new DocumentFragmentWriter("Hello World");
            writer.Open();
            foreach (SimpleXmlNode node in lines.Select(pLine=>new SimpleXmlNode("p", pLine)))
            {
                writer.WriteOpen(node);
                writer.WriteText(node);
                writer.WriteClose(node);
            }
            writer.Close();

            iDocument doc = new DocumentReader(writer.Document);
            //Fragment[] titles = doc.getFragments(Fragment.TITLE, ParserCursor.None);
            //Fragment[] body = doc.getFragments(Fragment.BODY, ParserCursor.None);
            Fragment[] paragraphs = doc.getFragments(Fragment.PARAGRAPH, DocumentCursor.None);
            Fragment[] sentences = doc.getFragments(Fragment.SENTENCE, DocumentCursor.None);

            Assert.AreEqual(2, paragraphs.Length);
            Assert.AreEqual(4, sentences.Length);
        }

        [TestMethod]
        public void Default_Title()
        {
            DocumentFragmentWriter writer = new DocumentFragmentWriter("Hello World");
            writer.Open();
            writer.Close();

            iDocument doc = new DocumentReader(writer.Document);
            Fragment[] fragments = doc.getFragments(Fragment.TITLE, DocumentCursor.None);
            Assert.AreEqual(1, fragments.Length);
            Assert.AreEqual(Fragment.TITLE, fragments[0].Type);
            Assert.AreEqual("Hello World", fragments[0].Text);
        }

        [TestMethod]
        public void Split_Missing_Period()
        {
            // last sentence doesn't have period
            const string paragraph =
                "Suspected militants armed with rocket-propelled grenades struck two buses carrying security forces and killed the soldiers in the city of Rafah, on the border between Egypt and Gaza, state-run Nile TV reported." +
                "The Sinai Peninsula is a lawless area that was the site of frequent attacks even before Egypt's latest round of turmoil";
            string[] sentences = DocumentFragmentWriter.SplitSentences(paragraph, 3);
            Assert.AreEqual(2, sentences.Length);
        }

        [TestMethod]
        public void Split_One_Sentence()
        {
            // from a random CNN article
            const string paragraph =
                "Suspected militants armed with rocket-propelled grenades struck two buses carrying security forces and killed the soldiers in the city of Rafah, on the border between Egypt and Gaza, state-run Nile TV reported.";
            string[] sentences = DocumentFragmentWriter.SplitSentences(paragraph, 3);
            Assert.AreEqual(1, sentences.Length);
        }

        [TestMethod]
        public void splitSentences()
        {
            // from a random CNN article
            const string paragraph =
                "Suspected militants armed with rocket-propelled grenades struck two buses carrying security forces and killed the soldiers in the city of Rafah, on the border between Egypt and Gaza, state-run Nile TV reported." +
                "The Sinai Peninsula is a lawless area that was the site of frequent attacks even before Egypt's latest round of turmoil." +
                "In May, for example, seven Egyptian solders were kidnapped and held for six days there, a spokesman for Egypt's armed forces said." +
                "But the attack adds to the persistent tension across the country since the military ousted democratically elected President Mohamed Morsy in a coup.";
            string[] sentences = DocumentFragmentWriter.SplitSentences(paragraph, 3);
            Assert.AreEqual(4, sentences.Length);
        }
    }
}