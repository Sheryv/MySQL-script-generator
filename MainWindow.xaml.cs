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
using MySql.Data.MySqlClient;
using SQL_Generator_WPF.Converter;

namespace SQL_Generator_WPF
{
    public partial class MainWindow : Window
    {
        private const string Version = "0.12";

        private const string TestTable =
                "+Users \r\nLogin, v 40,u \r\nEmail,varchar(255),u \r\nPassword,v 64 \r\nRegistered, datetime, n \r\nLastLogin, decimal(3 2) \r\nLastLoginInApp,datetime \r\nLastSynchronization, text \r\nSecurityLevel,tinyint 2 \r\nPermissions, int \r\nPhone Number,boolean \r\n\r\n\r\n+Roles, noid\r\nIdR, int, pk\r\nName, v 128\r\nDescription, text,n\r\n\r\n+Users Roles, noid\r\nUserId, int, ref Users, pk\r\nRoleId, int, ref Roles.IdR, pk\r\n\r\n"
            ;

        private string fileDirectory;
        private string insertFile;

        private AppConfiguration AppConfiguration { get; } = new AppConfiguration()
        {
            DbHost = "localhost",
            DbName = "test",
            DbPassword = "",
            DbUsername = "root"
        };

        public MainWindow()
        {
            InitializeComponent();
            foreach (var value in GeneratorConfiguration.NamingFormatters.Keys)
            {
                columnNamingCb.Items.Add(value);
                tableNamingCb.Items.Add(value);
            }
            columnNamingCb.SelectedItem =
                GeneratorConfiguration.NamingFormatters.First(n =>
                    n.Value == BasicGenerator.DummyConfiguration.ColumnFormatter).Key;
            tableNamingCb.SelectedItem =
                GeneratorConfiguration.NamingFormatters.First(n =>
                    n.Value == BasicGenerator.DummyConfiguration.TableFormatter).Key;
            versionTextBlock.Text = $"Version: {Version}";
            longIdNamesChk.IsChecked = BasicGenerator.DummyConfiguration.AddLongNameForColumnId;
            refernceInlineChk.IsChecked = BasicGenerator.DummyConfiguration.ReferencesInline;
            primaryKeyInline.IsChecked = BasicGenerator.DummyConfiguration.PrimaryKeyInline;
            notnullChk.IsChecked = BasicGenerator.DummyConfiguration.NotNullByDefault;
            skipIdInstertingChk.IsChecked = BasicGenerator.DummyConfiguration.SkipIdInsterting;
            tbReplacementFormat.Text = BasicGenerator.DummyConfiguration.ReplacementFormat;
            inputTextBox.Text = TestTable;
        }


        private void OpenBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openPicker = new OpenFileDialog();
                openPicker.DefaultExt = ".txt";
                openPicker.Filter = "Text files|*.txt;*.md;*.rtf|All files|*.*";
                bool? result = openPicker.ShowDialog();
                if (result == true)
                {
                    addressTxt.Text = openPicker.FileName.ToString();
                    fileDirectory = openPicker.FileName.ToString();
                    inputTextBox.Text = File.ReadAllText(fileDirectory);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Error while openning file!");
            }
        }

        private void generateBtn_Click(object sender, RoutedEventArgs e)
        {
            string txt = inputTextBox.Text;
            if (txt.Length == 0)
            {
                return;
            }

            try
            {
                BasicGenerator generator = new BasicGenerator(BasicGenerator.DummyConfiguration);
                outPutTextBox.Text = generator.Parse(txt, tbReplacement.Text).Generate().ToSql();
                string h = generator.ToHtmlWithHeader();
                WriteHtml(h);
                tableOutTextBox.Text = h;
            }
            catch (Exception exception)
            {
                MessageBox.Show($"{exception.Message}\n\n{exception.StackTrace}", "Error while generating");
            }
            //outPutTextBox.Text = Generate(txt);
        }

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


        private void exampleButton_Click(object sender, RoutedEventArgs e)
        {
            inputTextBox.Text = TestTable;
            Crypto.Test();
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
            if (columnNamingCb.SelectedItem != null)
            {
                BasicGenerator.DummyConfiguration.ColumnFormatter =
                    GeneratorConfiguration.NamingFormatters[(string) columnNamingCb.SelectedItem];
            }
            if (tableNamingCb.SelectedItem != null)
            {
                BasicGenerator.DummyConfiguration.TableFormatter =
                    GeneratorConfiguration.NamingFormatters[(string) tableNamingCb.SelectedItem];
            }
        }

