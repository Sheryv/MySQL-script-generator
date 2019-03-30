using System.Collections.Generic;

namespace SQL_Generator_WPF.Converter.Format
{
    public abstract class Formatter
    {
        public abstract string FormatName(string name, GeneratorConfiguration configuration);

        protected IReadOnlyList<string> GetSeparatedWords(string name, GeneratorConfiguration configuration)
        {
            string[] parts = name.Trim().Split(' ');
            IReadOnlyList<string> words = new List<string>(parts);
            return words;
        }
    }
}