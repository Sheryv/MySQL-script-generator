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
        public string Additions { get; set; }
        public List<Column> Columns { get; private set; }

        public Table(string name)
        {
            Name = name;
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