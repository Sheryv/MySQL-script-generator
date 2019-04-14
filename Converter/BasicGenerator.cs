using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Google.Protobuf.WellKnownTypes;
using SQL_Generator_WPF.Converter.Format;
using SQL_Generator_WPF.Models;

namespace SQL_Generator_WPF.Converter
{
    class BasicGenerator
    {
        internal readonly string TypeInt = "INT";
        internal readonly string TypeSmallInt = "SMALLINT";
        internal readonly string TypeByteInt = "TINYINT";
        internal readonly string TypeVarchar = "VARCHAR";
        internal readonly string TypeChar = "CHAR";
        internal readonly string TypeText = "TEXT";
        internal readonly string TypeLongText = "LONGTEXT";
        internal readonly string TypeDateTime = "DATETIME";
        internal readonly string TypeDate = "DATE";
        internal readonly string TypeFloat = "FLOAT";
        internal readonly string TypeTimeStamp = "TIMESTAMP";
        internal readonly string TypeBoolean = "BOOLEAN";
        internal readonly string TypeBit = "BIT";
        internal readonly string TypeDecimal = "DECIMAL";
        internal readonly int DefaultIntegerSize = 11;

        internal readonly char QuoteSign = '`';

        internal readonly AttributePattern AttributeNullFlag;
        internal readonly AttributePattern AttributeUnique;
        internal readonly AttributePattern AttributeReference;
        internal readonly AttributePattern AttributePrimary;
        internal static BasicGenerator Instance;
        public List<Table> Tables { get; private set; } = new List<Table>();
        public Dictionary<string, string> Replacement { get; } = new Dictionary<string, string>();

        internal List<DataTypePattern> DataTypePatterns { get; }

        //public List<AttributePattern> AttributePatterns { get; }
        //ALTER TABLE `platnosci`
        // ADD CONSTRAINT `fk_Platnosci_RodzajePlatnosci1` FOREIGN KEY(`idRodzajPlatnosci`) REFERENCES `rodzajeplatnosci` (`idRodzajePlatnosci`) ON DELETE NO ACTION ON UPDATE NO ACTION,
        public GeneratorConfiguration Config { get; }

        private static readonly char[] tablesSeparators = {'+', '#'};

        internal bool WrapInDoubleQuotes { get; set; }
        internal string NotNull => "NOT NULL";
        internal string Unique => "UNIQUE";
        internal string Unsigned => "UNSIGNED";
        internal string AutoIncrement => "AUTO_INCREMENT";
        internal string Primary => "PRIMARY KEY";
        internal string Create => "CREATE TABLE";
        internal string References => "REFERENCES";
        internal string Drop => "DROP TABLE IF EXISTS";

        internal string ParamsTable => "ENGINE=InnoDB DEFAULT CHARSET=utf8";


        internal string AlterAddConstraint =>
            "ALTER TABLE {0} ADD CONSTRAINT {1} FOREIGN KEY({2}) REFERENCES {3} ({4})";


        internal string DeleteConstraintAction => "ON DELETE NO ACTION ON UPDATE NO ACTION";

        public static GeneratorConfiguration DummyConfiguration = new GeneratorConfiguration()
        {
            TableColumnName = "Nazwa pola",
            TypeColumnName = "Typ pola",
            NecessityColumnName = "Czy pole jest wymagane",
            UniqueColumnName = "Czy wartość jest unikatowa",
            DescriptionColumnName = "Opis",
            AttrsLeftColumnName = "Pozostałe atrybuty",
            TableColumnGeneral = "Nazwa tabeli",
            TypeColumnGeneral = "Typ",
            NoAnswer = "Nie",
            YesAnswer = "Tak",
        };

        private Random random;
        private StringBuilder sqlBuilder;
        private StringBuilder htmlBuilder;

        public BasicGenerator(GeneratorConfiguration config, bool wrapInDoubleQuotes) : this(config)
        {
            WrapInDoubleQuotes = wrapInDoubleQuotes;
        }

