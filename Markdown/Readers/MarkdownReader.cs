using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Markdown.Exceptions;

namespace Markdown.Readers
{
    /// <summary>
    /// Reads markdown text into properties that can be analyzed.
    /// </summary>
    public class MarkdownReader
    {
        /// <summary>
        /// The type of reference.
        /// </summary>
        public enum ReferenceType
        {
            IMAGE,
            LINK,
            YOU_TUBE,
            VIMEO,
            UNKNOWN
        }

        /// <summary>
        /// The different types of sections.
        /// </summary>
        public enum SectionType
        {
            PARAGRAPH,
            HEADING,
            BULLET,
            QUOTE,
            LINE
        }

        private const string _REGEX_IMAGE = @"\!\[(?<alt>([^\[\]]|(\\\[)|(\\\]))*)\]\[{0}\]";
        private const string _REGEX_LINK = @"\[(?<title>([^\[\]]|(\\\[)|(\\\]))*)\]\[{0}\]";
        private const string _REGEX_REFERENCE = @"^\s+?\[(?<id>[\d]{1,2})\]:\s+?(?<url>.*?)$";

        /// <summary>
        /// The cleaned version of the markdown.
        /// </summary>
        public string Clean
        {
            get
            {
                StringBuilder str = new StringBuilder();
                foreach (Section section in Sections)
                {
                    str.Append(section.Clean);
                    str.Append("\n\n");
                }
                return str.ToString();
            }
        }

        /// <summary>
        /// Regenerates the markdown including any changes to references.
        /// </summary>
        public string Markdown
        {
            get
            {
                string str;
                StringBuilder strBldr = new StringBuilder();

                foreach (Section section in Sections)
                {
                    str = section.Markdown.Trim();
                    if (str == "")
                    {
                        continue;
                    }
                    strBldr.Append(str);
                    strBldr.Append("\n\n");
                }

                foreach (Reference reference in References.Values)
                {
                    strBldr.Append(reference);
                    strBldr.Append("\n");
                }

                // end with a linefeed unless blank
                str = strBldr.ToString().Trim();
                if (str != "")
                {
                    str += "\n";
                }

                return str;
            }
        }

        /// <summary>
        /// All the references from the markdown.
        /// </summary>
        public Dictionary<int, Reference> References { get; private set; }

        /// <summary>
        /// Contains a list of all the seconds, which include paragraphs and headings.
        /// </summary>
        public List<Section> Sections { get; private set; }

        /// <summary>
        /// Removes the references list from the bottom of the document.
        /// </summary>
        private static string RemoveFooter(string pMarkdown)
        {
            return Regex.Replace(pMarkdown, _REGEX_REFERENCE, "", RegexOptions.Multiline).Trim();
        }

        /// <summary>
        /// Populates the References property by finding all the references at the footer
        /// of the markdown document.
        /// </summary>
        private void ReadFooter(string pMarkdown)
        {
            foreach (Match match in Regex.Matches(pMarkdown, _REGEX_REFERENCE, RegexOptions.Multiline))
            {
                int id = int.Parse(match.Groups["id"].Value);
                string url = match.Groups["url"].Value.Trim();
                if (References.ContainsKey(id))
                {
                    throw new MarkdownException(string.Format("Reference already exists [{0}]: {1}", id, url));
                }

                // by default all references are links
                Reference _ref = new Reference {ID = id, URL = url, Type = ReferenceType.LINK};

                // search for references used as images
                if (Regex.IsMatch(pMarkdown, string.Format(_REGEX_IMAGE, id)))
                {
                    _ref.Type = ReferenceType.IMAGE;
                }

                References.Add(id, _ref);
            }
        }

