using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQL_Generator_WPF.Coverter
{
    class GeneratorConfiguration
    {
        public static readonly Dictionary<NamingTypes, string> NamingTypesNames = new Dictionary<NamingTypes, string>()
        {
            {NamingTypes.UpperCaseName, "UpperCamelCase"},
            {NamingTypes.LowerCaseName, "lowerCamelCase"},
            {NamingTypes.UnderscoreCase, "lower_case"},
            {NamingTypes.Mixed, "Mixed: UpperCase[columns] / lower_case[tables]"}
        };

        public NamingTypes NamingConvention { get; set; }
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
            NamingConvention = NamingTypes.Mixed;
            SetIntUnsigned = false;
            ReferencesInline = true;
            PrimaryKeyInline = true;
            NotNullByDefault = false;
        }
    }
}