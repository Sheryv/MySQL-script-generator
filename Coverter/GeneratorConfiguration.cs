using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQL_Generator_WPF.Coverter
{
    class GeneratorConfiguration
    {
        public bool UpperCamelCase { get; set; }
        public bool AddLongNameForColumnId { get; set; }
        public bool AddIdWithPrimaryAuto { get; set; }
        public bool SetIntUnsigned { get; set; }
        public bool AddDrops { get; set; }
        public bool AddQuotas { get; set; }

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
            UpperCamelCase = true;
            SetIntUnsigned = false;
        }
    }
}