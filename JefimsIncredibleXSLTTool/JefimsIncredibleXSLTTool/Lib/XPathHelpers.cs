using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace JefimsIncredibleXsltTool.Lib
{
    public class XPathHelpers
    {
        public static XElement GetXElementFromCursor(string xml, int line, int column)
        {
            using (var reader = new StringReader(xml))
            {
                var xdoc = XDocument.Load(reader, LoadOptions.SetLineInfo | LoadOptions.PreserveWhitespace);
                XElement bestCandidate = null;
                foreach (var ancestor in xdoc.Descendants())
                {
                    var lineInfo = ((IXmlLineInfo)ancestor);
                    var lineInfoLinePositionFixed = lineInfo.LinePosition - 2;
                    if (lineInfo.LineNumber > line) continue;
                    if (lineInfo.LineNumber == line && lineInfoLinePositionFixed > column) continue;
                    if (bestCandidate == null) bestCandidate = ancestor;
                    var bestCandidateLineInfo = (IXmlLineInfo)bestCandidate;
                    if (line - bestCandidateLineInfo.LineNumber > line - lineInfo.LineNumber) bestCandidate = ancestor;
                }
                return bestCandidate;
            }
        }

        /// <summary>
        /// Get the absolute XPath to a given XElement
        /// (e.g. "/people/person[6]/name[1]/last[1]").
        /// </summary>
        public static string GetAbsoluteXPath(XElement element, bool includeIndexes)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            string RelativeXPath(XElement e)
            {
                var index = includeIndexes ? IndexPosition(e) : -1;
                var currentNamespace = e.Name.Namespace;
                
                var namespacePrefix = e.GetPrefixOfNamespace(currentNamespace);
                var name = namespacePrefix + (string.IsNullOrWhiteSpace(namespacePrefix) ? "" : ":") + e.Name.LocalName;

                // If the element is the root, no index is required
                return (index == -1) ? "/" + name : $"/{name}[{index}]";
            }

            var ancestors = from e in element.Ancestors()
                            select RelativeXPath(e);

            return string.Concat(ancestors.Reverse().ToArray()) +
                   RelativeXPath(element);
        }

        /// <summary>
        /// Get the index of the given XElement relative to its
        /// siblings with identical names. If the given element is
        /// the root, -1 is returned.
        /// </summary>
        /// <param name="element">
        /// The element to get the index of.
        /// </param>
        public static int IndexPosition(XElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            if (element.Parent == null)
            {
                return -1;
            }

            var i = 1; // Indexes for nodes start at 1, not 0

            foreach (var sibling in element.Parent.Elements(element.Name))
            {
                if (sibling == element)
                {
                    return i;
                }

                i++;
            }

            throw new InvalidOperationException
                ("element has been removed from its parent.");
        }
    }
}
