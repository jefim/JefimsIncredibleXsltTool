using JefimsMagicalXsltSyntaxConcoctions.SyntaxSugars;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JefimsMagicalXsltSyntaxConcoctions
{
    public class JefimsMagicalTranspiler : ISyntaxSugar
    {
        public int Priority => 1;

        private readonly List<ISyntaxSugar> _sugars = new List<ISyntaxSugar>
        {
            new XsltStylesheetSugar(),
            new ValueOfSugar(),
            new VariableSugar(),
            new ParamSugar(),
            new ChooseSugar()
        };

        public string PureXsltToXsltWithSugar(string pureXslt)
        {
            var result = pureXslt;
            var exceptions = new List<SyntaxException>();
            foreach(var sugar in _sugars.OrderBy(o => o.Priority))
            {
                try {
                    result = sugar.PureXsltToXsltWithSugar(result);
                }
                catch(SyntaxException ex)
                {
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Any()) throw new AggregateException("Error when transpiling pure XSLT to XSLT with sugar", exceptions);

            return result;
        }

        public string XsltWithSugarToPureXslt(string xsltWithSugar)
        {
            var result = xsltWithSugar;
            var exceptions = new List<SyntaxException>();
            foreach (var sugar in _sugars.OrderBy(o => o.Priority))
            {
                try
                {
                    result = sugar.XsltWithSugarToPureXslt(result);
                }
                catch (SyntaxException ex)
                {
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Any()) throw new AggregateException("Error when transpiling sugary XSLT to pure XSLT", exceptions);

            return result;
        }
    }

    public interface ISyntaxSugar
    {
        int Priority { get; }
        string XsltWithSugarToPureXslt(string xsltWithSugar);

        string PureXsltToXsltWithSugar(string pureXslt);
    }

    public class SyntaxException : Exception
    {
        public SyntaxException() { }
        public SyntaxException(string message) : base(message) { }
    }

}
