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
using Microsoft.Win32;
using SQL_Generator_WPF.Coverter;

namespace SQL_Generator_WPF
{
    public partial class MainWindow : Window
    {

        private const string Version = "0.7";
        private const string TestTable =
            "+Users \r\nLogin, v 40,u \r\nEmail,varchar(255),u \r\nPassword,v 64 \r\nRegistered, datetime, n \r\nLastLogin, decimal( 1 3) \r\nLastLoginInApp,datetime \r\nLastSynchronization, text \r\nSecurityLevel,tinyint 2 \r\nPermissions, int \r\nPhone Number,boolean ";

        private const string UpperCaseName = "UpperCamelCase";
        private const string LowerCaseName = "lowerCamelCase";

        private string fileDirectory;


        public MainWindow()
        {
            InitializeComponent();
            modeComboBox.Items.Add(UpperCaseName);
            //modeComboBox.Items.Add(LowerCaseName);
            modeComboBox.SelectedIndex = 0;
            versionTextBlock.Text = $"Version: {Version}";
        }
//
//        protected string MakeTableHeader()
//        {
//            string header = "<thead>\r\n" +
//                            "<tr>\r\n" +
//                            $"    <th>{TableColumnName}</th>" +
//                            $"<th>{TypeColumnName}</th>" +
//                            $"<th>{NecessityColumnName}</th>" +
//                            $"<th>{UniqueColumnName}</th>" +
//                            $"<th>{AttrsLeftColumnName}</th>" +
//                            $"<th>{DescriptionColumnName}</th>" +
//                            "</tr>\r\n" +
//                            "</thead>";
//            return header;
//        }

        private void OpenBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openPicker = new OpenFileDialog();
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
            catch (Exception exception)
            {
                MessageBox.Show("Error", exception.Message);
                throw;
            }
        }

        private void generateBtn_Click(object sender, RoutedEventArgs e)
        {
            string txt = inputTextBox.Text;
            if (txt.Length == 0)
            {
                return;
            }
          
                BasicGenerator generator = new BasicGenerator(BasicGenerator.DummyConfiguration);
                outPutTextBox.Text= generator.Parse(txt).Generate().ToSql();
                string h = generator.ToHtmlWithHeader();
                WriteHtml(h);
                tableOutTextBox.Text = h;
             try
            { }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Error while generating");
            }
            //outPutTextBox.Text = Generate(txt);
        }


     /*   private string Generate(string txt)
        {
            TextBox output = outPutTextBox;
            List<string> tableList = new List<string>();
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
                tableList.Add(table);
                html.Append("<table class=\"table\">\n").Append(MakeTableHeader());
                html.Append("\n<tbody>\n");

                if (addDrops)
                {
                    builder.Append("DROP TABLE IF EXIST ").Append(table).AppendLine(";").AppendLine();
                }

                //tables
                builder.Append("CREATE TABLE ").Append(table).AppendLine(" (");
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

                            string param = ProcessLeftParams(2, attrs, out htmlAttrs, out htmlUnique, out necessity);
                            if (param.Length > 0)
                                builder.Append(" ").Append(param);
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
            WriteHtml(html.ToString());
            return builder.ToString();
        }

        private string MakeFieldName(string rawColumn)
        {
            string[] parts = rawColumn.Trim().Split(' ');
            string columnName = "";
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
                columnName += letter + part.Substring(1);
            }
            return columnName;
        }
*/
        private void WriteHtml(String html)
        {
            File.WriteAllText("table.htm", html);
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

       
/*
        private string ProcessLeftParams(int searchStartIndex, string[] attrs, out string htmlAttrs, out string htmlUnique, out string necessity)
        {
            string output = "";
            htmlUnique = NoAnswer;
            necessity = NoAnswer;
            for (int i = searchStartIndex; i < attrs.Length; i++)
            {
                string attr = attrs[searchStartIndex].Trim();
                if (!attr.StartsWith("n"))
                {
                    output+=" NOT NULL";
                    necessity = YesAnswer;
                }
                else if (attr.StartsWith("u"))
                {
                    htmlUnique = YesAnswer;
                    output+= " UNIQUE";
                }
                else if (attr.StartsWith("ref"))
                {
                    string key = attr.Substring(3);
                    string[] names = key.Split('.');

                }
            }

            htmlAttrs = "";
            return output;
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

      */
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
            BasicGenerator.DummyConfiguration.UpperCamelCase = (string) modeComboBox.SelectedItem == UpperCaseName;
        }

        private void Hyperlink_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Hyperlink h = (Hyperlink)sender;
                System.Diagnostics.Process.Start(h.NavigateUri.ToString());
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Error while openning url!");
            }
        }

        private void AddDropsChk_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox check = (CheckBox) sender;
            BasicGenerator.DummyConfiguration.AddDrops =  (check.IsChecked != null && check.IsChecked != false);
        }

        private void SetDefaultUnsigned_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox check = (CheckBox)sender;
            BasicGenerator.DummyConfiguration.SetIntUnsigned = (check.IsChecked != null && check.IsChecked != false);
        }

        private void QuotesChk_OnChecked(object sender, RoutedEventArgs e)
        {
            CheckBox check = (CheckBox)sender;
            BasicGenerator.DummyConfiguration.AddQuotas = (check.IsChecked != null && check.IsChecked != false);
        }

        private void ReadBtn_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                inputTextBox.Text = File.ReadAllText(addressTxt.Text);
            }
            catch (Exception exception)
            {
                
                throw;
            }
        }
    }
}