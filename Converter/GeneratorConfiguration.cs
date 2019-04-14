using System.Collections.Generic;
using SQL_Generator_WPF.Converter.Format;

namespace SQL_Generator_WPF.Converter
{
    public class GeneratorConfiguration
    {
        private const string UpperCamelCaseName = "UpperCamelCase";
        private const string LowerCamelCaseName = "lowerCamelCase";
        private const string UnderscoreCaseName = "underscore_case";

        public static readonly Dictionary<NamingTypes, string> NamingTypesNames = new Dictionary<NamingTypes, string>()
        {
            {NamingTypes.UpperCaseName, "UpperCamelCase"},
            {NamingTypes.LowerCaseName, "lowerCamelCase"},
            {NamingTypes.UnderscoreCase, "lower_case"},
            {NamingTypes.Mixed, "Mixed: UpperCase[columns] / lower_case[tables]"}
        };

        public static readonly Dictionary<string, Formatter> NamingFormatters = new Dictionary<string, Formatter>()
        {
            {UpperCamelCaseName, new UpperCamelFormatter()},
            {UnderscoreCaseName, new UnderscoreFormatter()},
            {LowerCamelCaseName, new LowerCamelFormatter()},
        };


        public Formatter ColumnFormatter { get; set; }
        public Formatter TableFormatter { get; set; }
        public bool AddLongNameForColumnId { get; set; }
        public bool AddIdWithPrimaryAuto { get; set; }
        public bool SetIntUnsigned { get; set; }
        public bool AddDrops { get; set; }
        public bool AddQuotas { get; set; }
        public bool ReferencesInline { get; set; }
        public bool PrimaryKeyInline { get; set; }
        public bool NotNullByDefault { get; set; }
        public string TablePrefix { get; set; }
        public string ColumnPrefix { get; set; }
        public bool SkipIdInsterting { get; set; }
        public string ReplacementFormat { get; set; }

        public string TableColumnName;
        public string TypeColumnName;
        public string NecessityColumnName;
        public string UniqueColumnName;
        public string DescriptionColumnName;
        public string AttrsLeftColumnName;
        public string TableColumnGeneral;
        public string TypeColumnGeneral;
        public string NoAnswer;
        public string YesAnswer;


        public GeneratorConfiguration()
        {
            AddLongNameForColumnId = false;
            AddDrops = false;
            AddIdWithPrimaryAuto = true;
            SetIntUnsigned = false;
            ReferencesInline = true;
            PrimaryKeyInline = true;
            SkipIdInsterting = true;
            NotNullByDefault = false;
            ReplacementFormat = "const {0}{1} = '{2}';\n";
            ColumnFormatter = NamingFormatters[UpperCamelCaseName];
            TableFormatter = NamingFormatters[UnderscoreCaseName];
        }
    }
}