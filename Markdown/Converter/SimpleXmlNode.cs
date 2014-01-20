using System.Collections.Generic;
using System.Xml;

namespace Markdown.Converter
{
    /// <summary>
    /// Represents a simple XML node.
    /// Makes testing the writers easier.
    /// </summary>
    public class SimpleXmlNode : Dictionary<string, string>
    {
        /// <summary>
        /// Inner name
        /// </summary>
        private string _name;

        /// <summary>
        /// Keys will always return a value or empty string.
        /// No exceptions if key doesn't exist.
        /// </summary>
        public new string this[string pKey]
        {
            get { return ContainsKey(pKey) ? base[pKey] : ""; }
        }

        /// <summary>
        /// The tag
        /// </summary>
        public string Name
        {
            get { return _name; }
            private set { _name = value.Trim().ToLower(); }
        }

        /// <summary>
        /// The inner text
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        private SimpleXmlNode()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public SimpleXmlNode(string pName, string pValue)
            : this()
        {
            Name = pName;
            Value = pValue;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public SimpleXmlNode(XmlReader pReader)
            : this()
        {
            pReader.MoveToElement();
            Name = pReader.Name;
            Value = pReader.Value;

            for (int i = 0, c = pReader.AttributeCount; i < c; i++)
            {
                pReader.MoveToAttribute(i);
                Add(pReader.Name, pReader.Value);
            }
        }

        /// <summary>
        /// Alias for ContainsKey
        /// </summary>
        public bool Has(string pKey)
        {
            return ContainsKey(pKey);
        }
    }
}