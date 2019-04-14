using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQL_Generator_WPF.Converter.Format
{
    public abstract class Formatter
    {
        private static readonly Dictionary<char,char> PolishChars = new Dictionary<char, char>()
        {
            {'Ą', 'A'}, {'Ć', 'C'}, {'Ę', 'E'}, {'Ł', 'L'}, {'Ń', 'N'}, {'Ó', 'O'}, {'Ś', 'S'}, {'Ź', 'Z'}, {'Ż', 'Z'},
            {'ą', 'a'}, {'ć', 'c'}, {'ę', 'e'}, {'ł', 'l'}, {'ń', 'n'}, {'ó', 'o'}, {'ś', 's'}, {'ź', 'z'}, {'ż', 'z'},
        };

        public abstract string FormatName(string name, GeneratorConfiguration configuration);

        protected IReadOnlyList<string> GetSeparatedWords(string name, GeneratorConfiguration configuration)
        {
            string[] parts = name.Trim().Split(' ');
            IReadOnlyList<string> words = new List<string>(parts);
            return words;
        }

        protected string Clean(string input)
        {
            var min = '\u0000';
            var max = '\u007F';
            return string.Join("", input.Select(c =>
            {
                if (c >= min && c <= max)
                {
                    return c;
                }
                if (!PolishChars.TryGetValue(c, out var r))
                {
                    r = '\0';
                }
                return r;
            }));
/*
            string asAscii = Encoding.ASCII.GetString(
                Encoding.Convert(
                    Encoding.UTF8,
                    Encoding.GetEncoding(
                        Encoding.ASCII.EncodingName,
                        new EncoderReplacementFallback(string.Empty),
                        new DecoderExceptionFallback()
                    ),
                    Encoding.UTF8.GetBytes(inputString)
                )
            );*/
        }
    }
}