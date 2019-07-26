using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JefimsIncredibleXsltTool.Lib
{
    public class TagCloser
    {
        public string GetClosingTagIfAny(string xml, int cursorPosition)
        {
            var closingTags = new List<Tag>();
            var currentPosition = cursorPosition;
            while (true)
            {
                if (currentPosition < 0) return null;
                var prevTag = FindPrevTag(xml, currentPosition);
                if (prevTag == null) return null;
                if (prevTag.IsClosing)
                {
                    closingTags.Add(prevTag);
                }
                else
                {
                    if (!closingTags.Any()) return prevTag.TagName;

                    if (closingTags.Any() && closingTags.Last().TagName == prevTag.TagName)
                    {
                        closingTags.RemoveAt(closingTags.Count - 1);
                    }
                }

                currentPosition = prevTag.StartIndex;
            }
        }

        public string GetClosingTagIfAnyOld(string xml, int cursorPosition)
        {
            var prevTagEnd = -1;
            var prevTagStart = -1;
            bool? isPrevTagClosing = null;
            for(var i = (cursorPosition - 1); i >= 0; i--)
            {
                // Search for a tag closing-symbol >
                if (xml[i] == '>')
                {
                    prevTagEnd = i;
                }
                
                // Search for a tag opening <, but only if we already
                // found a >
                if (xml[i] == '<' && prevTagEnd >= 0)
                {
                    prevTagStart = i;
                    isPrevTagClosing = (xml.Length > (i + 1)) && (xml[i + 1] == '/');
                }

                // We found an opening tag!
                if (prevTagStart >= 0)
                {
                    // If this is an opening tag - then get the tag name
                    // and return it. Otherwise (if it is a closing tag)
                    // we do not care about it and just reset our search
                    if (isPrevTagClosing == false)
                    {
                        for (var j = prevTagStart; j < xml.Length; j++)
                        {
                            var charJ = xml[j];
                            if (char.IsWhiteSpace(charJ) || charJ == '>')
                            {
                                var tagNameEnd = j;
                                var tagNameStart = prevTagStart + 1; // +1 because <
                                var tagNameLen = j - prevTagStart - 1; // -1 because of > or whitespace
                                var tagName = xml.Substring(tagNameStart, tagNameLen);
                                return tagName;
                            }
                        }
                    }
                    else
                    {
                        prevTagEnd = -1;
                        prevTagStart = -1;
                        isPrevTagClosing = null;
                    }
                }
            }

            return null;
        }

        public Tag FindPrevTag(string xml, int currentPosition)
        {
            var prevTagEnd = -1;
            var prevTagStart = -1;
            bool? isPrevTagClosing = null;
            for (var i = (currentPosition - 1); i >= 0; i--)
            {
                // Search for a tag closing-symbol >
                if (xml[i] == '>')
                {
                    prevTagEnd = i;
                }

                // Search for a tag opening <, but only if we already
                // found a >
                if (xml[i] == '<' && prevTagEnd >= 0)
                {
                    prevTagStart = i;
                    isPrevTagClosing = (xml.Length > (i + 1)) && (xml[i + 1] == '/');
                    var tag = new Tag
                    {
                        StartIndex = prevTagStart,
                        IsClosing = (bool)isPrevTagClosing,
                        TagName = GetTagName(xml, prevTagStart)
                    };
                    return tag;
                }
            }

            return null;
        }

        public string GetTagName(string xml, int tagStart)
        {
            for (var j = tagStart; j < xml.Length; j++)
            {
                var charJ = xml[j];
                if (char.IsWhiteSpace(charJ) || charJ == '>')
                {
                    var tagNameEnd = j;
                    var tagNameStart = tagStart + 1; // +1 because <
                    var tagNameLen = j - tagStart - 1; // -1 because of > or whitespace
                    var tagName = xml.Substring(tagNameStart, tagNameLen);
                    return tagName.Trim('/');
                }
            }

            return null;
        }
    }

    public class Tag
    {
        public string TagName { get; set; }
        public int StartIndex { get; set; }
        public bool IsClosing { get; set; }
    }
}
