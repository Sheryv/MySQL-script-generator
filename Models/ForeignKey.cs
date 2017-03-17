using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQL_Generator_WPF.Models
{
    class ForeignKey
    {
        public Column Key { get; set; }
        public Table RefTable { get; set; }
        public Column RefColumn { get; set; }
        public string RefTableString { get; set; }
        public string RefColumnString { get; set; }
    }
}
