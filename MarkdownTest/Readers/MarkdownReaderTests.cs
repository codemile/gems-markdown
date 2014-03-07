using System;
using System.Collections.Generic;
using Markdown.Readers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MarkdownTest.Readers
{
    [TestClass]
    public class MarkdownReaderTests : MarkdownTest
    {
        [TestMethod]
        public void Change_Reference_Url()
        {
            string markdown = getResourceAsString("Markdown.FeedItem.txt");
            MarkdownReader reader = new MarkdownReader(markdown);

            Assert.AreEqual(12, reader.References.Count);
            Assert.AreEqual(3, reader.References[3].ID);
            Assert.AreEqual("http://rack.3.mshcdn.com/assets/feed-tw-df3e816c4e85a109d6e247013aed8d66.jpg",
                reader.References[3].URL);

            reader.References[3].URL = "http://www.thinkingmedia.ca/logo.png";

            Assert.IsTrue(reader.Markdown.Contains("  [3]: http://www.thinkingmedia.ca/logo.png"));
        }

        [TestMethod]
        public void Remove_Empty_Links()
        {
            string before = getResourceAsString("Markdown.EmptyLinksBefore.txt");
            string after = getResourceAsString("Markdown.EmptyLinksAfter.txt");

            MarkdownReader reader = new MarkdownReader(before);

            Assert.AreEqual(2, reader.Sections.Count);
            Assert.AreEqual(3, reader.References.Count);

            reader.RemoveDeadReferences();

            Assert.AreEqual(2, reader.Sections.Count);
            Assert.AreEqual(1, reader.References.Count);

            Assert.AreEqual(after, reader.Markdown);
        }

        [TestMethod]
        public void Remove_Reference()
        {
            string before = getResourceAsString("Markdown.RemoveBefore.txt");
            string after = getResourceAsString("Markdown.RemoveAfter.txt");

            MarkdownReader reader = new MarkdownReader(before);

            Assert.AreEqual(18, reader.References.Count);

            reader.Remove(1);
            reader.Remove(2);
            reader.Remove(3);
            reader.Remove(4);

            Assert.AreEqual(14, reader.References.Count);

            string test = reader.Markdown;
            Assert.AreEqual(test, after);
        }

        [TestMethod]
        public void Section_Heading()
        {
            const string markdown = "###This is a heading###";
            MarkdownReader reader = new MarkdownReader(markdown);

            Assert.AreEqual(1, reader.Sections.Count);
            Assert.AreEqual(MarkdownReader.SectionType.HEADING, reader.Sections[0].Type);
            Assert.AreEqual("This is a heading", reader.Sections[0].Clean);
            Assert.AreEqual(markdown, reader.Sections[0].Markdown);
        }

        [TestMethod]
        public void Section_Quote()
        {
            const string markdown = "> This is a quote.";
            MarkdownReader reader = new MarkdownReader(markdown);

            Assert.AreEqual(1, reader.Sections.Count);
            Assert.AreEqual(MarkdownReader.SectionType.QUOTE, reader.Sections[0].Type);
            Assert.AreEqual("This is a quote.", reader.Sections[0].Clean);
            Assert.AreEqual(markdown, reader.Sections[0].Markdown);
        }

        [TestMethod]
        public void Section_Types()
        {
            string markdown = getResourceAsString("Markdown.generic.txt");
            MarkdownReader reader = new MarkdownReader(markdown);

            Assert.AreEqual(5, reader.Sections.Count);
            Assert.AreEqual(MarkdownReader.SectionType.HEADING, reader.Sections[0].Type);
            Assert.AreEqual(MarkdownReader.SectionType.PARAGRAPH, reader.Sections[1].Type);
            Assert.AreEqual(MarkdownReader.SectionType.BULLET, reader.Sections[2].Type);
            Assert.AreEqual(MarkdownReader.SectionType.QUOTE, reader.Sections[3].Type);
            Assert.AreEqual(MarkdownReader.SectionType.PARAGRAPH, reader.Sections[4].Type);

            Assert.AreEqual("#This is a heading#", reader.Sections[0].Markdown);
            Assert.AreEqual("This is a bullet of things.", reader.Sections[1].Markdown);
            Assert.AreEqual("- First bullet\n- Second bullet\n- Third bullet", reader.Sections[2].Markdown);
            Assert.AreEqual("> This is a bunch of source code or a quote.", reader.Sections[3].Markdown);
            Assert.AreEqual("This is a paragraph with a [cnn][1] inside it.", reader.Sections[4].Markdown);

            Assert.AreEqual("This is a heading", reader.Sections[0].Clean);
            Assert.AreEqual("This is a bullet of things.", reader.Sections[1].Clean);
            Assert.AreEqual("First bullet\nSecond bullet\nThird bullet", reader.Sections[2].Clean);
            Assert.AreEqual("This is a bunch of source code or a quote.", reader.Sections[3].Clean);
            Assert.AreEqual("This is a paragraph with a cnn inside it.", reader.Sections[4].Clean);
        }

        [TestMethod]
        public void Test_Paragraphs()
        {
            MarkdownReader reader = new MarkdownReader("This is a paragraph.\nThis is another.\n\nThis one too.");

            Assert.AreEqual(3, reader.Sections.Count);

            Assert.AreEqual("This is a paragraph.", reader.Sections[0].Clean);
            Assert.AreEqual("This is another.", reader.Sections[1].Clean);
            Assert.AreEqual("This one too.", reader.Sections[2].Clean);
        }

        [TestMethod]
        public void Test_References()
        {
            string markdown = getResourceAsString("Markdown.References.txt");
            MarkdownReader reader = new MarkdownReader(markdown);

            Assert.AreEqual(7, reader.References.Count);

            Assert.AreEqual(
                "http://cdn1.sbnation.com/uploads/chorus_image/image/18267435/drakengard333.0_cinema_720.0.jpg",
                reader.References[1].URL);
            Assert.AreEqual("http://www.jp.square-enix.com/dod3/information/index.html", reader.References[2].URL);
            Assert.AreEqual("http://www.polygon.com/2013/3/12/4096544/square-enix-confirms-drakengard-3",
                reader.References[3].URL);
            Assert.AreEqual("http://www.polygon.com/2013/6/27/4469582/drakengard-3-for-ps3-coming-october-31-in-japan",
                reader.References[4].URL);
            Assert.AreEqual(
                "http://www.polygon.com/2013/7/17/4530966/abot-drakengard-3-and-making-the-music-for-a-very-strange-game",
                reader.References[5].URL);
            Assert.AreEqual("/game/drakengard-3/11053", reader.References[6].URL);
            Assert.AreEqual("http://www.polygon.com/2013/8/22/4647804/drakengard-3-delayed-to-dec-19-in-japan",
                reader.References[7].URL);

            Assert.AreEqual(1, reader.References[1].ID);
            Assert.AreEqual(2, reader.References[2].ID);
            Assert.AreEqual(3, reader.References[3].ID);
            Assert.AreEqual(4, reader.References[4].ID);
            Assert.AreEqual(5, reader.References[5].ID);
            Assert.AreEqual(6, reader.References[6].ID);
            Assert.AreEqual(7, reader.References[7].ID);
        }

        [TestMethod]
        public void Test_RemoveImages()
        {
            string markdown = getResourceAsString("Markdown.References.txt");
            MarkdownReader reader = new MarkdownReader(markdown);

            const string imageRef = "![Square Enix][1]";

            Assert.AreEqual(4, reader.Sections.Count);
            Assert.IsTrue(reader.Markdown.IndexOf(imageRef, StringComparison.Ordinal) != -1);
            Assert.IsTrue(reader.Clean.IndexOf(imageRef, StringComparison.Ordinal) == -1);

            Assert.IsTrue(reader.References[1].Type == MarkdownReader.ReferenceType.IMAGE);
            Assert.AreEqual(1, reader.References[1].Titles.Count);
            Assert.AreEqual("Square Enix", reader.References[1].Titles[0]);
        }

        [TestMethod]
        public void Test_RemoveLinks()
        {
            string markdown = getResourceAsString("Markdown.References.txt");
            MarkdownReader reader = new MarkdownReader(markdown);

            // verify links titles
            Assert.AreEqual(7, reader.References.Count);
            Assert.AreEqual("official website", reader.References[2].Titles[0]);
            Assert.AreEqual("first confirmed by Square Enix", reader.References[3].Titles[0]);
            Assert.AreEqual("date announced in June", reader.References[4].Titles[0]);
            Assert.AreEqual("rehashing Nobuyoshi Sano's work", reader.References[5].Titles[0]);
            Assert.AreEqual("D...", reader.References[6].Titles[0]);
            Assert.AreEqual("Continue reading&hellip;", reader.References[7].Titles[0]);

            // verify paragraphs
            Assert.AreEqual(4, reader.Sections.Count);
            Assert.AreEqual(
                "Square Enix is shifting the release date of  Drakengard 3 in Japan from Oct. 31 to Dec. 19 for the PlayStation 3, according to an update on the company's official website.",
                reader.Sections[0].Clean);
            Assert.AreEqual(
                "Drakengard 3 was first confirmed by Square Enix back in March, with the original release date announced in June. The title is being developed by Access Games,  a development studio touts several core Drakengard and Nier developers, such as producer Takamasa Shiba, director/writer Taro Yokoo, character designer Kimihiko Fujisaka and music composer Keiichi Okabe.",
                reader.Sections[1].Clean);
            Assert.AreEqual(
                "Okabe, who composed the  Nier soundtrack, said that he wanted to avoid both rehashing Nobuyoshi Sano's work &mdash; the composer of the original Drakengard &mdash; and repeating his compositions featured in Nier. D...",
                reader.Sections[2].Clean);
            Assert.AreEqual("Continue reading&hellip;", reader.Sections[3].Clean);
        }

        [TestMethod]
        public void Verify_Images()
        {
            string markdown = getResourceAsString("Markdown.post-2248.txt");
            MarkdownReader reader = new MarkdownReader(markdown);

            List<MarkdownReader.Reference> images = reader.getReferences(MarkdownReader.ReferenceType.IMAGE);

            Assert.AreEqual(28, reader.References.Count);
            Assert.AreEqual(14, images.Count);
        }
    }
}