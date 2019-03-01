using Microsoft.VisualStudio.TestTools.UnitTesting;
using JefimsIncredibleXsltTool.Lib;

namespace Tests
{
    [TestClass]
    public class GetAbsoluteXPathTests
    {
        [TestMethod]
        public void GetAbsoluteXPathWithIndex()
        {
            var xml = @"
<a test=""dada"">
    <bda>
        <c></c>
        <c></c>
    </bda>
</a>";
            Assert.AreEqual("bda", XPathHelpers.GetXElementFromCursor(xml, 4, 0).Name.LocalName);
            Assert.AreEqual("c", XPathHelpers.GetXElementFromCursor(xml, 4, 8).Name.LocalName);
            Assert.AreEqual("/a/bda[1]/c[2]", XPathHelpers.GetAbsoluteXPath(XPathHelpers.GetXElementFromCursor(xml, 5, 8), true));
        }

        [TestMethod]
        public void TestGetAbsoluteXPath()
        {
            var xml = @"
<breakfast_menu>
<food>
<calories>950</calories>
</food>
</breakfast_menu>";
            Assert.AreEqual("/breakfast_menu/food", XPathHelpers.GetAbsoluteXPath(XPathHelpers.GetXElementFromCursor(xml, 3, 8), false));
        }
    }
}