        private void Hyperlink_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Hyperlink h = (Hyperlink) sender;
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
            BasicGenerator.DummyConfiguration.AddDrops = (check.IsChecked != null && check.IsChecked != false);
        }

        private void SetDefaultUnsigned_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox check = (CheckBox) sender;
            BasicGenerator.DummyConfiguration.SetIntUnsigned = (check.IsChecked != null && check.IsChecked != false);
        }

        private void QuotesChk_OnChecked(object sender, RoutedEventArgs e)
        {
            CheckBox check = (CheckBox) sender;
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
                MessageBox.Show($"{exception.Message}\n\n{exception.StackTrace}", "Error while reading file");
            }
        }

        private void LongIdNamesChk_OnChecked(object sender, RoutedEventArgs e)
        {
            CheckBox check = (CheckBox) sender;
            BasicGenerator.DummyConfiguration.AddLongNameForColumnId =
                (check.IsChecked != null && check.IsChecked != false);
        }

        private void RefernceInlineChk_OnChecked(object sender, RoutedEventArgs e)
        {
            CheckBox check = (CheckBox) sender;
            BasicGenerator.DummyConfiguration.ReferencesInline = (check.IsChecked != null && check.IsChecked != false);
        }

        private void PrimaryKeyInline_OnChecked(object sender, RoutedEventArgs e)
        {
            CheckBox check = (CheckBox) sender;
            BasicGenerator.DummyConfiguration.PrimaryKeyInline = (check.IsChecked != null && check.IsChecked != false);
        }

        private void NotnullChk_OnChecked(object sender, RoutedEventArgs e)
        {
            CheckBox check = (CheckBox) sender;
            BasicGenerator.DummyConfiguration.NotNullByDefault = (check.IsChecked != null && check.IsChecked != false);
        }

        private void SkipIdInstertingChk_OnChecked(object sender, RoutedEventArgs e)
        {
            CheckBox check = (CheckBox)sender;
            BasicGenerator.DummyConfiguration.SkipIdInsterting = (check.IsChecked != null && check.IsChecked != false);
        }

        private void SkipIdInstertingChk_OnUnchecked(object sender, RoutedEventArgs e)
        {
            CheckBox check = (CheckBox)sender;
            BasicGenerator.DummyConfiguration.SkipIdInsterting = (check.IsChecked != null && check.IsChecked != false);
        }

        private void columnPrefix_TextChanged(object sender, TextChangedEventArgs e)
        {
            BasicGenerator.DummyConfiguration.ColumnPrefix = ((TextBox) sender).Text;
        }

        private void tablePrefixTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            BasicGenerator.DummyConfiguration.TablePrefix = ((TextBox) sender).Text;
        }

        private void DbConnectionBtn_OnClick(object sender, RoutedEventArgs e)
        {
        }

        private void DbExecuteBtn_OnClick(object sender, RoutedEventArgs e)
        {
            string connstring =
                $"Server={AppConfiguration.DbHost}; database={AppConfiguration.DbName}; UID={AppConfiguration.DbUsername}; password={AppConfiguration.DbPassword}";
            try
            {
                var connection = new MySqlConnection(connstring);
                connection.Open();
                MySqlScript script = new MySqlScript(connection, outPutTextBox.Text);
                script.Delimiter = ";";
                var c = script.Execute();
                connection.Close();
                MessageBox.Show($"Script executed with code: {c}", "Success");
            }
            catch (Exception exception)
            {
                MessageBox.Show($"{exception.Message}\n\n{exception.StackTrace}", "Error while executing script!");
            }
        }

        private void DbDeleteBtn_OnClick(object sender, RoutedEventArgs e)
        {
            string txt = inputTextBox.Text;
            BasicGenerator generator = new BasicGenerator(BasicGenerator.DummyConfiguration);
            outPutTextBox.Text = generator.Parse(txt, tbReplacement.Text).Generate().GetDeleteTablesSql();
        }

        private void DbInsertBtn_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (insertFile == null)
                {
                    DbInsertFileBtn_OnClick(sender, e);
                }
                var data = File.ReadAllText(insertFile);
                BasicGenerator generator = new BasicGenerator(BasicGenerator.DummyConfiguration);
                outPutTextBox.Text = generator.Parse(inputTextBox.Text, tbReplacement.Text).GetInsertDataSql(data);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Error while openning file!");
            }
        }

        private void DbInsertFileBtn_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openPicker = new OpenFileDialog();
                openPicker.DefaultExt = ".txt";
                openPicker.Filter = "CSV files|*.csv|All files|*.*";
                bool? result = openPicker.ShowDialog();
                if (result == true)
                {
                    insertFile = openPicker.FileName;
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Error while openning file!");
            }
        }


        private void TbReplacementFormat_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            BasicGenerator.DummyConfiguration.ReplacementFormat = tbReplacementFormat.Text;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            BasicGenerator generator = new BasicGenerator(BasicGenerator.DummyConfiguration);
            markdownOutput.Text = generator.Parse(inputTextBox.Text, tbReplacement.Text).GetItemsList();
        }
    }
}