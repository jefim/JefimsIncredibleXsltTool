using System;
using System.Text.RegularExpressions;

namespace JefimsMagicalXsltSyntaxConcoctions.SyntaxSugars
{
    public class XsltStylesheetSugar : ISyntaxSugar
    {
        public const string Keyword = "#here_be_xslt";

        public int Priority => 10;

        public string PureXsltToXsltWithSugar(string pureXslt)
        {
            var regexStart = @"<xsl:stylesheet[ \t\r\n]+version=""2.0""[ \t\r\n]+xmlns:xsl=""http://www.w3.org/1999/XSL/Transform""[ \t\r\n]+xmlns:xs=""http://www.w3.org/2001/XMLSchema""[ \t\r\n]+xmlns:fn=""http://www.w3.org/2005/xpath-functions""[ \t\r\n]+exclude-result-prefixes=""xs fn"">[ \t\r\n]+<xsl:output[ \t\r\n]+method=""xml""[ \t\r\n]+version=""1.0""[ \t\r\n]+encoding=""UTF-8""[ \t\r\n]+indent=""yes""/>";
            var regex = new Regex(regexStart);
            var hasStartTag = regex.IsMatch(pureXslt);
            regex.Matches(pureXslt);
            var hasEndTag = pureXslt.Trim().EndsWith("</xsl:stylesheet>");
            if (!hasStartTag || !hasEndTag) return pureXslt;
            var result = regex.Replace(pureXslt.Trim(), Keyword);
            result = result.Substring(0, result.Length - "</xsl:stylesheet>".Length);
            return result.Trim();

        }

        public string XsltWithSugarToPureXslt(string xsltWithSugar)
        {
            var index = xsltWithSugar.IndexOf(Keyword, StringComparison.Ordinal);
            if (index > 0) throw new SyntaxException($"Error: {Keyword} must be located at the start of the document and no later!");
            if (index < 0) return xsltWithSugar;
            var result = xsltWithSugar.Substring(Keyword.Length);
            result = @"<xsl:stylesheet version=""2.0""
	xmlns:xsl=""http://www.w3.org/1999/XSL/Transform""
	xmlns:xs=""http://www.w3.org/2001/XMLSchema""
	xmlns:fn=""http://www.w3.org/2005/xpath-functions""
    exclude-result-prefixes=""xs fn"">
    <xsl:output method=""xml"" version=""1.0"" encoding=""UTF-8"" indent=""yes""/>"
            + result + Environment.NewLine + 
            "</xsl:stylesheet>";
            return result;
        }
    }
}