        public BasicGenerator(GeneratorConfiguration config)
        {
            Config = config;
            WrapInDoubleQuotes = false;
            Instance = this;
            DataTypePattern.OnTypePatternsGeneration += OnTypePatternsGenerationEvent;
            DataTypePatterns = DataTypePattern.GetPatterns();
            //AttributePatterns = AttributePattern.GetAttributePatterns();
            AttributeNullFlag = new AttributePattern(EnumAtrributes.NullFlag, NotNull, "n", true);
            AttributeUnique = new AttributePattern(EnumAtrributes.Unique, Unique, "u");
            AttributeReference = new AttributePattern(EnumAtrributes.References, References, "ref");
            AttributePrimary = new AttributePattern(EnumAtrributes.PrimaryKey, Primary, "pk");
            random = new Random();
        }

        protected string MakeItemName(string rawName, bool isColumn = true)
        {
            Formatter formatter = Config.TableFormatter;
            if (isColumn)
            {
                formatter = Config.ColumnFormatter;
            }
            var key = new LowerCamelFormatter().FormatName(rawName, DummyConfiguration);
            var name = rawName;
            if (Replacement.ContainsKey(key))
            {
                name = Replacement[key];
            }
            return formatter.FormatName(name, Config);
        }

        protected DataType GetDataType(string rawType)
        {
            foreach (DataTypePattern dataTypePattern in DataTypePatterns)
            {
                if (rawType.Contains(dataTypePattern.SearchString.ToLower()))
                {
                    Regex regexSizeBrackets = new Regex(@"\D*\(\s*\d+\s*\)\s*");
                    Regex regexDoubleSizeBrackets = new Regex(@"\D*\(\s*\d+\s+\d+\s*\)\s*");
                    string sizeString;
                    string sizeSecondString = null;
                    if (regexDoubleSizeBrackets.IsMatch(rawType))
                    {
                        int start = rawType.IndexOf('(');
                        int end = rawType.LastIndexOf(')');
                        sizeString = rawType.Substring(start + 1, end - start - 1).Trim();
                        string[] p = sizeString.Split(' ');
                        sizeString = p[0].Trim();
                        sizeSecondString = p[1].Trim();
                    }
                    else if (regexSizeBrackets.IsMatch(rawType))
                    {
                        int start = rawType.IndexOf('(');
                        int end = rawType.LastIndexOf(')');
                        sizeString = rawType.Substring(start + 1, end - start - 1).Trim();
                    }
                    else if (rawType.Contains(" "))
                    {
                        int index = rawType.LastIndexOf(' ');
                        sizeString = rawType.Substring(index);
                    }
                    else
                    {
                        if (dataTypePattern.HasSizeRequired)
                        {
                            throw new FormatException(
                                $"Data type require size but this type does not provide it: {dataTypePattern.Type} -> {dataTypePattern.PrintName}");
                        }
                        return new DataType(dataTypePattern, -1);
                    }
                    var size = 0;
                    try
                    {
                        size = Convert.ToInt32(sizeString);
                        if (sizeSecondString != null)
                        {
                            int size2 = Convert.ToInt32(sizeSecondString);
                            return new DataType(dataTypePattern, size, size2);
                        }
                    }
                    catch (Exception e)
                    {
                        throw new FormatException($"Wrong data type size format near: {rawType}");
                    }
                    return new DataType(dataTypePattern, size);
                }
            }
            throw new ArgumentException(
                $"Column data type name not recognized! Do you need define it yorself? Problem occured near statement: {rawType}");
        }

        private void LoadReplacement(string text)
        {
            var lines = new Regex(@"\r|#|\+|\t").Replace(text, "").Split('\n');
            foreach (var line in lines)
            {
                var pair = line.Split(',');
                if (pair.Length == 2 && pair[0].Length > 0 && pair[1].Length > 0)
                {
                    var name = new LowerCamelFormatter().FormatName(pair[0], DummyConfiguration);
                    Replacement[name] = pair[1];
                }
            }
        }