        /// <summary>
        /// Reads all the paragraphs.
        /// </summary>
        private void ReadSections(string pMarkdown)
        {
            foreach (string line in pMarkdown.Split(new[] {'\n', '\r'}))
            {
                string str = line.Trim();
                if (str == "")
                {
                    continue;
                }
                if (str == "---")
                {
                    Sections.Add(new Section(SectionType.LINE, str));
                }
                else
                {
                    switch (str[0])
                    {
                        case '#':
                            Sections.Add(new Section(SectionType.HEADING, str));
                            break;
                        case '-':
                            // group bullet lines into a single section
                            if (Sections.Count > 0 && Sections[Sections.Count - 1].Type == SectionType.BULLET)
                            {
                                Sections[Sections.Count - 1] = new Section(
                                    SectionType.BULLET,
                                    Sections[Sections.Count - 1].Markdown + "\n" + str
                                    );
                            }
                            else
                            {
                                Sections.Add(new Section(SectionType.BULLET, str));
                            }
                            break;
                        case '>':
                            Sections.Add(new Section(SectionType.QUOTE, str));
                            break;
                        default:
                            Sections.Add(new Section(SectionType.PARAGRAPH, str));
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public MarkdownReader(string pMarkdown)
        {
            Sections = new List<Section>();
            References = new Dictionary<int, Reference>();

            // remove windows \r
            string str = pMarkdown.Replace("\r", "");

            ReadFooter(str);
            str = RemoveFooter(str);

            ReadSections(str);

            // clean all the sections
            foreach (Section section in Sections)
            {
                foreach (Reference reference in References.Values)
                {
                    section.CleanReference(reference);
                }
            }
        }

        /// <summary>
        /// Returns all the references of the specified type.
        /// </summary>
        public List<Reference> getReferences(ReferenceType pType)
        {
            return (from Reference r in References.Values where r.Type == pType select r).ToList();
        }

        /// <summary>
        /// Returns the reference by it's ID.
        /// </summary>
        public Reference getReference(int pID)
        {
            return References[pID];
        }

        /// <summary>
        /// Removes a reference from the document.
        /// </summary>
        public void Remove(int pID)
        {
            if (!References.ContainsKey(pID))
            {
                return;
            }
            Reference reference = References[pID];
            foreach (Section section in Sections)
            {
                section.RemoveReference(reference);
            }
            References.Remove(pID);
        }

        /// <summary>
        /// Removes all the empty links from the markdown, and then removes
        /// and references that are no longer used.
        /// </summary>
        public void RemoveDeadReferences()
        {
            // perform multiple passes to remove empty nested links
            bool changed;
            do
            {
                changed = false;

                List<Reference> refs = new List<Reference>(References.Values);
                foreach (Reference reference in refs)
                {
                    bool dead = Sections.All(pSection=>!pSection.HasReference(reference));
                    if (!dead)
                    {
                        continue;
                    }
                    foreach (Section section in Sections)
                    {
                        section.RemoveReference(reference);
                    }
                    References.Remove(reference.ID);
                    changed = true;
                }
            } while (changed);
        }

        /// <summary>
        /// A reference from the document to a resource.
        /// </summary>
        public class Reference
        {
            /// <summary>
            /// A list of titles used in the article for this reference.
            /// </summary>
            public readonly List<string> Titles = new List<string>();

            /// <summary>
            /// The reference ID.
            /// </summary>
            public int ID;

            /// <summary>
            /// The type of resource.
            /// </summary>
            public ReferenceType Type;

            /// <summary>
            /// The URL of the resource.
            /// </summary>
            public string URL;

            /// <summary>
            /// Formatted as a reference link for the markdown footer.
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return string.Format("  [{0}]: {1}", ID, URL);
            }
        }

        /// <summary>
        /// A section can be a paragraph, heading or list.
        /// </summary>
        public class Section
        {
            /// <summary>
            /// The clean version with references removed.
            /// </summary>
            public string Clean { get; private set; }

            /// <summary>
            /// The original markdown for this section.
            /// </summary>
            public string Markdown { get; private set; }

            /// <summary>
            /// The type of section.
            /// </summary>
            public SectionType Type { get; private set; }

            /// <summary>
            /// Reads the reference anchor and replaces it with the title.
            /// </summary>
            private static string CreateReferences(string pStr, Reference pReference, string pRegexTerm, string pGroup,
                                                   string pReplaceWith, bool pExpandTitle)
            {
                string regex = string.Format(pRegexTerm, pReference.ID);
                foreach (Match match in Regex.Matches(pStr, regex))
                {
                    pReference.Titles.Add(match.Groups[pGroup].Value);
                    string find = string.Format(pReplaceWith, match.Groups[pGroup].Value, pReference.ID);
                    pStr = pStr.Replace(find, pExpandTitle ? match.Groups[pGroup].Value : "");
                }
                return pStr;
            }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="pType"></param>
            /// <param name="pStr"></param>
            public Section(SectionType pType, string pStr)
            {
                Type = pType;
                Markdown = pStr;

                switch (Type)
                {
                    case SectionType.BULLET:
                        Clean = "";
                        foreach (string line in Markdown.Split(new[] {'\n'}))
                        {
                            Clean += line.Substring(1).Trim() + "\n";
                        }
                        Clean = Clean.Trim();
                        break;
                    case SectionType.HEADING:
                        Clean = pStr;
                        while (Clean.StartsWith("#") && Clean.EndsWith("#"))
                        {
                            Clean = Clean.Substring(1, Clean.Length - 2);
                        }
                        break;
                    case SectionType.LINE:
                        Clean = "";
                        break;
                    case SectionType.PARAGRAPH:
                        Clean = pStr;
                        break;
                    case SectionType.QUOTE:
                        Clean = Markdown.Substring(1).Trim();
                        break;
                }
            }

            /// <summary>
            /// Updates the clean string by removing references and replacing
            /// text with the link titles.
            /// </summary>
            public void CleanReference(Reference pReference)
            {
                Clean = CreateReferences(Clean, pReference, _REGEX_IMAGE, "alt", "![{0}][{1}]", false);
                Clean = CreateReferences(Clean, pReference, _REGEX_LINK, "title", "[{0}][{1}]", true);
            }

            /// <summary>
            /// True if the reference is used in the markdown.
            /// Note: Links that have no title will return False as not used.
            /// </summary>
            public bool HasReference(Reference pReference)
            {
                string regex;

                switch (pReference.Type)
                {
                        // True only if link has a title
                    case ReferenceType.LINK:
                        // remove any links with empty titles
                        Markdown = Markdown.Replace(string.Format("[][{0}]", pReference.ID), "");

                        // check if the reference is used.
                        regex = string.Format(_REGEX_LINK, pReference.ID);
                        if (
                            Regex.Matches(Markdown, regex)
                                .Cast<Match>()
                                .Any(pMatch=>pMatch.Groups["title"].Value.Trim() != ""))
                        {
                            return true;
                        }
                        break;
                        // True if there are any image references
                    case ReferenceType.IMAGE:
                        regex = string.Format(_REGEX_IMAGE, pReference.ID);
                        return Regex.IsMatch(Markdown, regex);
                        // TODO: Add support for video
                    default:
                        return true;
                }
                return false;
            }

            /// <summary>
            /// Updates the markdown to reference the uses of a reference that no longer
            /// exists.
            /// </summary>
            public void RemoveReference(Reference pReference)
            {
                Markdown = CreateReferences(Markdown, pReference, _REGEX_IMAGE, "alt", "![{0}][{1}]", false);
                Markdown = CreateReferences(Markdown, pReference, _REGEX_LINK, "title", "[{0}][{1}]", true);
            }
        }
    }
}