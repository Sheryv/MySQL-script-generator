using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQL_Generator_WPF.Converter.Format
{
    public class LowerCamelFormatter : Formatter
    {
        public override string FormatName(string name, GeneratorConfiguration configuration)
        {
            string result = "";
            var list = GetSeparatedWords(name, configuration);
            for (var i = 0; i < list.Count; i++)
            {
                var word = list[i];
                string letter;
                if (i == 0)
                {
                    letter = word[0].ToString().ToLower();
                }
                else
                {
                    letter = word[0].ToString().ToUpper();
                }
                result += letter + word.Substring(1).ToLower();
            }
            return result;
        }
    }
}