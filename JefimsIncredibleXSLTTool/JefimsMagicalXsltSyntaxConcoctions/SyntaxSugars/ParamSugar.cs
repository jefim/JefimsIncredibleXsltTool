using System.Text.RegularExpressions;

namespace JefimsMagicalXsltSyntaxConcoctions.SyntaxSugars
{
    public class ParamSugar : ISyntaxSugar
    {
        public int Priority => 1;

        public string PureXsltToXsltWithSugar(string pureXslt)
        {
            var regexPattern = @"<xsl:param[ \t\r\n]+name=""([^\""]+)""[ \t\r\n]+/>";
            var replace = "#param $1";
            return new Regex(regexPattern).Replace(pureXslt, replace);
        }

        public string XsltWithSugarToPureXslt(string xsltWithSugar)
        {
            var regexPattern = @"#param[ \t]+([^\r\n""<]+)";
            var replace = @"<xsl:param name=""$1"" />";
            return new Regex(regexPattern).Replace(xsltWithSugar, replace);
        }
    }
}
