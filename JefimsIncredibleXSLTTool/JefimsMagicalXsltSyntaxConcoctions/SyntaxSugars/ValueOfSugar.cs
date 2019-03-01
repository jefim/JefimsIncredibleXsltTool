using System.Text.RegularExpressions;

namespace JefimsMagicalXsltSyntaxConcoctions.SyntaxSugars
{
    public class ValueOfSugar : ISyntaxSugar
    {
        public int Priority => 1;

        public string PureXsltToXsltWithSugar(string pureXslt)
        {
            var regexPattern = @"<xsl:value-of[ \t\r\n]+select=""([^\""]+)""[ \t\r\n]+/>";
            var replace = "#echo $1";
            return new Regex(regexPattern).Replace(pureXslt, replace);
        }

        public string XsltWithSugarToPureXslt(string xsltWithSugar)
        {
            var regexPattern = @"#echo[ \t]+([^\r\n""<]+)";
            var replace = @"<xsl:value-of select=""$1"" />";
            return new Regex(regexPattern).Replace(xsltWithSugar, replace);
        }
    }
}
