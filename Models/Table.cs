using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQL_Generator_WPF.Models
{
    class Table
    {
        public string Name { get; set; }
        public string RawName { get; set; }
        public string Additions { get; set; }
        public List<Column> Columns { get; private set; }

        public Table(string name, string rawName)
        {
            Name = name;
            RawName = rawName;
        }

        public void AddColumn(Column column)
        {
            if (Columns == null)
            {
                Columns = new List<Column>();
            }
            Columns.Add(column);
        }
    }
}