        public BasicGenerator Parse(string encodedString, string replacement)
        {
            LoadReplacement(replacement);
            Tables = new List<Table>();
            encodedString = new Regex(@" +").Replace(encodedString, " ");
            string[] tables = encodedString.Split(tablesSeparators);
            foreach (string tbl in tables)
            {
                bool addMultiPrimary = false;
                string s = tbl.Trim();
                if (s.Length <= 3)
                {
                    continue;
                }
                int columnsIndex = 1;
                string[] lines = s.Split('\n');
                string[] tableStringNameAttrs = lines[0].Split(',');
                string tableName = tableStringNameAttrs[0].Trim();
                if (tableStringNameAttrs.Length > 1 && tableStringNameAttrs[1].Contains("noid"))
                {
                    addMultiPrimary = true;
                }
                Table table = new Table(MakeItemName(tableName, false), tableName);
                Column column = null;
                if (!addMultiPrimary && Config.AddIdWithPrimaryAuto)
                {
                    string col = "id";
                    if (Config.AddLongNameForColumnId)
                    {
                        col += col + " " + MakeItemName(table.Name);
                    }
                    column = new Column(MakeItemName(col), col);
                    column.SetPrimaryAutoIncrement();
                    //columnsIndex++;
                    table.AddColumn(column);
                }
                for (; columnsIndex < lines.Length; columnsIndex++)
                {
                    string line = lines[columnsIndex].Trim();
                    if (line.Length <= 2)
                    {
                        continue;
                    }

                    //attrs
                    string[] attributes = line.Split(',');

                    //name
                    string name = MakeItemName(attributes[0]);
                    column = new Column(name, attributes[0]);
                    if (attributes.Length <= 1)
                    {
                        throw new ApplicationException("Error: Type not specified for line: " + columnsIndex +
                                                       " in table " + tableName);
                    }

                    //type
                    string dataType = attributes[1].Trim();
                    column.Type = GetDataType(dataType);
                    if (Config.SetIntUnsigned)
                    {
                        switch (column.Type.Pattern.Type)
                        {
                            case EnumDataTypes.ByteInt:
                            case EnumDataTypes.Int:
                            case EnumDataTypes.SmallInt:
                                column.IsUnsigned = true;
                                break;
                        }
                    }

                    //left attributes
                    string necessity = Config.NoAnswer;
                    string htmlAttrs = "";
                    string htmlUnique = Config.NoAnswer;
                    if (attributes.Length > 2)
                    {
                        for (int i = 2; i < attributes.Length; i++)
                        {
                            string param = attributes[i].Trim();
                            if (param.StartsWith(AttributeNullFlag.SearchString))
                            {
                                column.NullFlagAttribute = AttributeNullFlag;
                            }
                            else if (param.StartsWith(AttributeUnique.SearchString))
                            {
                                column.UniqueAttribute = AttributeUnique;
                            }
                            else if (param.StartsWith(AttributePrimary.SearchString))
                            {
                                column.IsPrimary = true;
                            }
                            else if (param.StartsWith(AttributeReference.SearchString))
                            {
                                string refer = param.Substring(param.IndexOf(' '));
                                string refTable;
                                string refCol = null;
                                if (refer.Contains("."))
                                {
                                    string[] parts = refer.Split('.');
                                    refTable = MakeItemName(parts[0], false);
                                    refCol = MakeItemName(parts[1]);
                                }
                                else
                                {
                                    refTable = MakeItemName(refer, false);
                                }
                                ForeignKey key = new ForeignKey()
                                {
                                    Key = column,
                                    RefTableString = refTable,
                                    RefColumnString = refCol
                                };
                                column.AddForeignKey(key);
                            }
                        }
                    }
                    table.AddColumn(column);
                }
                Tables.Add(table);
            }

            foreach (var table in Tables)
            {
                foreach (var column in table.Columns)
                {
                    if (column.ForeignKeys != null)
                    {
                        foreach (ForeignKey foreignKey in column.ForeignKeys)
                        {
                            Table refTable = Tables.FirstOrDefault(tbl => tbl.Name == foreignKey.RefTableString);
                            if (refTable == null)
                            {
                                throw new ArgumentException(
                                    $"Referenced table `{foreignKey.RefTableString}` not found. " +
                                    $"Referenced at {table.Name}.{column.Name}");
                            }
                            Column refColumn = refTable.Columns[0];
                            if (foreignKey.RefColumnString != null)
                            {
                                refColumn = refTable.Columns.First(
                                    column1 => column1.Name == foreignKey.RefColumnString);
                            }
                            foreignKey.RefTable = refTable;
                            foreignKey.RefColumn = refColumn;
                        }
                    }
                }
            }
            foreach (var table in Tables)
            {
                foreach (var column in table.Columns)
                {
                    if (!string.IsNullOrEmpty(Config.ColumnPrefix))
                    {
                        column.Name = Config.ColumnPrefix + column.Name;
                    }
                }
                if (!string.IsNullOrEmpty(Config.TablePrefix))
                {
                    table.Name = Config.TablePrefix + table.Name;
                }
            }
            return this;
        }

