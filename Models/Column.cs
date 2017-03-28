using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQL_Generator_WPF.Coverter;

namespace SQL_Generator_WPF.Models
{
    class Column
    {
        public string Name { get; set; }
        public DataType Type { get; set; }
        //public int Size { get; set; }
        public AttributePattern NotNullAttribute { get; set; }
        public AttributePattern UniqueAttribute { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsAutoIncrement { get; set; }
        public bool IsUnsigned { get; set; }
        public List<ForeignKey> ForeignKeys { get; private set; }
        public List<Constraint> Constraints { get; private set; }

        public Column(string name)
        {
            Name = name;
            NotNullAttribute = BasicGenerator.Instance.AttributeNotNull;
            UniqueAttribute = null;
            IsPrimary = false;
            IsAutoIncrement = false;
            IsUnsigned = false;
        }

        public void AddForeignKey(ForeignKey key)
        {
            if (ForeignKeys == null)
            {
                ForeignKeys = new List<ForeignKey>();
            }
            ForeignKeys.Add(key);
        }

        public void SetPrimaryAutoIncrement()
        {
            Type = new DataType(BasicGenerator.Instance.DataTypePatterns.First(pattern => pattern.Type == EnumDataTypes.Int), BasicGenerator.Instance.DefaultIntegerSize);
            NotNullAttribute = BasicGenerator.Instance.AttributeNotNull;
            UniqueAttribute = null;
            IsPrimary = true;
            IsAutoIncrement = true;
        }

    }

    class AttributePattern
    {
        public AttributePattern( EnumAtrributes atrributeType, string printName, string searchString, bool inverse)
        {
            PrintName = printName;
            SearchString = searchString;
            Inverse = inverse;
            AtrributeType = atrributeType;
        }

        public AttributePattern( EnumAtrributes atrributeType, string printName, string searchString)
        {
            PrintName = printName;
            SearchString = searchString;
            AtrributeType = atrributeType;
            Inverse = false;
        }
//
//        public static List<AttributePattern> GetAttributePatterns()
//        {
//            List<AttributePattern> list = new List<AttributePattern>();
//            BasicGenerator gen = BasicGenerator.Instance;
//            list.Add(new AttributePattern(EnumAtrributes.NotNull, gen.NotNull, "n", true));
//            list.Add(new AttributePattern(EnumAtrributes.Unique, gen.Unique, "u"));
//            list.Add(new AttributePattern(EnumAtrributes.References, gen.References, "ref"));
//            return list;
//        }

        public EnumAtrributes AtrributeType { get; set; }
        public string SearchString { get; set; }
        public string PrintName { get; set; }
        public bool Inverse { get; set; }
    }

    enum EnumAtrributes
    {
        NotNull,
        Unique,
        References,
        PrimaryKey
    }

}
