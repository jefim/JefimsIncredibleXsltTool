using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JefimsIncredibleXsltTool.Lib;

namespace Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var xml = @"
<a test=""dada"">
    <bda>
        <c></c>
    </bda>
</a>";
            Assert.AreEqual("bda", XPathHelpers.GetXElementFromCursor(xml, 4, 0).Name.LocalName);
            Assert.AreEqual("c", XPathHelpers.GetXElementFromCursor(xml, 4, 8).Name.LocalName);
            Assert.AreEqual("/a/bda/c", XPathHelpers.GetAbsoluteXPath(XPathHelpers.GetXElementFromCursor(xml, 4, 8), false));
        }

        [TestMethod]
        public void TestMethod2()
        {
            var xml = @"
<breakfast_menu>
<food>
<calories>950</calories>
</food>
</breakfast_menu>";
            Assert.AreEqual("/breakfast_menu/food", XPathHelpers.GetAbsoluteXPath(XPathHelpers.GetXElementFromCursor(xml, 5, 8), false));
        }
    }
}
