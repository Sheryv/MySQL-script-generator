using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SQL_Generator_WPF
{
    public partial class MainWindow : Window
    {
        private const string TestTable =
            "+Users \r\nLogin, v 40,u \r\nEmail,v 255,u \r\nPassword,v 64 \r\nRegistered, datetime \r\nLastLogin, datetime \r\nLastLoginInApp,datetime \r\nLastSynchronization, datetime \r\nSecurityLevel,tinyint 2 \r\nPermissions, int \r\nPhone Number,v 20 ";

        private const string UpperCaseName = "UpperCamelCase";
        private const string LowerCaseName = "lowerCamelCase";

        private const string TableColumnName = "Nazwa pola";
        private const string TypeColumnName = "Typ pola";
        private const string NecessityColumnName = "Czy pole jest wymagane";
        private const string UniqueColumnName = "Czy wartość jest unikatowa";
        private const string DescriptionColumnName = "Opis";
        private const string AttrsLeftColumnName = "Pozostałe atrybuty";

        private const string TableColumnGeneral = "Nazwa tabeli";
        private const string TypeColumnGeneral = "Typ";

        private string fileDirectory;
        private bool upperCamelCase = true;
        private bool addIdWithPrimaryAuto = true;
        private const string NoAnswer = "Nie";
        private const string YesAnswer = "Tak";


        public MainWindow()
        {
            InitializeComponent();
            modeComboBox.Items.Add(UpperCaseName);
            modeComboBox.Items.Add(LowerCaseName);
            modeComboBox.SelectedIndex = 0;
        }

        private void OpenBtn_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openPicker = new Microsoft.Win32.OpenFileDialog();
            openPicker.DefaultExt = ".txt";
            openPicker.Filter = "Text files|*.txt";
            Nullable<bool> result = openPicker.ShowDialog();
            if (result == true)
            {
                addressTxt.Text = openPicker.FileName.ToString();
                fileDirectory = openPicker.FileName.ToString();
            }
            inputTextBox.Text = File.ReadAllText(fileDirectory);
        }

        private void generateBtn_Click(object sender, RoutedEventArgs e)
        {
            string txt = inputTextBox.Text;
            if (txt.Length == 0)
            {
                return;
            }
            outPutTextBox.Text = Generate(txt);
        }


        private string Generate(string txt)
        {
            TextBox output = outPutTextBox;

            StringBuilder builder = new StringBuilder();
            StringBuilder html = new StringBuilder();
            string[] tables = txt.Split('+');
            foreach (string tbl in tables)
            {
                string s = tbl.Trim();
                if (s.Length <= 3)
                {
                    continue;
                }
                string[] lines = s.Split('\n');
                string table = lines[0].Trim();
                html.AppendLine($"<h2>{table}</h2>");
                html.Append("<table class=\"table\">\n").Append(MakeTableHeader());
                html.Append("\n<tbody>\n");

                //tables
                builder.Append("CREATE TABLE ").Append(table).Append(" (\n");
                if (addIdWithPrimaryAuto)
                {
                    builder.Append("    ");
                    string name;
                    if (upperCamelCase)
                        name = "Id";
                    else
                        name = "id";
                    builder.Append(name);
                    html.Append("<tr>\n" +
                                $"    <td>{name}</td><td>Całkowity</td><td>Tak</td><td>Tak</td><td>Klucz podstawowy, automatyczna inkrementacja</td><td>Wewnętrzny identyfikator</td></tr>\n");
                    builder.Append(" INT NOT NULL PRIMARY KEY AUTO_INCREMENT,\n");
                }
                for (var lineIndex = 0; lineIndex < lines.Length; lineIndex++)
                {
                    if (lineIndex == 0)
                    {
                        continue;
                    }
                    string line = lines[lineIndex].Trim();
                    if (line.Length <= 2)
                    {
                        continue;
                    }
                    html.Append("<tr>\n");

                    string[] attrs = line.Split(',');
                    string field = MakeFieldName(attrs[0].Trim());
                    builder.Append("    ").Append(field).Append(" ");
                    html.AppendLine($"    <td>{field}</td>");
                    if (attrs.Length <= 1)
                    {
                        return "Error: Type not specified for line: " + lineIndex + " in table " + table;
                    }
                    string attribute = attrs[1].Trim();
                    string type = null;
                    string htmlType = null;
                    if (attribute.StartsWith("v "))
                    {
                        string o;
                        int size = GetTypeMaxSize(attribute, out o);
                        if (size >= 0)
                        {
                            type = $"VARCHAR({size})";
                            htmlType = $"Znakowy (max. {size})";
                        }
                        else
                        {
                            return "\nError parsing size of type";
                        }
                    }
                    else if (attribute.Contains(" "))
                    {
                        string tempType = "";
                        int size = GetTypeMaxSize(attribute, out tempType);
                        if (size >= 0)
                        {
                            type = $"{tempType}({size})";
                            if (attribute.ToLower().Contains("int"))
                            {
                                htmlType = $"Całkowity (max. {size})";
                            }
                        }
                        else
                        {
                            return "\nError parsing size of type";
                        }
                    }
                    else
                    {
                        if (attribute.ToLower() == "int")
                        {
                            htmlType = "Całkowity";
                        }
                        else if (attribute.ToLower().Contains("int"))
                        {
                            string tempSize = attribute.Substring(attribute.IndexOf('('), attribute.Length - 2);
                            htmlType = $"Całkowity (max. {tempSize})";
                        }
                        else
                        {
                            htmlType = $"{attribute}_#";
                        }
                        //proper type
                        type = attribute.ToUpper();
                    }
                    if (htmlType == null)
                    {
                        return "Error: Incorrect type for line: " + lineIndex + " in table " + table;
                    }
                    html.AppendLine($"    <td>{htmlType}</td>");
                    builder.Append(type);
                    string necessity = NoAnswer;
                    string htmlAttrs = "";
                    string htmlUnique = NoAnswer;
                    if (attrs.Length > 2)
                    {
                        int leftParamsIndex = 0;
                        if (!attrs[2].Contains("n"))
                        {
                            builder.Append(" NOT NULL");
                            necessity = YesAnswer;
                            leftParamsIndex = 2;
                        }
                        else
                        {
                            leftParamsIndex = 3;
                        }
                        if (attrs.Length > 2)
                        {
                            string param = ProcessLeftParams(leftParamsIndex, attrs, out htmlAttrs, out htmlUnique);
                            if (param.Length > 0)
                                builder.Append(" ").Append(param);
                        }
                    }
                    else
                    {
                        necessity = YesAnswer;
                        builder.Append(" NOT NULL");
                    }
                    html.AppendLine($"    <td>{necessity}</td>")
                        .AppendLine($"    <td>{htmlUnique}</td>")
                        .AppendLine($"    <td>{htmlAttrs}</td>")
                        .AppendLine("    <td></td>");
                    html.AppendLine("</tr>");
                    if (lineIndex < lines.Length - 1)
                        builder.Append(",");
                    builder.Append("\n");
                }
                builder.AppendLine(");");
                builder.AppendLine();
                builder.AppendLine();
                html.AppendLine("</tbody>");
                html.AppendLine("</table>");
                html.AppendLine();
            }
            tableOutTextBox.Text = html.ToString();
            ShowHtml(html);
            return builder.ToString();
        }

        private void ShowHtml(StringBuilder html)
        {
            string inside = html.ToString();
            html = new StringBuilder();
            html.AppendLine("<html lang=\"pl\"><head><meta charset=\"UTF-8\"><link rel=\"stylesheet\" href=\"https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css\" /></head><body>");
            html.Append(inside);
            html.Append("\n</body></html>");
            File.WriteAllText("table.htm", html.ToString());
        }

        private void OpenFile(string path)
        {
            try
            {
                System.Diagnostics.Process.Start(path);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Error while openning file!");
            }
        }

        private string MakeFieldName(string s)
        {
            string[] parts = s.Trim().Split(' ');
            string field = "";
            for (var i = 0; i < parts.Length; i++)
            {
                string part = parts[i];
                var letter = part[0].ToString();
                if (upperCamelCase || i > 0)
                {
                    letter = letter.ToUpper();
                }
                else
                {
                    letter = letter.ToLower();
                }
                field += letter + part.Substring(1);
            }
            return field;
        }

        private string ProcessLeftParams(int index, string[] attrs, out string htmlAttrs, out string htmlUnique)
        {
            string a = attrs[index].Trim();
            htmlAttrs = "";
            htmlUnique = NoAnswer;
            if (a.Contains("u"))
            {
                htmlUnique = YesAnswer;
                return "UNIQUE";
            }
            return "";
        }

        private int GetTypeMaxSize(string attr, out string type)
        {
            type = attr.Trim();
            int index = type.LastIndexOf(' ');
            string size = type.Substring(index);
            int sizeInt;
            index = type.IndexOf(' ');
            type = type.Substring(0, index).ToUpper();
            if (int.TryParse(size, out sizeInt))
            {
                return sizeInt;
            }
            else
            {
                return -1;
            }
        }

        private string MakeTableHeader()
        {
            string header = "<thead>\r\n" +
                            "<tr>\r\n" +
                            $"    <th>{TableColumnName}</th>" +
                            $"<th>{TypeColumnName}</th>" +
                            $"<th>{NecessityColumnName}</th>" +
                            $"<th>{UniqueColumnName}</th>" +
                            $"<th>{AttrsLeftColumnName}</th>" +
                            $"<th>{DescriptionColumnName}</th>" +
                            "</tr>\r\n" +
                            "</thead>";
            return header;
        }

        private void exampleButton_Click(object sender, RoutedEventArgs e)
        {
            inputTextBox.Text = TestTable;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists("table.htm"))
            {
                OpenFile("table.htm");
            }
        }

        private void modeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            upperCamelCase = (string) modeComboBox.SelectedItem == UpperCaseName;
        }
    }
}