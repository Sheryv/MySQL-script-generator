using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQL_Generator_WPF.Converter.Format
{
    public class UpperCamelFormatter : Formatter
    {
        public override string FormatName(string name, GeneratorConfiguration configuration)
        {
            return string.Join("", GetSeparatedWords(name, configuration)
                .Select(word => word[0].ToString().ToUpper() + word.Substring(1).ToLower()));
        }
    }
}