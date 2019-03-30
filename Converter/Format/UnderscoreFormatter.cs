using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQL_Generator_WPF.Converter.Format
{
    public class UnderscoreFormatter : Formatter
    {
        public override string FormatName(string name, GeneratorConfiguration configuration)
        {
            var list = GetSeparatedWords(name, configuration).Select(s => s.ToLower());
            return string.Join("_", list);
        }
    }
}