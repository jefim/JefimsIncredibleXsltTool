using System.Text.RegularExpressions;
namespace JefimsMagicalXsltSyntaxConcoctions.SyntaxSugars
{
    public class ChooseSugar : ISyntaxSugar
    {
        public int Priority => 1;

        public string PureXsltToXsltWithSugar(string pureXslt)
        {
            var result = pureXslt;

            // Replace xsl:choose together with first when element
            // Example:
            // <xsl:choose>
            //     <xsl:when test="$some test">
            var regexChooseAndFirstWhen = new Regex(@"<xsl:choose>([ \t\r\n]*)<xsl:when[ \t\r\n]+test=""([^""]+)"">");
            result = regexChooseAndFirstWhen.Replace(result, @"#choose$1#when $2");

            // Replace one closing when and new opening when elements
            // Example:
            // </xsl:when>
            // <xsl:when test="$some test">
            var regexClosingWhenAndNewWhen = new Regex(@"</xsl:when>([ \t\r\n]*)<xsl:when[ \t\r\n]+test=""([^""]*)"">");
            result = regexClosingWhenAndNewWhen.Replace(result, @"#when $2");

            // Replace one closing when and new opening otherwise
            // Example:
            // </xsl:when>
            // <xsl:otherwise>
            var regexClosingWhenAndNewOtherwise = new Regex(@"</xsl:when>([ \t\r\n]*)<xsl:otherwise>");
            result = regexClosingWhenAndNewOtherwise.Replace(result, @"#else");

            // Replace one closing otherwise
            // Example:
            // </xsl:otherwise>
            result = result.Replace("</xsl:otherwise>", "#end-else");
            
            // Replace remaining choose els, if no 'when' els defined
            result = result.Replace("<xsl:choose>", "#choose");

            // Replace one closing choose
            // Example:
            // </xsl:choose>
            result = result.Replace("</xsl:choose>", "#end-choose");

            return result;
        }

        public string XsltWithSugarToPureXslt(string xsltWithSugar)
        {
            var result = xsltWithSugar;

            // Replace xsl:choose together with first when element
            // Example:
            // <xsl:choose>
            //     <xsl:when test="$some test">
            var regexChooseAndFirstWhen = new Regex(@"#choose([ \t\r\n]*)#when[ \t]+([^""\r\n]+)");
            result = regexChooseAndFirstWhen.Replace(result, @"<xsl:choose>$1<xsl:when test=""$2"">");

            // Replace one closing when and new opening when elements
            // Example:
            // </xsl:when>
            // <xsl:when test="$some test">
            var regexClosingWhenAndNewWhen = new Regex(@"([ \t\r\n]*)#when[ \t]+([^""\r\n]+)");
            result = regexClosingWhenAndNewWhen.Replace(result, @"$1</xsl:when>$1<xsl:when test=""$2"">");

            // Replace one closing when and new opening otherwise
            // Example:
            // </xsl:when>
            // <xsl:otherwise>
            var regexClosingWhenAndNewOtherwise = new Regex(@"([ \t\r\n]*)#else");
            result = regexClosingWhenAndNewOtherwise.Replace(result, @"$1</xsl:when>$1<xsl:otherwise>");

            // Replace one closing otherwise
            // Example:
            // </xsl:otherwise>
            result = result.Replace("#end-else", "</xsl:otherwise>");

            // Replace remaining choose els, if no 'when' els defined
            result = result.Replace("#choose", "<xsl:choose>");

            // Replace one closing choose
            // Example:
            // </xsl:choose>
            result = result.Replace("#end-choose", "</xsl:choose>");

            return result;
        }
    }
}