        public BasicGenerator Generate()
        {
            StringBuilder refsBuilder = new StringBuilder();
            sqlBuilder = new StringBuilder();

            htmlBuilder = new StringBuilder();
            if (Tables == null)
            {
                throw new ArgumentException("Null list of tables! First call Parse() method.");
            }

            foreach (Table table in Tables)
            {
                List<Column> primaryCols = new List<Column>();
                htmlBuilder.AppendLine($"<h2>{table.Name}</h2>")
                    .AppendLine("<table class=\"table\">")
                    .Append(MakeTableHeader())
                    .AppendLine()
                    .AppendLine("<tbody>");
                if (Config.AddDrops)
                {
                    sqlBuilder.Append(Drop).Append(" ").Append(table.Name).AppendLine(";").AppendLine();
                }
                sqlBuilder.Append(Create).Append(" ");
                if (Config.AddQuotas)
                {
                    sqlBuilder.Append(QuoteSign).Append(table.Name).Append(QuoteSign);
                }
                else
                {
                    sqlBuilder.Append(table.Name);
                }
                sqlBuilder.AppendLine(" (");

                for (var i = 0; i < table.Columns.Count; i++)
                {
                    Column column = table.Columns[i];
                    sqlBuilder.Append("    ").Append(column.Name).Append(" ");
                    if (column.IsPrimary)
                    {
                        primaryCols.Add(column);
                        htmlBuilder.AppendLine(
                            $"<tr>\n    <td>{column.Name}</td><td>Całkowity</td><td>Tak</td><td>Tak</td><td>Klucz podstawowy, automatyczna inkrementacja</td><td>Wewnętrzny identyfikator</td></tr>");

                        sqlBuilder.Append("INT");
                        if (Config.SetIntUnsigned)
                        {
                            sqlBuilder.Append(" ").Append(Unsigned);
                        }
                        if (column.IsAutoIncrement)
                        {
                            sqlBuilder.Append(" ").Append("AUTO_INCREMENT");
                        }
//                        if (column.NullFlagAttribute != null)
//                        {
                        sqlBuilder.Append(" ").Append(AttributeNullFlag.PrintName);
//                        }
                        if (Config.PrimaryKeyInline)
                        {
                            sqlBuilder.Append(" ").Append(AttributePrimary.PrintName);
                        }
                    }
                    else
                    {
                        htmlBuilder.AppendLine("<tr>").AppendLine($"    <td>{column.Name}</td>");
                        htmlBuilder.Append("    <td>").Append(GetHtmlColumnType(column)).AppendLine("</td>");
                        sqlBuilder.Append(column.Type.Pattern.PrintName);
                        if (column.Type.Size > 0)
                        {
                            if (column.Type.Pattern.HasAddistinalSize)
                            {
                                sqlBuilder.Append($"({column.Type.Size},{column.Type.AdditionalSize})");
                            }
                            else
                            {
                                sqlBuilder.Append($"({column.Type.Size})");
                            }
                        }
                        if (column.IsUnsigned)
                        {
                            sqlBuilder.Append(" ").Append(Unsigned);
                        }
                        var par = Config.NoAnswer;

                        htmlBuilder.AppendLine($"    <td>{par}</td>");
                        par = Config.NoAnswer;
                        if (NullAtributeApply(column, sqlBuilder))
                        {
                            par = Config.YesAnswer;
                        }

                        if (column.UniqueAttribute != null)
                        {
                            sqlBuilder.Append(" ").Append(column.UniqueAttribute.PrintName);
                            par = Config.YesAnswer;
                        }
                        htmlBuilder.AppendLine($"    <td>{par}</td>");
                        htmlBuilder.AppendLine("    <td></td>");
                        htmlBuilder.AppendLine("    <td></td>");
                        htmlBuilder.AppendLine("</tr>");
                    }

                    if (Config.ReferencesInline && column.ForeignKeys != null)
                    {
                        ForeignKey key = column.ForeignKeys[0];
                        sqlBuilder.Append(" ").Append(AttributeReference.PrintName).Append(" ")
                            .Append(key.RefTable.Name).Append("(").Append(key.RefColumn.Name).Append(")");
                    }
                    if (column.ForeignKeys != null && !Config.ReferencesInline)
                    {
                        foreach (ForeignKey foreignKey in column.ForeignKeys)
                        {
                            Table refTable = foreignKey.RefTable;
                            Column refColumn = foreignKey.RefColumn;
                            refsBuilder.Append(String.Format(AlterAddConstraint, table.Name,
                                $"{table.Name}_{refTable.Name}_{(byte) random.Next():X}",
                                column.Name, refTable.Name, refColumn.Name));
                            refsBuilder.Append(" ").Append(DeleteConstraintAction).AppendLine(";");
                        }
                    }
                    if (i < table.Columns.Count - 1 || (!Config.PrimaryKeyInline && primaryCols.Count > 0))
                    {
                        sqlBuilder.Append(",");
                    }
                    sqlBuilder.AppendLine();
                }
                if (!Config.PrimaryKeyInline && primaryCols.Count > 0)
                {
                    sqlBuilder.Append(Primary).Append("(");
                    for (var i = 0; i < primaryCols.Count; i++)
                    {
                        Column primaryCol = primaryCols[i];

                        sqlBuilder.Append(primaryCol.Name);
                        if (i < primaryCols.Count - 1)
                            sqlBuilder.Append(", ");
                    }
                    sqlBuilder.AppendLine(")");
                }


                sqlBuilder.Append(") ").Append(ParamsTable).AppendLine(";");
                sqlBuilder.AppendLine();
                htmlBuilder.AppendLine("</tbody>");
                htmlBuilder.AppendLine("</table>");
                htmlBuilder.AppendLine();
            }
            sqlBuilder.AppendLine();
            sqlBuilder.Append(refsBuilder.ToString());
            sqlBuilder.AppendLine();

            return this;
        }

