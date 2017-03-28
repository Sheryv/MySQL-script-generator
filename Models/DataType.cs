using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQL_Generator_WPF.Coverter;

namespace SQL_Generator_WPF.Models
{
    class DataType
    {
        public DataTypePattern Pattern { get; set; }
        public int Size { get; set; }
        public int AdditionalSize { get; set; }

        public DataType(DataTypePattern pattern,  int size)
        {
            Pattern = pattern;
            Size = size;
            AdditionalSize = -1;
        }

        public DataType(DataTypePattern pattern,  int size, int additionalSize)
        {
            Pattern = pattern;
            Size = size;
            AdditionalSize = additionalSize;
        }
    }

    class DataTypePattern
    {
        public static event Func<List<DataTypePattern>, List<DataTypePattern>> OnTypePatternsGeneration;
        public EnumDataTypes Type { get; set; }
        public string SearchString { get; set; }
        public string PrintName { get; set; }
        public bool HasSizeRequired { get; set; }
        public bool HasAddistinalSize { get; set; }

        /// <summary>
        /// For creating aliases
        /// </summary>
        /// <param name="type"></param>
        /// <param name="searchString">Alias</param>
        /// <param name="printName">Proper sql string</param>
        /// <param name="hasSizeRequired"></param>
        public DataTypePattern(EnumDataTypes type, string searchString, string printName, bool hasSizeRequired)
        {
            SearchString = searchString;
            HasSizeRequired = hasSizeRequired;
            Type = type;
            PrintName = printName;
            HasAddistinalSize = false;
        }

        public DataTypePattern(EnumDataTypes type, string searchString, bool hasSizeRequired)
        {
            SearchString = searchString;
            HasSizeRequired = hasSizeRequired;
            Type = type;
            PrintName = searchString;
            HasAddistinalSize = false;
        }

        public static List<DataTypePattern> GetPatterns()
        {
            List<DataTypePattern> list = new List<DataTypePattern>();
            BasicGenerator gen = BasicGenerator.Instance;
            list.Add(new DataTypePattern(EnumDataTypes.ByteInt, gen.TypeByteInt, true));
            list.Add(new DataTypePattern(EnumDataTypes.SmallInt, gen.TypeSmallInt, true));
            list.Add(new DataTypePattern(EnumDataTypes.Boolean, gen.TypeBoolean, false));
            list.Add(new DataTypePattern(EnumDataTypes.Int, gen.TypeInt, false));
            list.Add(new DataTypePattern(EnumDataTypes.Varchar, gen.TypeVarchar, true));
            list.Add(new DataTypePattern(EnumDataTypes.Char, gen.TypeChar, true));
            list.Add(new DataTypePattern(EnumDataTypes.LongText, gen.TypeLongText, false));
            list.Add(new DataTypePattern(EnumDataTypes.Text, gen.TypeText, false));
            list.Add(new DataTypePattern(EnumDataTypes.DateTime, gen.TypeDateTime, false));
            list.Add(new DataTypePattern(EnumDataTypes.Bit, gen.TypeBit, false));
            list.Add(new DataTypePattern(EnumDataTypes.TimeStamp, gen.TypeTimeStamp, false));
            list.Add(new DataTypePattern(EnumDataTypes.Date, gen.TypeDate, false));
            list.Add(new DataTypePattern(EnumDataTypes.Decimal, gen.TypeDecimal, true){HasAddistinalSize = true});
            if (OnTypePatternsGeneration != null)
                return OnTypePatternsGeneration.Invoke(list);
            return list;
        }
    }

    enum EnumDataTypes
    {
        Int,
        SmallInt,
        ByteInt,
        Varchar,
        Char,
        Text,
        LongText,
        Decimal,
        Blob,
        DateTime,
        Date,
        TimeStamp,
        Boolean,
        Bit
    }
}