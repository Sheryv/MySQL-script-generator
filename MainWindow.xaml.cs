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

        private const string Version = "0.8";
        private const string TestTable =
            "+Users \r\nLogin, v 40,u \r\nEmail,varchar(255),u \r\nPassword,v 64 \r\nRegistered, datetime, n \r\nLastLogin, decimal( 1 3) \r\nLastLoginInApp,datetime \r\nLastSynchronization, text \r\nSecurityLevel,tinyint 2 \r\nPermissions, int \r\nPhone Number,boolean \r\n\r\n\r\n+Roles\r\nName, v 128\r\nDesc, longtext,n\r\n\r\n+UsersRoles, noid\r\nUserId, int, ref Users.Id, pk\r\nRoleId, int, ref Roles.Id, pk\r\n\r\n";

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