        public string ToHtml()
        {
            if (htmlBuilder == null)
            {
                throw new ArgumentException("Null html string builder! First call Parse() and Generate() method.");
            }
            return htmlBuilder.ToString();
        }

        public string ToHtmlWithHeader()
        {
            StringBuilder html = new StringBuilder();
            html.AppendLine(
                "<html lang=\"pl\"><head><meta charset=\"UTF-8\"><link rel=\"stylesheet\" href=\"https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css\" /></head><body>");
            html.Append(ToHtml());
            //htmlBuilder.Clear();
            html.Append("\n</body></html>");
            return html.ToString();
        }

        public string ToSql()
        {
            if (sqlBuilder == null)
            {
                throw new ArgumentException("Null SQL string builder! First call Parse() and Generate() method.");
            }
            return sqlBuilder.ToString();
        }

        public string GetDeleteTablesSql()
        {
            return string.Join("\n", Enumerable.Reverse(Tables).Select(table => $"{Drop} {table.Name};").ToList());
        }

        public string GetInsertDataSql(string csvData)
        {
            var tabledDatas = csvData.Replace("\r", "").Split(tablesSeparators);
            var builder = new StringBuilder();
            foreach (var tabledData in tabledDatas)
            {
                if (string.IsNullOrEmpty(tabledData))
                {
                    continue;
                }
                var lines = tabledData.Split('\n');
                var tableName = lines[0].Trim(' ', ';').Trim();
                var table = Tables.FirstOrDefault(t => t.Name == tableName);
                if (table == null)
                {
                    Console.WriteLine($@"Table {tableName} skipped - not found");
                    lines[0] = "";
                    continue;
                }
                var skipId = Config.SkipIdInsterting && table.Columns.Count(c => c.IsPrimary) == 1;
                builder.Append("-- ").Append(tableName).Append(" | ");
                foreach (var column in table.Columns)
                {
                    if (skipId && column.IsPrimary)
                    {
                        continue;
                    }
                    builder.Append(column.Name).Append(", ");
                }
                builder.AppendLine();


                for (var index = 1; index < lines.Length; index++)
                {
                    var line = lines[index];
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }
                    builder.Append("INSERT INTO ").Append(table.Name).Append(" (");
                    var valueStrings = line.Trim('\n').Split(';');
                    var values = new List<string>();
                    foreach (var value in valueStrings)
                    {
                        var valueTrim = value.Trim();
                        valueTrim = valueTrim.Trim('"');
                        values.Add(valueTrim);
                    }
                    int offset = 0;
                    if (skipId)
                    {
                        offset = 1;
                    }
                    if (values.Count != table.Columns.Count - offset)
                    {
                        throw new ArgumentException(
                            $@"Incorrect number of columns in file. Was {values.Count}, expected {
                                    table.Columns.Count - offset
                                }. For table '{table.Name}' at line {index + 1}");
                    }
                    string vs = "";
                    for (int i = offset; i < table.Columns.Count; i++)
                    {
                        var column = table.Columns[i];
                        builder.Append(column.Name);
                        var value = values[i - offset];
                        if (value.ToLower() == "null")
                        {
                            if (column.ForeignKeys != null ||
                                (Config.NotNullByDefault ^ column.NullFlagAttribute != null))
                            {
                                throw new ArgumentException(
                                    $@"Incorrect value. Null value not allowed for non null column. "
                                    + $@"Occured for table '{table.Name}', column '{column.Name}' at line {index + 1}");
                            }
                            value = "NULL";
                        }

                        if (column.Type.Pattern.Type == EnumDataTypes.Text ||
                            column.Type.Pattern.Type == EnumDataTypes.Varchar && value.ToLower() != "null")
                        {
                            vs += "'" + value + "'";
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(value))
                            {
                                throw new ArgumentException(
                                    $@"Incorrect value. Empty value is not supported for non-string columns. "
                                    + $@"Occured for table '{table.Name}', column '{column.Name}' at line {index + 1}");
                            }
                            vs += value;
                        }


                        if (i < table.Columns.Count - 1)
                        {
                            vs += ", ";
                            builder.Append(", ");
                        }
                    }
                    builder.Append(") VALUES (");
                    builder.Append(vs);
//                    builder.Append(string.Join(", ", values));
                    builder.AppendLine(");");
                }
                builder.AppendLine();
            }
            return builder.ToString();
        }

        public string GetItemsList()
        {
            var columnBuilder = new StringBuilder();
            var tables = "";
            foreach (var table in Tables)
            {
                string beforeReplace = new LowerCamelFormatter().FormatName(table.RawName, DummyConfiguration);
                string constTableName = new UnderscoreFormatter().FormatName(table.RawName, DummyConfiguration);

                tables += string.Format(Config.ReplacementFormat, "", constTableName.ToUpper(), beforeReplace);
                foreach (var column in table.Columns)
                {
                    beforeReplace = new LowerCamelFormatter().FormatName(column.RawName, DummyConfiguration);
                    string constName = new UnderscoreFormatter().FormatName(column.RawName, DummyConfiguration);

                    columnBuilder.AppendFormat(Config.ReplacementFormat, constTableName.ToUpper() + "_",
                        constName.ToUpper(),
                        beforeReplace);
                }
            }
            columnBuilder.AppendLine();
            var items = new Dictionary<string, string>();
            foreach (var table in Tables)
            {
                string beforeReplace = new LowerCamelFormatter().FormatName(table.RawName, DummyConfiguration);
                items[beforeReplace] = table.Name;
                foreach (var column in table.Columns)
                {
                    beforeReplace = new LowerCamelFormatter().FormatName(column.RawName, DummyConfiguration);
                    items[beforeReplace] = column.Name;
                }
            }
            foreach (var pair in items)
            {
                columnBuilder.AppendFormat("'{0}' => '{1}',", pair.Key, pair.Value).AppendLine();
            }

            return tables + "\n\n" + columnBuilder.ToString();
        }

        public string GetHtmlColumnType(Column col)
        {
            bool size = false;
            string name;
            switch (col.Type.Pattern.Type)
            {
                case EnumDataTypes.Int:
                    name = "Całkowity";
                    break;
                case EnumDataTypes.SmallInt:
                case EnumDataTypes.ByteInt:
                    name = "Całkowity";
                    size = true;
                    break;
                case EnumDataTypes.Boolean:
                    name = "Logiczny";
                    break;
                case EnumDataTypes.Char:
                case EnumDataTypes.Varchar:
                    name = "Znakowy";
                    size = true;
                    break;
                case EnumDataTypes.Text:
                    name = "Tekstowy";
                    break;
                case EnumDataTypes.Decimal:
                    return $"Dziesiętny (max. {col.Type.Size},{col.Type.AdditionalSize})";
                case EnumDataTypes.DateTime:
                case EnumDataTypes.TimeStamp:
                    name = "Data i czas";
                    break;
                case EnumDataTypes.Date:
                    name = "Data";
                    break;
                default:
                    name = col.Type.Pattern.PrintName + "_#";
                    break;
            }
            if (size)
            {
                name += $" (max. {col.Type.Size})";
            }
            return name;
        }

        public static string GetNameWithFirstUpper(string name)
        {
            string f = name.Substring(0, 1);
            return $"{f.ToUpper()}{name.Substring(1)}";
        }

        protected List<DataTypePattern> OnTypePatternsGenerationEvent(List<DataTypePattern> list)
        {
            list.Add(new DataTypePattern(EnumDataTypes.Varchar, "v", TypeVarchar, true));
            return list;
        }

        private bool NullAtributeApply(Column column, StringBuilder builder)
        {
            bool addNotNull = Config.NotNullByDefault;
            if (column.NullFlagAttribute != null)
            {
                addNotNull = !addNotNull;
            }

            if (column.ForeignKeys != null)
            {
                addNotNull = column.NullFlagAttribute == null;
            }
            if (addNotNull)
            {
                builder.Append(" ").Append(AttributeNullFlag.PrintName);
                return true;
            }
            return false;
        }

        protected string MakeTableHeader()
        {
            string header = "<thead>\r\n" +
                            "<tr>\r\n" +
                            $"    <th>{Config.TableColumnName}</th>" +
                            $"<th>{Config.TypeColumnName}</th>" +
                            $"<th>{Config.NecessityColumnName}</th>" +
                            $"<th>{Config.UniqueColumnName}</th>" +
                            $"<th>{Config.AttrsLeftColumnName}</th>" +
                            $"<th>{Config.DescriptionColumnName}</th>" +
                            "</tr>\r\n" +
                            "</thead>";
            return header;
        }
    }
}