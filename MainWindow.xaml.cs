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
        private const string Version = "0.10";

        private const string TestTable =
                "+Users \r\nLogin, v 40,u \r\nEmail,varchar(255),u \r\nPassword,v 64 \r\nRegistered, datetime, n \r\nLastLogin, decimal( 1 3) \r\nLastLoginInApp,datetime \r\nLastSynchronization, text \r\nSecurityLevel,tinyint 2 \r\nPermissions, int \r\nPhone Number,boolean \r\n\r\n\r\n+Roles, noid\r\nIdR, int, pk\r\nName, v 128\r\nDesc, longtext,n\r\n\r\n+Users Roles, noid\r\nUserId, int, ref Users, pk\r\nRoleId, int, ref Roles.IdR, pk\r\n\r\n"
            ;

        private string fileDirectory;


        public MainWindow()
        {
            InitializeComponent();
            foreach (var value in GeneratorConfiguration.NamingTypesNames.Values)
            {
                modeComboBox.Items.Add(value);
            }
            modeComboBox.SelectedItem = GeneratorConfiguration.NamingTypesNames[NamingTypes.Mixed];
            versionTextBlock.Text = $"Version: {Version}";
            longIdNamesChk.IsChecked = BasicGenerator.DummyConfiguration.AddLongNameForColumnId;
            refernceInlineChk.IsChecked = BasicGenerator.DummyConfiguration.ReferencesInline;
            primaryKeyInline.IsChecked = BasicGenerator.DummyConfiguration.PrimaryKeyInline;
            notnullChk.IsChecked = BasicGenerator.DummyConfiguration.NotNullByDefault;
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
                outPutTextBox.Text = generator.Parse(txt).Generate().ToSql();
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
            var type = GeneratorConfiguration.NamingTypesNames
                .First(pair => Equals(pair.Value, modeComboBox.SelectedItem)).Key;
            BasicGenerator.DummyConfiguration.NamingConvention = type;
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

        private void columnPrefix_TextChanged(object sender, TextChangedEventArgs e)
        {
            BasicGenerator.DummyConfiguration.ColumnPrefix = ((TextBox) sender).Text;
        }

        private void tablePrefixTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            BasicGenerator.DummyConfiguration.TablePrefix = ((TextBox) sender).Text;
        }
    }
}