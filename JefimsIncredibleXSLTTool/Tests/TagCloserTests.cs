using JefimsIncredibleXsltTool.Lib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass]
    public class TagCloserTests
    {
        [TestMethod]
        public void Test_OnlyOneTag()
        {
            var closer = new TagCloser();
            var closingTag = closer.GetClosingTagIfAny("<xml><", 6);
            Assert.AreEqual("xml", closingTag);
        }
        [TestMethod]
        public void Test_OnlyOneTagWithText()
        {
            var closer = new TagCloser();
            var xml = "<xml>test text<";
            var closingTag = closer.GetClosingTagIfAny(
                xml,
                xml.Length);
            Assert.AreEqual("xml", closingTag);
        }

        [TestMethod]
        public void Test_NestedTags()
        {
            var closer = new TagCloser();
            var xml = "<xml><str>hello, world</str><";
            var closingTag = closer.GetClosingTagIfAny(
                xml,
                xml.Length);
            Assert.AreEqual("xml", closingTag);
        }

        [TestMethod]
        public void Test_SelfClosingTag()
        {
            var closer = new TagCloser();
            var xml = "<xml><str>hello, <self-closing-tag /> world</str><";
            var closingTag = closer.GetClosingTagIfAny(
                xml,
                xml.Length);
            Assert.AreEqual("xml", closingTag);
        }

        [TestMethod]
        public void FindPrevTag_Closing()
        {
            var closer = new TagCloser();
            var xml = "<xml><str>hello, world</str><s";
            var tag = closer.FindPrevTag(xml, xml.Length);
            Assert.AreEqual("str", tag.TagName);
            Assert.AreEqual(true, tag.IsClosing);
            Assert.AreEqual(22, tag.StartIndex);
        }

        [TestMethod]
        public void FindPrevTag_Opening()
        {
            var closer = new TagCloser();
            var xml = "<xml><str>hello, world</str><s";
            var tag = closer.FindPrevTag(xml, 15);
            Assert.AreEqual("str", tag.TagName);
            Assert.AreEqual(false, tag.IsClosing);
            Assert.AreEqual(5, tag.StartIndex);
        }
    }
}
